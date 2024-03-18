#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Profiling;
using Unity.Profiling;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using GCHandle = System.Runtime.InteropServices.GCHandle;
using UnityEngine;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;
	using Pathfinding.Drawing;
	using Pathfinding.RVO;
	using Pathfinding.PID;
	using Pathfinding.Util;
	using Unity.Burst.Intrinsics;

	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[BurstCompile]
	public partial struct FollowerControlSystem : ISystem {
		EntityQuery entityQueryPrepare;
		EntityQuery entityQueryControl;
		EntityQuery entityQueryOffMeshLink;
		EntityQuery entityQueryOffMeshLinkCleanup;
		ComponentTypeHandle<LocalTransform> LocalTransformTypeHandleRO;
		ComponentTypeHandle<MovementState> MovementStateTypeHandleRW;
		ComponentTypeHandle<MovementState> MovementStateTypeHandleRO;
		ComponentTypeHandle<AgentCylinderShape> AgentCylinderShapeTypeHandleRO;
		ComponentTypeHandle<DestinationPoint> DestinationPointTypeHandleRO;
		ComponentTypeHandle<AgentMovementPlane> AgentMovementPlaneTypeHandleRO;
		ComponentTypeHandle<ManagedState> ManagedStateTypeHandleRW;
		ComponentTypeHandle<MovementSettings> MovementSettingsTypeHandleRO;
		ComponentTypeHandle<ResolvedMovement> ResolvedMovementHandleRO;
		RedrawScope redrawScope;
		GCHandle entityManagerHandle;

		public void OnCreate (ref SystemState state) {
			entityManagerHandle = GCHandle.Alloc(state.EntityManager);

			LocalTransformTypeHandleRO = state.GetComponentTypeHandle<LocalTransform>(true);
			MovementStateTypeHandleRW = state.GetComponentTypeHandle<MovementState>(false);
			MovementStateTypeHandleRO = state.GetComponentTypeHandle<MovementState>(true);
			AgentCylinderShapeTypeHandleRO = state.GetComponentTypeHandle<AgentCylinderShape>(true);
			DestinationPointTypeHandleRO = state.GetComponentTypeHandle<DestinationPoint>(true);
			AgentMovementPlaneTypeHandleRO = state.GetComponentTypeHandle<AgentMovementPlane>(true);
			MovementSettingsTypeHandleRO = state.GetComponentTypeHandle<MovementSettings>(true);
			ResolvedMovementHandleRO = state.GetComponentTypeHandle<ResolvedMovement>(true);
			// Need to bypass the T : unmanaged check in state.GetComponentTypeHandle
			ManagedStateTypeHandleRW = state.EntityManager.GetComponentTypeHandle<ManagedState>(false);
			redrawScope = DrawingManager.GetRedrawScope();

			entityQueryPrepare = AIMoveSystem.EntityQueryPrepareMovement().WithAll<SimulateMovement, SimulateMovementRepair>().Build(ref state);
			entityQueryControl = state.GetEntityQuery(
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadOnly<AgentMovementPlane>(),
				ComponentType.ReadOnly<DestinationPoint>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadOnly<MovementStatistics>(),
				ComponentType.ReadWrite<ManagedState>(),
				ComponentType.ReadOnly<MovementSettings>(),
				ComponentType.ReadOnly<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementControl>(),
				ComponentType.Exclude<AgentOffMeshLinkTraversal>(),
				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementControl>()
				);

			entityQueryOffMeshLink = state.GetEntityQuery(
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadWrite<AgentMovementPlane>(),
				ComponentType.ReadOnly<DestinationPoint>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadOnly<MovementStatistics>(),
				ComponentType.ReadWrite<ManagedState>(),
				ComponentType.ReadWrite<MovementSettings>(),
				ComponentType.ReadOnly<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementControl>(),
				ComponentType.ReadWrite<AgentOffMeshLinkTraversal>(),
				ComponentType.ReadWrite<ManagedAgentOffMeshLinkTraversal>(),
				ComponentType.ReadOnly<SimulateMovement>()
				);

			entityQueryOffMeshLinkCleanup = state.GetEntityQuery(
				// ManagedAgentOffMeshLinkTraversal is a cleanup component.
				// If it exists, but the AgentOffMeshLinkTraversal does not exist,
				// then the agent must have been destroyed while traversing the off-mesh link.
				ComponentType.ReadWrite<ManagedAgentOffMeshLinkTraversal>(),
				ComponentType.Exclude<AgentOffMeshLinkTraversal>()
				);
		}

		public void OnDestroy (ref SystemState state) {
			redrawScope.Dispose();
			entityManagerHandle.Free();
		}

		public void OnUpdate (ref SystemState systemState) {
			if (AstarPath.active == null) return;

			var simulator = RVOSimulator.active?.GetSimulator();
			var commandBuffer = new EntityCommandBuffer(systemState.WorldUpdateAllocator);

			// First check if we have a simulator. If not, we can skip handling RVO components
			if (simulator != null) {
				Profiler.BeginSample("AddRVOComponents");
				foreach (var(managedState, entity) in SystemAPI.Query<ManagedState>().WithNone<RVOAgent>().WithEntityAccess()) {
					if (managedState.enableLocalAvoidance) {
						commandBuffer.AddComponent<RVOAgent>(entity, managedState.rvoSettings);
					}
				}
				Profiler.EndSample();
				Profiler.BeginSample("CopyRVOSettings");
				foreach (var(managedState, rvoAgent, entity) in SystemAPI.Query<ManagedState, RefRW<RVOAgent> >().WithEntityAccess()) {
					rvoAgent.ValueRW = managedState.rvoSettings;
					if (!managedState.enableLocalAvoidance) {
						commandBuffer.RemoveComponent<RVOAgent>(entity);
					}
				}

				Profiler.EndSample();
			}

			Profiler.BeginSample("Schedule search");
			// Block the pathfinding threads from starting new path calculations while this loop is running.
			// This is done to reduce lock contention and significantly improve performance.
			// If we did not do this, all pathfinding threads would immediately wake up when a path was pushed to the queue.
			// Immediately when they wake up they will try to acquire a lock on the path queue.
			// If we are scheduling a lot of paths, this causes significant contention, and can make this loop take 100 times
			// longer to complete, compared to if we block the pathfinding threads.
			// TODO: Switch to a lock-free queue to avoid this issue altogether.
			var pathfindingLock = AstarPath.active.PausePathfindingSoon();
			var time = (float)SystemAPI.Time.ElapsedTime;

			foreach (var(state, shape, movementState, movementSettings, transform, destination, movementPlane, entity) in SystemAPI.Query<ManagedState, RefRW<AgentCylinderShape>, RefRO<MovementState>, RefRW<MovementSettings>, RefRO<LocalTransform>, RefRW<DestinationPoint>, RefRO<AgentMovementPlane> >()
					 .WithEntityAccess()
			         // Do not recalculate the path of agents that are currently traversing an off-mesh link.
			         // Also do not try to add another off-mesh link component to agents that already have one.
					 .WithNone<AgentOffMeshLinkTraversal>()) {
				if ((state.pathTracer.isStale || state.autoRepath.ShouldRecalculatePath(transform.ValueRO.Position, shape.ValueRO.radius, destination.ValueRO.destination, time)) && state.pendingPath == null) {
					if (state.autoRepath.mode != AutoRepathPolicy.Mode.Never && float.IsFinite(destination.ValueRO.destination.x)) {
						var path = ABPath.Construct(transform.ValueRO.Position, destination.ValueRO.destination, null);
						path.UseSettings(state.pathfindingSettings);
						path.nnConstraint.distanceMetric = DistanceMetric.ClosestAsSeenFromAboveSoft(movementPlane.ValueRO.value.up);
						ManagedState.SetPath(path, state, in movementPlane.ValueRO, ref destination.ValueRW);
						state.autoRepath.DidRecalculatePath(destination.ValueRO.destination, time);
					}
				}

				// This would be a part of StartOffMeshLinkTransitionJob, but it's faster to do it here in the same loop as we are accessing the same memory.
				// Add the AgentOffMeshLinkTraversal and ManagedAgentOffMeshLinkTraversal when the agent should start traversing an off-mesh link.
				// If the graph has been recently updated, the link may contain destroyed nodes. In that case we shouldn't try to traverse it,
				// but instead wait for the agent to recalculate its path.
				if (movementState.ValueRO.reachedEndOfPart && state.pathTracer.partCount > 1 && state.pathTracer.GetPartType(1) == Funnel.PartType.OffMeshLink && !state.pathTracer.PartContainsDestroyedNodes(1)) {
					// If we are calculating a path right now, cancel that path calculation.
					// We don't want to calculate a path while we are traversing an off-mesh link.
					state.CancelCurrentPathRequest();

					var linkInfo = state.pathTracer.GetLinkInfo(1);
					commandBuffer.AddComponent(entity, new AgentOffMeshLinkTraversal {
						relativeStart = linkInfo.relativeStart,
						relativeEnd = linkInfo.relativeEnd,
						isReverse = linkInfo.isReverse,
					});
					var ctx = new AgentOffMeshLinkTraversalContext {
						link = linkInfo.link,
					};
					var handler = state.onTraverseOffMeshLink ?? ctx.link.handler;
					var stateMachine = handler != null? handler.GetOffMeshLinkStateMachine(ctx) : null;
					commandBuffer.AddComponent(entity, new ManagedAgentOffMeshLinkTraversal {
						context = ctx,
						stateMachine = stateMachine,
						coroutine = stateMachine != null ? stateMachine.OnTraverseOffMeshLink(ctx).GetEnumerator() : StartOffMeshLinkTransitionJob.DefaultOnTraverseOffMeshLink(ctx).GetEnumerator(),
					});
				}
			}
			pathfindingLock.Release();
			Profiler.EndSample();

			commandBuffer.Playback(systemState.EntityManager);
			commandBuffer.Dispose();

			{
				commandBuffer = new EntityCommandBuffer(systemState.WorldUpdateAllocator);
				systemState.CompleteDependency();
				new ManagedOffMeshLinkTransitionJob {
					commandBuffer = commandBuffer,
					deltaTime = AIMovementSystemGroup.TimeScaledRateManager.CheapStepDeltaTime,
				}.Run(entityQueryOffMeshLink);

				new ManagedOffMeshLinkTransitionCleanupJob().Run(entityQueryOffMeshLinkCleanup);
#if MODULE_ENTITIES_1_0_8_OR_NEWER
				commandBuffer.RemoveComponent<ManagedAgentOffMeshLinkTraversal>(entityQueryOffMeshLinkCleanup, EntityQueryCaptureMode.AtPlayback);
#else
				commandBuffer.RemoveComponent<ManagedAgentOffMeshLinkTraversal>(entityQueryOffMeshLinkCleanup);
#endif
				commandBuffer.Playback(systemState.EntityManager);
				commandBuffer.Dispose();
			}

			Profiler.BeginSample("PreparePathJob");
			LocalTransformTypeHandleRO.Update(ref systemState);
			MovementStateTypeHandleRW.Update(ref systemState);
			AgentCylinderShapeTypeHandleRO.Update(ref systemState);
			DestinationPointTypeHandleRO.Update(ref systemState);
			ManagedStateTypeHandleRW.Update(ref systemState);
			MovementSettingsTypeHandleRO.Update(ref systemState);
			AgentMovementPlaneTypeHandleRO.Update(ref systemState);
			MovementStateTypeHandleRO.Update(ref systemState);
			ResolvedMovementHandleRO.Update(ref systemState);

			// This job accesses managed component data in a somewhat unsafe way.
			// It should be safe to run it in parallel with other systems, but I'm not 100% sure.
			// This job also accesses graph data, but this is safe because the AIMovementSystemGroup
			// holds a read lock on the graph data while its subsystems are running.
			systemState.Dependency = new RepairPathJob {
				LocalTransformTypeHandleRO = LocalTransformTypeHandleRO,
				MovementStateTypeHandleRW = MovementStateTypeHandleRW,
				AgentCylinderShapeTypeHandleRO = AgentCylinderShapeTypeHandleRO,
				DestinationPointTypeHandleRO = DestinationPointTypeHandleRO,
				AgentMovementPlaneTypeHandleRO = AgentMovementPlaneTypeHandleRO,
				ManagedStateTypeHandleRW = ManagedStateTypeHandleRW,
				MovementSettingsTypeHandleRO = MovementSettingsTypeHandleRO,
				entityManagerHandle = entityManagerHandle,
			}.ScheduleParallel(entityQueryPrepare, systemState.Dependency);

			Profiler.EndSample();

			// The full movement calculations do not necessarily need to be done every frame if the fps is high
			if (!AIMovementSystemGroup.TimeScaledRateManager.CheapSimulationOnly) {
				redrawScope.Rewind();
				var draw = DrawingManager.GetBuilder(redrawScope);
				var navmeshEdgeData = AstarPath.active.hierarchicalGraph.navmeshEdges.GetNavmeshEdgeData(out var readLock);
				systemState.Dependency = new ControlJob {
					navmeshEdgeData = navmeshEdgeData,
					draw = draw,
					dt = SystemAPI.Time.DeltaTime,
				}.ScheduleParallel(entityQueryControl, JobHandle.CombineDependencies(systemState.Dependency, readLock.dependency));
				readLock.UnlockAfter(systemState.Dependency);
				draw.DisposeAfter(systemState.Dependency);
			}
		}

		[BurstCompile]
		public partial struct ControlJob : IJobEntity, IJobEntityChunkBeginEnd {
			public float dt;
			public CommandBuilder draw;
			[ReadOnly]
			[NativeDisableContainerSafetyRestriction]
			public NavmeshEdges.NavmeshEdgeData navmeshEdgeData;

			[NativeDisableContainerSafetyRestriction]
			public NativeList<float2> edgesScratch;

			private static readonly ProfilerMarker MarkerConvertObstacles = new ProfilerMarker("ConvertObstacles");

			static float3 ClampToNavmesh (float3 position, float3 closestOnNavmesh, in AgentCylinderShape shape, in AgentMovementPlane movementPlane) {
				// Don't clamp the elevation except to make sure it's not too far below the navmesh.
				var clamped2D = movementPlane.value.ToPlane(closestOnNavmesh, out float clampedElevation);
				movementPlane.value.ToPlane(position, out float currentElevation);
				currentElevation = math.max(currentElevation, clampedElevation - shape.height * 0.4f);
				position = movementPlane.value.ToWorld(clamped2D, currentElevation);
				return position;
			}

			public bool OnChunkBegin (in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
				if (!edgesScratch.IsCreated) edgesScratch = new NativeList<float2>(64, Allocator.Temp);
				return true;
			}

			public void OnChunkEnd (in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted) {}

			public void Execute (ref LocalTransform transform, ref MovementState state, in DestinationPoint destination, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, in MovementSettings settings, in ResolvedMovement resolvedMovement, ref MovementControl controlOutput) {
				// Clamp the agent to the navmesh.
				var position = ClampToNavmesh(transform.Position, state.closestOnNavmesh, in shape, in movementPlane);

				edgesScratch.Clear();
				var scale = transform.Scale;
				var settingsTemp = settings.follower;
				// Scale the settings by the agent's scale
				settingsTemp.speed *= scale;
				settingsTemp.leadInRadiusWhenApproachingDestination *= scale;
				settingsTemp.desiredWallDistance *= scale * resolvedMovement.turningRadiusMultiplier;

				if (state.hierarchicalNodeIndex != -1) {
					MarkerConvertObstacles.Begin();
					var localBounds = PIDMovement.InterestingEdgeBounds(ref settingsTemp, position, state.nextCorner, shape.height, movementPlane.value);
					navmeshEdgeData.GetEdgesInRange(state.hierarchicalNodeIndex, localBounds, edgesScratch, movementPlane.value);
					MarkerConvertObstacles.End();
				}

				// To ensure we detect that the end of the path is reached robustly we make the agent move slightly closer.
				// to the destination than the stopDistance.
				const float FUZZ = 0.005f;
				// If we are moving towards an off-mesh link, then we want the agent to stop precisely at the off-mesh link.
				// TODO: Depending on the link, we may want the agent to move towards the link at full speed, instead of slowing down.
				var stopDistance = state.traversingLastPart ? math.max(0, settings.stopDistance - FUZZ) : 0f;
				var distanceToSteeringTarget = math.max(0, state.remainingDistanceToEndOfPart - stopDistance);
				var rotation = movementPlane.value.ToPlane(transform.Rotation) - state.rotationOffset - state.rotationOffset2;

				transform.Position = position;

				if (dt > 0.000001f) {
					if (!math.isfinite(distanceToSteeringTarget)) {
						// The agent has no path, just stay still
						controlOutput = new MovementControl {
							targetPoint = position,
							speed = 0,
							endOfPath = position,
							maxSpeed = settings.follower.speed,
							overrideLocalAvoidance = false,
							hierarchicalNodeIndex = state.hierarchicalNodeIndex,
							targetRotation = resolvedMovement.targetRotation,
							rotationSpeed = settings.follower.maxRotationSpeed,
							targetRotationOffset = 0, // May be set by other systems
						};
					} else if (settings.isStopped) {
						// The user has requested that the agent slow down as quickly as possible.
						// TODO: If the agent is not clamped to the navmesh, it should still move towards the navmesh if it is outside it.
						controlOutput = new MovementControl {
							// Keep moving in the same direction as during the last frame, but slow down
							targetPoint = position + math.normalizesafe(resolvedMovement.targetPoint - position) * 10.0f,
							speed = settings.follower.Accelerate(resolvedMovement.speed, settings.follower.slowdownTime, -dt),
							endOfPath = state.endOfPath,
							maxSpeed = settings.follower.speed,
							overrideLocalAvoidance = false,
							hierarchicalNodeIndex = state.hierarchicalNodeIndex,
							targetRotation = resolvedMovement.targetRotation,
							rotationSpeed = settings.follower.maxRotationSpeed,
							targetRotationOffset = 0, // May be set by other systems
						};
					} else {
						var controlParams = new PIDMovement.ControlParams {
							edges = edgesScratch.AsArray(),
							nextCorner = state.nextCorner,
							agentRadius = shape.radius,
							facingDirectionAtEndOfPath = destination.facingDirection,
							endOfPath = state.endOfPath,
							remainingDistance = distanceToSteeringTarget,
							closestOnNavmesh = state.closestOnNavmesh,
							debugFlags = settings.debugFlags,
							p = position,
							rotation = rotation,
							maxDesiredWallDistance = state.followerState.maxDesiredWallDistance,
							speed = controlOutput.speed,
							movementPlane = movementPlane.value,
						};

						var control = PIDMovement.Control(ref settingsTemp, dt, ref controlParams, ref draw, out state.followerState.maxDesiredWallDistance);
						var positionDelta = movementPlane.value.ToWorld(control.positionDelta, 0);
						var speed = math.length(positionDelta) / dt;

						controlOutput = new MovementControl {
							targetPoint = position + math.normalizesafe(positionDelta) * distanceToSteeringTarget,
							speed = speed,
							endOfPath = state.endOfPath,
							maxSpeed = settingsTemp.speed * 1.1f,
							overrideLocalAvoidance = false,
							hierarchicalNodeIndex = state.hierarchicalNodeIndex,
							// It may seem sketchy to use a target rotation so close to the current rotation. One might think
							// there's risk of overshooting this target rotation if the frame rate is uneven.
							// But the TimeScaledRateManager ensures that this is not the case.
							// The cheap simulation's time (which is the one actually rotating the agent) is always guaranteed to be
							// behind (or precisely caught up with) the full simulation's time (that's the simulation which runs this system).
							targetRotation = rotation + control.rotationDelta,
							targetRotationHint = rotation + AstarMath.DeltaAngle(rotation, control.targetRotation),
							rotationSpeed = math.abs(control.rotationDelta / dt),
							targetRotationOffset = 0, // May be set by other systems
						};
					}
				} else {
					controlOutput.hierarchicalNodeIndex = -1;
				}
			}
		}
	}
}
#endif
