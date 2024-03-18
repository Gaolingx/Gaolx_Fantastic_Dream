using Pathfinding.Jobs;
using Pathfinding.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Builds nodes and tiles and prepares them for pathfinding.
	///
	/// Takes input from a <see cref="TileBuilder"/> job and outputs a <see cref="BuildNodeTilesOutput"/>.
	///
	/// This job takes the following steps:
	/// - Calculate connections between nodes inside each tile
	/// - Create node and tile objects
	/// - Connect adjacent tiles together
	/// </summary>
	public struct JobBuildNodes {
		AstarPath astar;
		uint graphIndex;
		public uint initialPenalty;
		public bool recalculateNormals;
		public float maxTileConnectionEdgeDistance;
		Matrix4x4 graphToWorldSpace;
		TileLayout tileLayout;

		public class BuildNodeTilesOutput : IProgress, System.IDisposable {
			public TileBuilder.TileBuilderOutput dependency;
			public NavmeshTile[] tiles;

			public float Progress => dependency.Progress;

			public void Dispose () {
			}
		}

		internal JobBuildNodes(RecastGraph graph, TileLayout tileLayout) {
			this.astar = graph.active;
			this.tileLayout = tileLayout;
			this.graphIndex = graph.graphIndex;
			this.initialPenalty = graph.initialPenalty;
			this.recalculateNormals = graph.RecalculateNormals;
			this.maxTileConnectionEdgeDistance = graph.MaxTileConnectionEdgeDistance;
			this.graphToWorldSpace = tileLayout.transform.matrix;
		}

		public Promise<BuildNodeTilesOutput> Schedule (DisposeArena arena, Promise<TileBuilder.TileBuilderOutput> dependency) {
			var input = dependency.GetValue();
			var tileRect = input.tileMeshes.tileRect;
			UnityEngine.Assertions.Assert.AreEqual(input.tileMeshes.tileMeshes.Length, tileRect.Area);
			var tiles = new NavmeshTile[tileRect.Area];
			var tilesGCHandle = System.Runtime.InteropServices.GCHandle.Alloc(tiles);
			var nodeConnections = new NativeArray<JobCalculateTriangleConnections.TileNodeConnectionsUnsafe>(tileRect.Area, Allocator.Persistent);

			var calculateConnectionsJob = new JobCalculateTriangleConnections {
				tileMeshes = input.tileMeshes.tileMeshes,
				nodeConnections = nodeConnections,
			}.Schedule(dependency.handle);

			var tileWorldSize = new Vector2(tileLayout.TileWorldSizeX, tileLayout.TileWorldSizeZ);
			var createTilesJob = new JobCreateTiles {
				tileMeshes = input.tileMeshes.tileMeshes,
				tiles = tilesGCHandle,
				tileRect = tileRect,
				graphTileCount = tileLayout.tileCount,
				graphIndex = graphIndex,
				initialPenalty = initialPenalty,
				recalculateNormals = recalculateNormals,
				graphToWorldSpace = this.graphToWorldSpace,
				tileWorldSize = tileWorldSize,
			}.Schedule(dependency.handle);

			var applyConnectionsJob = new JobWriteNodeConnections {
				nodeConnections = nodeConnections,
				tiles = tilesGCHandle,
			}.Schedule(JobHandle.CombineDependencies(calculateConnectionsJob, createTilesJob));

			Profiler.BeginSample("Scheduling ConnectTiles");
			var connectTilesDependency = JobConnectTiles.ScheduleBatch(tilesGCHandle, applyConnectionsJob, tileRect, tileWorldSize, maxTileConnectionEdgeDistance);
			Profiler.EndSample();

			arena.Add(tilesGCHandle);
			arena.Add(nodeConnections);

			return new Promise<BuildNodeTilesOutput>(connectTilesDependency, new BuildNodeTilesOutput {
				dependency = input,
				tiles = tiles,
			});
		}
	}
}
