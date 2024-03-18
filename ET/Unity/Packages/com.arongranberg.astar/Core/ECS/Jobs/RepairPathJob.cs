#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.Util;
	using Unity.Burst;

	/// <summary>
	/// Repairs the path of agents.
	///
	/// This job will repair the agent's path based on the agent's current position and its destination.
	/// It will also recalculate various statistics like how far the agent is from the destination,
	/// and if it has reached the destination or not.
	/// </summary>
	public struct RepairPathJob : IJobChunk {
		public GCHandle entityManagerHandle;
		[ReadOnly]
		public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandleRO;
		public ComponentTypeHandle<MovementState> MovementStateTypeHandleRW;
		[ReadOnly]
		public ComponentTypeHandle<AgentCylinderShape> AgentCylinderShapeTypeHandleRO;
		public ComponentTypeHandle<ManagedState> ManagedStateTypeHandleRW;
		[ReadOnly]
		public ComponentTypeHandle<MovementSettings> MovementSettingsTypeHandleRO;
		[ReadOnly]
		public ComponentTypeHandle<DestinationPoint> DestinationPointTypeHandleRO;
		[ReadOnly]
		public ComponentTypeHandle<AgentMovementPlane> AgentMovementPlaneTypeHandleRO;


		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> indicesScratch;
		[NativeDisableContainerSafetyRestriction]
		public NativeList<float3> nextCornersScratch;
		public bool onlyApplyPendingPaths;


		public void Execute (in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in Unity.Burst.Intrinsics.v128 chunkEnabledMask) {
			if (!indicesScratch.IsCreated) {
				nextCornersScratch = new NativeList<float3>(Allocator.Temp);
				indicesScratch = new NativeArray<int>(8, Allocator.Temp);
			}

			unsafe {
				var localTransforms = (LocalTransform*)chunk.GetNativeArray(ref LocalTransformTypeHandleRO).GetUnsafeReadOnlyPtr();
				var movementStates = (MovementState*)chunk.GetNativeArray(ref MovementStateTypeHandleRW).GetUnsafePtr();
				var agentCylinderShapes = (AgentCylinderShape*)chunk.GetNativeArray(ref AgentCylinderShapeTypeHandleRO).GetUnsafeReadOnlyPtr();
				var destinationPoints = (DestinationPoint*)chunk.GetNativeArray(ref DestinationPointTypeHandleRO).GetUnsafeReadOnlyPtr();
				var movementSettings = (MovementSettings*)chunk.GetNativeArray(ref MovementSettingsTypeHandleRO).GetUnsafeReadOnlyPtr();
				var agentMovementPlanes = (AgentMovementPlane*)chunk.GetNativeArray(ref AgentMovementPlaneTypeHandleRO).GetUnsafeReadOnlyPtr();
				var managedStates = chunk.GetManagedComponentAccessor(ref ManagedStateTypeHandleRW, (EntityManager)entityManagerHandle.Target);

				for (int i = 0; i < chunk.Count; i++) {
					Execute(
						ref localTransforms[i],
						ref movementStates[i],
						ref agentCylinderShapes[i],
						ref agentMovementPlanes[i],
						ref destinationPoints[i],
						managedStates[i],
						in movementSettings[i],
						nextCornersScratch,
						ref indicesScratch,
						Allocator.Temp,
						onlyApplyPendingPaths
						);
				}
			}
		}

		private static readonly ProfilerMarker MarkerRepair = new ProfilerMarker("Repair");
		private static readonly ProfilerMarker MarkerGetNextCorners = new ProfilerMarker("GetNextCorners");
		private static readonly ProfilerMarker MarkerUpdateReachedEndInfo = new ProfilerMarker("UpdateReachedEndInfo");

		public static void Execute (ref LocalTransform transform, ref MovementState state, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ManagedState managedState, in MovementSettings settings, NativeList<float3> nextCornersScratch, ref NativeArray<int> indicesScratch, Allocator allocator, bool onlyApplyPendingPaths) {
			// Only enabled by the PollPendingPathsSystem
			if (onlyApplyPendingPaths) {
				if (managedState.pendingPath != null && managedState.pendingPath.IsDone()) {
					// The path has been calculated, apply it to the agent
					// Immediately after this we must also repair the path to ensure that it is valid and that
					// all properties like #MovementState.reachedEndOfPath are correct.
					ManagedState.SetPath(managedState.pendingPath, managedState, in movementPlane, ref destination);
				} else {
					// The agent has no path that has just been calculated, skip it
					return;
				}
			}

			ref var pathTracer = ref managedState.pathTracer;

			if (pathTracer.hasPath) {
				MarkerRepair.Begin();
				// Update the start and end points of the path based on the current position and destination.
				// This will repair the path if necessary, ensuring that the agent has a valid, if not necessarily optimal, path.
				// If it cannot be repaired well, the path will be marked as stale.
				state.closestOnNavmesh = pathTracer.UpdateStart(transform.Position, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
				state.endOfPath = pathTracer.UpdateEnd(destination.destination, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
				MarkerRepair.End();

				if (state.pathTracerVersion != pathTracer.version) {
					nextCornersScratch.Clear();

					MarkerGetNextCorners.Begin();
					// Find the next corners of the path. The first corner is our current position,
					// the second corner is the one we are moving towards and the third corner is the one after that.
					//
					// Using GetNextCorners with the default transformation instead of ConvertCornerIndicesToPathProjected
					// is about 20% faster, but it does not work well at all on spherical worlds.
					// In the future we might want to switch dynamically between these modes,
					// but on the other hand, it is very nice to be able to use the exact same code path for everything.
					// pathTracer.GetNextCorners(nextCornersScratch, 3, ref indicesScratch, allocator);
					var numCorners = pathTracer.GetNextCornerIndices(ref indicesScratch, pathTracer.desiredCornersForGoodSimplification, allocator, out bool lastCorner, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
					pathTracer.ConvertCornerIndicesToPathProjected(indicesScratch, numCorners, lastCorner, nextCornersScratch, movementPlane.value.up);
					MarkerGetNextCorners.End();

					// We need to copy a few fields to a new struct, in order to be able to pass it to a burstified function
					var pathTracerInfo = new RepairPathJobHelpers.PathTracerInfo {
						endPointOfFirstPart = pathTracer.endPointOfFirstPart,
						partCount = pathTracer.partCount,
						isStale = pathTracer.isStale
					};
					var nextCorners = nextCornersScratch.AsUnsafeSpan();
					RepairPathJobHelpers.UpdateReachedEndInfo(ref nextCorners, ref state, ref movementPlane, ref transform, ref shape, ref destination, settings.stopDistance, ref pathTracerInfo);
					state.pathTracerVersion = pathTracer.version;
				} else {
					RepairPathJobHelpers.UpdateReachedOrientation(ref state, ref transform, ref movementPlane, ref destination);
				}

				if (pathTracer.startNode != null && !pathTracer.startNode.Destroyed && pathTracer.startNode.Walkable) {
					state.hierarchicalNodeIndex = pathTracer.startNode.HierarchicalNodeIndex;
				} else {
					state.hierarchicalNodeIndex = -1;
				}
			} else {
				state.SetPathIsEmpty(transform.Position);
			}
		}
	}

	[BurstCompile]
	static class RepairPathJobHelpers {
		public struct PathTracerInfo {
			public float3 endPointOfFirstPart;
			public int partCount;
			// Bools are not blittable by burst so we must use a byte instead. Very ugly, but it is what it is.
			byte isStaleBacking;
			public bool isStale {
				get => isStaleBacking != 0;
				set => isStaleBacking = value ? (byte)1 : (byte)0;
			}
		}

		/// <summary>Checks if the agent has reached its destination, or the end of the path</summary>
		[BurstCompile]
		public static void UpdateReachedEndInfo (ref UnsafeSpan<float3> nextCorners, ref MovementState state, ref AgentMovementPlane movementPlane, ref LocalTransform transform, ref AgentCylinderShape shape, ref DestinationPoint destination, float stopDistance, ref PathTracerInfo pathTracer) {
			// TODO: Edit GetNextCorners so that it gets corners until at least stopDistance units from the agent
			state.nextCorner = nextCorners.length > 1 ? nextCorners[1] : transform.Position;
			state.remainingDistanceToEndOfPart = PathTracer.RemainingDistanceLowerBound(in nextCorners, in pathTracer.endPointOfFirstPart, in movementPlane.value);

			// TODO: Check if end node is the globally closest node
			movementPlane.value.ToPlane(pathTracer.endPointOfFirstPart - transform.Position, out var elevationDiffEndOfPart);
			var validHeightRangeEndOfPart = elevationDiffEndOfPart< shape.height && elevationDiffEndOfPart > -0.5f*shape.height;

			movementPlane.value.ToPlane(destination.destination - transform.Position, out var elevationDiffDestination);
			var validHeightRangeDestination = elevationDiffDestination< shape.height && elevationDiffDestination > -0.5f*shape.height;
			var endOfPathToDestination = math.length(movementPlane.value.ToPlane(destination.destination - state.endOfPath));
			// If reachedEndOfPath is true we allow a slightly larger margin of error for reachedDestination.
			// This is to ensure that if reachedEndOfPath becomes true, it is very likely that reachedDestination becomes
			// true during the same frame.
			const float FUZZ = 0.01f;
			// When checking if the agent has reached the end of the current part (mostly used for off-mesh-links), we check against
			// the agent's radius. This is because when there are many agents trying to reach the same off-mesh-link, the agents will
			// crowd up and it may become hard to get to a point closer than the agent's radius.
			state.reachedEndOfPart = !pathTracer.isStale && validHeightRangeEndOfPart && state.remainingDistanceToEndOfPart <= shape.radius*1.1f;
			state.reachedEndOfPath = !pathTracer.isStale && validHeightRangeEndOfPart && pathTracer.partCount == 1 && state.remainingDistanceToEndOfPart <= stopDistance;
			state.reachedDestination = !pathTracer.isStale && validHeightRangeDestination && pathTracer.partCount == 1 && state.remainingDistanceToEndOfPart + endOfPathToDestination <= stopDistance + (state.reachedEndOfPath ? FUZZ : 0);
			state.traversingLastPart = pathTracer.partCount == 1;
			UpdateReachedOrientation(ref state, ref transform, ref movementPlane, ref destination);
		}

		/// <summary>Checks if the agent is oriented towards the desired facing direction</summary>
		public static void UpdateReachedOrientation (ref MovementState state, ref LocalTransform transform, ref AgentMovementPlane movementPlane, ref DestinationPoint destination) {
			state.reachedEndOfPathAndOrientation = state.reachedEndOfPath;
			state.reachedDestinationAndOrientation = state.reachedDestination;
			if (state.reachedEndOfPathAndOrientation || state.reachedDestinationAndOrientation) {
				var reachedOrientation = ReachedDesiredOrientation(ref transform, ref movementPlane, ref destination);
				state.reachedEndOfPathAndOrientation &= reachedOrientation;
				state.reachedDestinationAndOrientation &= reachedOrientation;
			}
		}

		static bool ReachedDesiredOrientation (ref LocalTransform transform, ref AgentMovementPlane movementPlane, ref DestinationPoint destination) {
			var facingDirection2D = math.normalizesafe(movementPlane.value.ToPlane(destination.facingDirection));

			// If no desired facing direction is set, then we always treat the orientation as correct
			if (math.all(facingDirection2D == 0)) return true;

			var forward2D = math.normalizesafe(movementPlane.value.ToPlane(transform.Forward()));
			const float ANGLE_THRESHOLD_COS = 0.9999f;
			return math.dot(forward2D, facingDirection2D) >= ANGLE_THRESHOLD_COS;
		}
	}
}
#endif
