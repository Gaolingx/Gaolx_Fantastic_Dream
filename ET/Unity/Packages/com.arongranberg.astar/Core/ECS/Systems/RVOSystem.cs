#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Pathfinding.ECS.RVO {
	using Pathfinding.RVO;

	[BurstCompile]
	[UpdateAfter(typeof(FollowerControlSystem))]
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	public partial struct RVOSystem : ISystem {
		EntityQuery entityQuery;
		/// <summary>
		/// Keeps track of the last simulator that this RVOSystem saw.
		/// This is a weak GCHandle to allow it to be stored in an ISystem.
		/// </summary>
		GCHandle lastSimulator;
		EntityQuery withAgentIndex;
		EntityQuery shouldBeAddedToSimulation;
		EntityQuery shouldBeRemovedFromSimulation;
		ComponentLookup<AgentOffMeshLinkTraversal> agentOffMeshLinkTraversalLookup;

		public void OnCreate (ref SystemState state) {
			entityQuery = state.GetEntityQuery(
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadOnly<LocalTransform>(),
				ComponentType.ReadOnly<RVOAgent>(),
				ComponentType.ReadOnly<AgentIndex>(),
				ComponentType.ReadOnly<AgentMovementPlane>(),
				ComponentType.ReadOnly<MovementControl>(),
				ComponentType.ReadWrite<ResolvedMovement>(),
				ComponentType.ReadOnly<SimulateMovement>()
				);
			withAgentIndex = state.GetEntityQuery(
				ComponentType.ReadWrite<AgentIndex>()
				);
			shouldBeAddedToSimulation = state.GetEntityQuery(
				ComponentType.ReadOnly<RVOAgent>(),
				ComponentType.Exclude<AgentIndex>()
				);
			shouldBeRemovedFromSimulation = state.GetEntityQuery(
				ComponentType.ReadOnly<AgentIndex>(),
				ComponentType.Exclude<RVOAgent>()
				);
			lastSimulator = GCHandle.Alloc(null, System.Runtime.InteropServices.GCHandleType.Weak);
			agentOffMeshLinkTraversalLookup = state.GetComponentLookup<AgentOffMeshLinkTraversal>(true);
		}

		public void OnDestroy (ref SystemState state) {
			lastSimulator.Free();
		}

		public void OnUpdate (ref SystemState systemState) {
			var simulator = RVOSimulator.active?.GetSimulator();

			// Early out in case we don't need to do anything
			if (simulator == null && lastSimulator.Target == null) return;

			// If the simulator has been destroyed, we need to remove all AgentIndex components
			if (simulator != lastSimulator.Target) {
				var buffer = new EntityCommandBuffer(Allocator.Temp);
				var entities = withAgentIndex.ToEntityArray(systemState.WorldUpdateAllocator);
				buffer.RemoveComponent<AgentIndex>(entities);

				lastSimulator.Target = simulator;
				buffer.Playback(systemState.EntityManager);
				buffer.Dispose();
			}
			if (simulator != null) {
				// Remove all agents from the simulation that do not have an RVOAgent component, but have an AgentIndex
				var indicesToRemove = shouldBeRemovedFromSimulation.ToComponentDataArray<AgentIndex>(systemState.WorldUpdateAllocator);
				// Add all agents to the simulation that have an RVOAgent component, but not AgentIndex component
				var entitiesToAdd = shouldBeAddedToSimulation.ToEntityArray(systemState.WorldUpdateAllocator);
				// Avoid a sync point in the common case
				if (indicesToRemove.Length > 0 || entitiesToAdd.Length > 0) {
					var buffer = new EntityCommandBuffer(Allocator.Temp);
#if MODULE_ENTITIES_1_0_8_OR_NEWER
					buffer.RemoveComponent<AgentIndex>(shouldBeRemovedFromSimulation, EntityQueryCaptureMode.AtPlayback);
#else
					buffer.RemoveComponent<AgentIndex>(shouldBeRemovedFromSimulation);
#endif
					for (int i = 0; i < indicesToRemove.Length; i++) {
						simulator.RemoveAgent(indicesToRemove[i]);
					}
					for (int i = 0; i < entitiesToAdd.Length; i++) {
						buffer.AddComponent<AgentIndex>(entitiesToAdd[i], simulator.AddAgentBurst(UnityEngine.Vector3.zero));
					}

					buffer.Playback(systemState.EntityManager);
					buffer.Dispose();
				}
			} else {
				return;
			}

			// The full movement calculations do not necessarily need to be done every frame if the fps is high
			if (AIMovementSystemGroup.TimeScaledRateManager.CheapSimulationOnly) {
				return;
			}

			agentOffMeshLinkTraversalLookup.Update(ref systemState);
			systemState.Dependency = new CopyFromEntitiesToRVOSimulator {
				agentData = simulator.simulationData,
				agentOutputData = simulator.outputData,
				movementPlaneMode = simulator.movementPlane,
				agentOffMeshLinkTraversalLookup = agentOffMeshLinkTraversalLookup,
				dt = SystemAPI.Time.DeltaTime,
			}.ScheduleParallel(entityQuery, systemState.Dependency);

			// Schedule RVO update
			systemState.Dependency = simulator.Update(
				systemState.Dependency,
				SystemAPI.Time.DeltaTime,
				AIMovementSystemGroup.TimeScaledRateManager.IsLastSubstep,
				systemState.WorldUpdateAllocator
				);

			systemState.Dependency = new CopyFromRVOSimulatorToEntities {
				quadtree = simulator.quadtree,
				agentData = simulator.simulationData,
				agentOutputData = simulator.outputData,
			}.ScheduleParallel(entityQuery, systemState.Dependency);
			simulator.LockSimulationDataReadOnly(systemState.Dependency);
		}

		[BurstCompile]
		public partial struct CopyFromEntitiesToRVOSimulator : IJobEntity {
			[NativeDisableParallelForRestrictionAttribute]
			public SimulatorBurst.AgentData agentData;
			[ReadOnly]
			public SimulatorBurst.AgentOutputData agentOutputData;
			public MovementPlane movementPlaneMode;
			[ReadOnly]
			public ComponentLookup<AgentOffMeshLinkTraversal> agentOffMeshLinkTraversalLookup;
			public float dt;

			public void Execute (Entity entity, in LocalTransform transform, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, in AgentIndex agentIndex, in RVOAgent controller, in MovementControl target) {
				var scale = math.abs(transform.Scale);
				var index = agentIndex.Index;

				if (agentData.version[index].Version != agentIndex.Version) throw new System.InvalidOperationException("RVOAgent has an invalid entity index");

				// Actual infinity is not handled well by some algorithms, but very large values are ok.
				// This should be larger than any reasonable value a user might want to use.
				const float VERY_LARGE = 100000;

				// Copy all fields to the rvo simulator, and clamp them to reasonable values
				agentData.radius[index] = math.clamp(shape.radius * scale, 0.001f, VERY_LARGE);
				agentData.agentTimeHorizon[index] = math.clamp(controller.agentTimeHorizon, 0, VERY_LARGE);
				agentData.obstacleTimeHorizon[index] = math.clamp(controller.obstacleTimeHorizon, 0, VERY_LARGE);
				agentData.locked[index] = controller.locked;
				agentData.maxNeighbours[index] = math.max(controller.maxNeighbours, 0);
				agentData.debugFlags[index] = controller.debug;
				agentData.layer[index] = controller.layer;
				agentData.collidesWith[index] = controller.collidesWith;
				agentData.targetPoint[index] = target.targetPoint;
				agentData.desiredSpeed[index] = math.clamp(target.speed, 0, VERY_LARGE);
				agentData.maxSpeed[index] = math.clamp(target.maxSpeed, 0, VERY_LARGE);
				agentData.manuallyControlled[index] = target.overrideLocalAvoidance;
				agentData.endOfPath[index] = target.endOfPath;
				agentData.hierarchicalNodeIndex[index] = target.hierarchicalNodeIndex;
				// control.endOfPath // TODO
				agentData.movementPlane[index] = movementPlane.value;

				// Use the position from the movement script if one is attached
				// as the movement script's position may not be the same as the transform's position
				// (in particular if IAstarAI.updatePosition is false).
				var pos = movementPlane.value.ToPlane(transform.Position, out float elevation);
				var center = 0.5f * shape.height;
				if (movementPlaneMode == MovementPlane.XY) {
					// In 2D it is assumed the Z coordinate differences of agents is ignored.
					agentData.height[index] = 1;
					agentData.position[index] = movementPlane.value.ToWorld(pos, 0);
				} else {
					agentData.height[index] = math.clamp(shape.height * scale, 0, VERY_LARGE);
					agentData.position[index] = movementPlane.value.ToWorld(pos, elevation + (center - 0.5f * shape.height) * scale);
				}


				// TODO: Move this to a separate file
				var reached = agentOutputData.effectivelyReachedDestination[index];
				var prio = math.clamp(controller.priority * controller.priorityMultiplier, 0, VERY_LARGE);
				var flow = math.clamp(controller.flowFollowingStrength, 0, 1);
				if (reached == ReachedEndOfPath.Reached) {
					flow = math.lerp(agentData.flowFollowingStrength[index], 1.0f, 6.0f * dt);
					prio *= 0.3f;
				} else if (reached == ReachedEndOfPath.ReachedSoon) {
					flow = math.lerp(agentData.flowFollowingStrength[index], 1.0f, 6.0f * dt);
					prio *= 0.45f;
				}
				agentData.priority[index] = prio;
				agentData.flowFollowingStrength[index] = flow;

				if (agentOffMeshLinkTraversalLookup.HasComponent(entity)) {
					// Agents traversing off-mesh links should not avoid other agents,
					// but other agents may still avoid them.
					agentData.manuallyControlled[index] = true;
				}
			}
		}

		[BurstCompile]
		public partial struct CopyFromRVOSimulatorToEntities : IJobEntity {
			[ReadOnly]
			public SimulatorBurst.AgentData agentData;
			[ReadOnly]
			public RVOQuadtreeBurst quadtree;
			[ReadOnly]
			public SimulatorBurst.AgentOutputData agentOutputData;

			/// <summary>See https://en.wikipedia.org/wiki/Circle_packing</summary>
			const float MaximumCirclePackingDensity = 0.9069f;

			public void Execute (in LocalTransform transform, in AgentCylinderShape shape, in AgentIndex agentIndex, in RVOAgent controller, in MovementControl control, ref ResolvedMovement resolved) {
				var index = agentIndex.Index;

				if (agentData.version[index].Version != agentIndex.Version) return;

				var scale = math.abs(transform.Scale);
				var r = shape.radius * scale * 3f;
				var area = quadtree.QueryArea(transform.Position, r);
				var density = area / (MaximumCirclePackingDensity * math.PI * r * r);


				resolved.targetPoint = agentOutputData.targetPoint[index];
				resolved.speed = agentOutputData.speed[index];
				var rnd = 1.0f; // (agentIndex.Index % 1024) / 1024f;
				resolved.turningRadiusMultiplier = math.max(1f, math.pow(density * 2.0f, 4.0f) * rnd);

				// Pure copy
				resolved.targetRotation = control.targetRotation;
				resolved.targetRotationHint = control.targetRotationHint;
				resolved.targetRotationOffset = control.targetRotationOffset;
				resolved.rotationSpeed = control.rotationSpeed;
			}
		}
	}
}
#endif
