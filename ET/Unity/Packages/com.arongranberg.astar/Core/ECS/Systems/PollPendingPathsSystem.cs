#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Burst;
using GCHandle = System.Runtime.InteropServices.GCHandle;
using Unity.Transforms;

namespace Pathfinding.ECS {
	/// <summary>
	/// Checks if paths have been calculated, and updates the agent's paths if they have.
	///
	/// This is essentially a replacement for <see cref="Path.callback"/> for ECS agents.
	///
	/// This system is a bit different in that it doesn't run in the normal update loop,
	/// but instead it will run when the <see cref="AstarPath.OnPathsCalculated"/> event fires.
	/// This is to avoid having to call a separate callback for every agent, since that
	/// would result in excessive overhead as it would have to synchronize with the ECS world
	/// on every such call.
	///
	/// See: <see cref="AstarPath.OnPathsCalculated"/>
	/// </summary>
	[BurstCompile]
	public partial struct PollPendingPathsSystem : ISystem {
		GCHandle onPathsCalculated;
		GCHandle entityManagerHandle;
		static bool anyPendingPaths;

		ComponentTypeHandle<LocalTransform> LocalTransformTypeHandleRO;
		ComponentTypeHandle<MovementState> MovementStateTypeHandleRW;
		ComponentTypeHandle<MovementState> MovementStateTypeHandleRO;
		ComponentTypeHandle<AgentCylinderShape> AgentCylinderShapeTypeHandleRO;
		ComponentTypeHandle<DestinationPoint> DestinationPointTypeHandleRO;
		ComponentTypeHandle<AgentMovementPlane> AgentMovementPlaneTypeHandleRO;
		ComponentTypeHandle<ManagedState> ManagedStateTypeHandleRW;
		ComponentTypeHandle<MovementSettings> MovementSettingsTypeHandleRO;
		ComponentTypeHandle<ResolvedMovement> ResolvedMovementHandleRO;
		EntityQuery entityQueryPrepare;

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

			entityQueryPrepare = AIMoveSystem.EntityQueryPrepareMovement().Build(ref state);

			var world = state.WorldUnmanaged;
			System.Action onPathsCalculated = () => {
				// Allow the system to run
				anyPendingPaths = true;
				try {
					// Update the system manually
					world.GetExistingUnmanagedSystem<PollPendingPathsSystem>().Update(world);
				} finally {
					anyPendingPaths = false;
				}
			};
			AstarPath.OnPathsCalculated += onPathsCalculated;
			// Store the callback in a GCHandle to get around limitations on unmanaged systems.
			this.onPathsCalculated = GCHandle.Alloc(onPathsCalculated);
		}

		public void OnDestroy (ref SystemState state) {
			AstarPath.OnPathsCalculated -= (System.Action)onPathsCalculated.Target;
			onPathsCalculated.Free();
			entityManagerHandle.Free();
		}

		void OnUpdate (ref SystemState systemState) {
			// Only run the system when we have triggered it manually
			if (!anyPendingPaths) return;

			// During an off-mesh link traversal, we shouldn't calculate any paths, because it's somewhat undefined where they should start.
			// Paths are already cancelled when the off-mesh link traversal starts, but just in case it has been started by a user manually in some way, we also cancel them every frame.
			foreach (var state in SystemAPI.Query<ManagedState>().WithAll<AgentOffMeshLinkTraversal>()) state.CancelCurrentPathRequest();

			// This job accesses managed component data in a somewhat unsafe way.
			// It should be safe to run it in parallel with other systems, but I'm not 100% sure.
			LocalTransformTypeHandleRO.Update(ref systemState);
			MovementStateTypeHandleRW.Update(ref systemState);
			AgentCylinderShapeTypeHandleRO.Update(ref systemState);
			DestinationPointTypeHandleRO.Update(ref systemState);
			ManagedStateTypeHandleRW.Update(ref systemState);
			MovementSettingsTypeHandleRO.Update(ref systemState);
			AgentMovementPlaneTypeHandleRO.Update(ref systemState);
			MovementStateTypeHandleRO.Update(ref systemState);
			ResolvedMovementHandleRO.Update(ref systemState);

			// The RepairPathJob may access graph data, so we need to lock it for reading.
			// Otherwise a graph update could start while the job was running, which could cause all kinds of problems.
			var readLock = AstarPath.active.LockGraphDataForReading();

			// Iterate over all agents and check if they have any pending paths, and if they have been calculated.
			// If they have, we update the agent's current path to the newly calculated one.
			//
			// We do this by running the RepairPathJob for all agents that have just had their path calculated.
			// This ensures that all properties like remainingDistance are up to date immediately after
			// a path recalculation.
			// This may seem wasteful, but during the next update, the regular RepairPathJob job
			// will most likely be able to early out, because we did most of the work here.
			systemState.Dependency = new RepairPathJob {
				LocalTransformTypeHandleRO = LocalTransformTypeHandleRO,
				MovementStateTypeHandleRW = MovementStateTypeHandleRW,
				AgentCylinderShapeTypeHandleRO = AgentCylinderShapeTypeHandleRO,
				DestinationPointTypeHandleRO = DestinationPointTypeHandleRO,
				AgentMovementPlaneTypeHandleRO = AgentMovementPlaneTypeHandleRO,
				ManagedStateTypeHandleRW = ManagedStateTypeHandleRW,
				MovementSettingsTypeHandleRO = MovementSettingsTypeHandleRO,
				entityManagerHandle = entityManagerHandle,
				onlyApplyPendingPaths = true,
			}.ScheduleParallel(entityQueryPrepare, systemState.Dependency);

			readLock.UnlockAfter(systemState.Dependency);
		}
	}
}
#endif
