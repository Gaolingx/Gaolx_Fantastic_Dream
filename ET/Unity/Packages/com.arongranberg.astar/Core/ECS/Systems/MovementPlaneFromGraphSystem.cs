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

	[UpdateBefore(typeof(FollowerControlSystem))]
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[RequireMatchingQueriesForUpdateAttribute]
	[BurstCompile]
	public partial struct MovementPlaneFromGraphSystem : ISystem {
		public void OnDestroy (ref SystemState state) {}

		public void OnUpdate (ref SystemState systemState) {
			if (AstarPath.active == null) return;

			var graphs = AstarPath.active.data.graphs;
			if (graphs == null) return;

			var movementPlanes = new NativeArray<AgentMovementPlane>(graphs.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < graphs.Length; i++) {
				var graph = graphs[i];
				var plane = new NativeMovementPlane(quaternion.identity);
				if (graph is NavmeshBase navmesh) {
					plane = new NativeMovementPlane(navmesh.transform.rotation);
				} else if (graph is GridGraph grid) {
					plane = new NativeMovementPlane(grid.transform.rotation);
				}
				movementPlanes[i] = new AgentMovementPlane {
					value = plane,
				};
			}

			// This is faster to index into than a NativeArray
			var movementPlaneSpan = movementPlanes.AsUnsafeReadOnlySpan();
			foreach (var(managedState, movementSettings, movementPlane) in SystemAPI.Query<ManagedState, RefRO<MovementSettings>, RefRW<AgentMovementPlane> >()) {
				if (movementSettings.ValueRO.movementPlaneSource == MovementPlaneSource.Graph) {
					var node = managedState.pathTracer.startNode;
					if (node != null && !node.Destroyed) {
						var graphIndex = (int)node.GraphIndex;
						if (graphIndex >= 0 && graphIndex < movementPlaneSpan.Length) {
							movementPlane.ValueRW = movementPlaneSpan[graphIndex];
						} else {
							// This should not be possible if proper synchronization is adhered to.
							// But if users do something bad, just avoid updating the movement plane this frame
						}
					}
				}
			}
			movementPlanes.Dispose();
		}
	}
}
#endif
