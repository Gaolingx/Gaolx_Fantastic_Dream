#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;
	using Pathfinding.Drawing;
	using Pathfinding.Util;

	[BurstCompile]
	[UpdateAfter(typeof(FollowerControlSystem))]
	[UpdateAfter(typeof(RVOSystem))]
	[UpdateAfter(typeof(FallbackResolveMovementSystem))]
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[RequireMatchingQueriesForUpdate]
	public partial struct AIMoveSystem : ISystem {
		EntityQuery entityQueryPrepareMovement;
		EntityQuery entityQueryWithGravity;
		EntityQuery entityQueryWithoutGravity;
		EntityQuery entityQueryRotation;
		EntityQuery entityQueryGizmos;
		ComponentTypeHandle<LocalTransform> LocalTransformTypeHandleRO;
		ComponentTypeHandle<MovementState> MovementStateTypeHandleRW;
		ComponentTypeHandle<AgentCylinderShape> AgentCylinderShapeTypeHandleRO;
		ComponentTypeHandle<DestinationPoint> DestinationPointTypeHandleRO;
		ComponentTypeHandle<AgentMovementPlane> AgentMovementPlaneTypeHandleRO;
		ComponentTypeHandle<ManagedState> ManagedStateTypeHandleRW;
		ComponentTypeHandle<MovementSettings> MovementSettingsTypeHandleRO;
		ComponentTypeHandle<MovementState> MovementStateTypeHandleRO;
		ComponentTypeHandle<ResolvedMovement> ResolvedMovementHandleRO;
		GCHandle entityManagerHandle;

		public static EntityQueryBuilder EntityQueryPrepareMovement () {
			return new EntityQueryBuilder(Allocator.Temp)
				   .WithAllRW<MovementState>()
				   .WithAllRW<ManagedState>()
				   .WithAllRW<LocalTransform>()
				   .WithAll<MovementSettings, DestinationPoint, AgentMovementPlane, AgentCylinderShape>()
				   .WithAbsent<AgentOffMeshLinkTraversal>();
		}

		public void OnCreate (ref SystemState state) {
			entityManagerHandle = GCHandle.Alloc(state.EntityManager);

			LocalTransformTypeHandleRO = state.GetComponentTypeHandle<LocalTransform>(true);
			MovementStateTypeHandleRW = state.GetComponentTypeHandle<MovementState>(false);
			AgentCylinderShapeTypeHandleRO = state.GetComponentTypeHandle<AgentCylinderShape>(true);
			DestinationPointTypeHandleRO = state.GetComponentTypeHandle<DestinationPoint>(true);
			AgentMovementPlaneTypeHandleRO = state.GetComponentTypeHandle<AgentMovementPlane>(true);
			MovementSettingsTypeHandleRO = state.GetComponentTypeHandle<MovementSettings>(true);
			MovementStateTypeHandleRO = state.GetComponentTypeHandle<MovementState>(true);
			ResolvedMovementHandleRO = state.GetComponentTypeHandle<ResolvedMovement>(true);
			// Need to bypass the T : unmanaged check in state.GetComponentTypeHandle
			ManagedStateTypeHandleRW = state.EntityManager.GetComponentTypeHandle<ManagedState>(false);

			entityQueryRotation = state.GetEntityQuery(
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadOnly<MovementSettings>(),
				ComponentType.ReadOnly<MovementState>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadOnly<AgentMovementPlane>(),
				ComponentType.ReadOnly<MovementControl>(),
				ComponentType.ReadWrite<ResolvedMovement>(),
				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementFinalize>()
				);

			entityQueryWithoutGravity = state.GetEntityQuery(
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadOnly<AgentMovementPlane>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadOnly<MovementSettings>(),
				ComponentType.ReadOnly<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementStatistics>(),

				ComponentType.Exclude<GravityState>(),

				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementFinalize>()
				);

			entityQueryWithGravity = state.GetEntityQuery(
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadWrite<AgentMovementPlane>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadOnly<MovementSettings>(),
				ComponentType.ReadWrite<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementStatistics>(),
				ComponentType.ReadOnly<MovementControl>(),
				ComponentType.ReadWrite<GravityState>(),

				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementFinalize>()
				);

			entityQueryPrepareMovement = EntityQueryPrepareMovement().WithAll<SimulateMovement, SimulateMovementRepair>().Build(ref state);

			entityQueryGizmos = state.GetEntityQuery(
				ComponentType.ReadOnly<LocalTransform>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadOnly<MovementSettings>(),
				ComponentType.ReadOnly<AgentMovementPlane>(),
				ComponentType.ReadOnly<ManagedState>(),
				ComponentType.ReadOnly<MovementState>(),
				ComponentType.ReadOnly<ResolvedMovement>(),

				ComponentType.ReadOnly<SimulateMovement>()
				);
		}

		public void OnDestroy (ref SystemState state) {
			entityManagerHandle.Free();
		}

		public void OnUpdate (ref SystemState systemState) {
			var count = entityQueryWithGravity.CalculateEntityCount();
			var raycastCommands = CollectionHelper.CreateNativeArray<RaycastCommand>(count, systemState.WorldUpdateAllocator);
			var raycastHits = CollectionHelper.CreateNativeArray<RaycastHit>(count, systemState.WorldUpdateAllocator);
			var draw = DrawingManager.GetBuilder();

			// This system is executed at least every frame to make sure the agent is moving smoothly even at high fps.
			// The control loop and local avoidance may be running less often.
			// So this is designated a "cheap" system, and we use the corresponding delta time for that.
			var dt = AIMovementSystemGroup.TimeScaledRateManager.CheapStepDeltaTime;

			systemState.Dependency = new JobAlignAgentWithMovementDirection {
				dt = dt,
			}.Schedule(entityQueryRotation, systemState.Dependency);

			// Move all agents which do not have a GravityState component
			systemState.Dependency = new JobMoveAgent {
				dt = dt,
			}.ScheduleParallel(entityQueryWithoutGravity, systemState.Dependency);

			// Prepare raycasts for all entities that have a GravityState component
			systemState.Dependency = new JobPrepareAgentRaycasts {
				raycastQueryParameters = new QueryParameters(-1, false, QueryTriggerInteraction.Ignore, false),
				raycastCommands = raycastCommands,
				draw = draw,
				dt = dt,
				gravity = Physics.gravity.y,
			}.ScheduleParallel(entityQueryWithGravity, systemState.Dependency);

			var raycastJob = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 32, 1, systemState.Dependency);

			// Apply gravity and move all agents that have a GravityState component
			systemState.Dependency = new JobApplyGravity {
				raycastHits = raycastHits,
				raycastCommands = raycastCommands,
				draw = draw,
				dt = dt,
			}.ScheduleParallel(entityQueryWithGravity, JobHandle.CombineDependencies(systemState.Dependency, raycastJob));

			UnityEngine.Profiling.Profiler.BeginSample("PreparePathJob");
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
			}.ScheduleParallel(entityQueryPrepareMovement, systemState.Dependency);

			UnityEngine.Profiling.Profiler.EndSample();

			var gizmosDependency = systemState.Dependency;

			// Draw gizmos only in the editor, and at most once per frame.
			// The movement calculations may run multiple times per frame when using high time-scales,
			// but rendering gizmos more than once would just lead to clutter.
			if (Application.isEditor && AIMovementSystemGroup.TimeScaledRateManager.IsLastSubstep) {
				gizmosDependency = new DrawGizmosJob {
					draw = draw,
					entityManagerHandle = entityManagerHandle,
					LocalTransformTypeHandleRO = LocalTransformTypeHandleRO,
					AgentCylinderShapeHandleRO = AgentCylinderShapeTypeHandleRO,
					MovementSettingsHandleRO = MovementSettingsTypeHandleRO,
					AgentMovementPlaneHandleRO = AgentMovementPlaneTypeHandleRO,
					ManagedStateHandleRW = ManagedStateTypeHandleRW,
					MovementStateHandleRO = MovementStateTypeHandleRO,
					ResolvedMovementHandleRO = ResolvedMovementHandleRO,
				}.ScheduleParallel(entityQueryGizmos, systemState.Dependency);
			}

			// Render gizmos as soon as all relevant jobs are done
			draw.DisposeAfter(gizmosDependency);

			int numComponents = BatchedEvents.GetComponents<FollowerEntity>(BatchedEvents.Event.None, out var transforms, out var components);
			if (numComponents > 0) {
				var entities = CollectionHelper.CreateNativeArray<Entity>(numComponents, systemState.WorldUpdateAllocator);
				for (int i = 0; i < numComponents; i++) entities[i] = components[i].entity;

				systemState.Dependency = new JobSyncEntitiesToTransforms {
					entities = entities,
					syncPositionWithTransform = SystemAPI.GetComponentLookup<SyncPositionWithTransform>(true),
					syncRotationWithTransform = SystemAPI.GetComponentLookup<SyncRotationWithTransform>(true),
					orientationYAxisForward = SystemAPI.GetComponentLookup<OrientationYAxisForward>(true),
					entityPositions = SystemAPI.GetComponentLookup<LocalTransform>(true),
					movementState = SystemAPI.GetComponentLookup<MovementState>(true),
				}.Schedule(transforms, systemState.Dependency);
			}

			systemState.Dependency = JobHandle.CombineDependencies(systemState.Dependency, gizmosDependency);
		}
	}
}
#endif
