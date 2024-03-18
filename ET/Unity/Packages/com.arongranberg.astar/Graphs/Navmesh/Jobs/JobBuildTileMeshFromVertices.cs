using Pathfinding.Util;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Builds tiles from raw mesh vertices and indices.
	///
	/// This job takes the following steps:
	/// - Transform all vertices using the <see cref="meshToGraph"/> matrix.
	/// - Remove duplicate vertices
	/// - If <see cref="recalculateNormals"/> is enabled: ensure all triangles are laid out in the clockwise direction.
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Default)]
	public struct JobBuildTileMeshFromVertices : IJob {
		public NativeArray<Vector3> vertices;
		public NativeArray<int> indices;
		public Matrix4x4 meshToGraph;
		public NativeArray<TileMesh.TileMeshUnsafe> outputBuffers;
		public bool recalculateNormals;


		[BurstCompile(FloatMode = FloatMode.Fast)]
		public struct JobTransformTileCoordinates : IJob {
			public NativeArray<Vector3> vertices;
			public NativeArray<Int3> outputVertices;
			public Matrix4x4 matrix;

			public void Execute () {
				for (int i = 0; i < vertices.Length; i++) {
					outputVertices[i] = (Int3)matrix.MultiplyPoint3x4(vertices[i]);
				}
			}
		}

		public struct BuildNavmeshOutput : IProgress, System.IDisposable {
			public NativeArray<TileMesh.TileMeshUnsafe> tiles;

			public float Progress => 0.0f;

			public void Dispose () {
				for (int i = 0; i < tiles.Length; i++) tiles[i].Dispose();
				tiles.Dispose();
			}
		}

		public static Promise<BuildNavmeshOutput> Schedule (NativeArray<Vector3> vertices, NativeArray<int> indices, Matrix4x4 meshToGraph, bool recalculateNormals) {
			if (vertices.Length > NavmeshBase.VertexIndexMask) throw new System.ArgumentException("Too many vertices in the navmesh graph. Provided " + vertices.Length + ", but the maximum number of vertices per tile is " + NavmeshBase.VertexIndexMask + ". You can raise this limit by enabling ASTAR_RECAST_LARGER_TILES in the A* Inspector Optimizations tab");

			var outputBuffers = new NativeArray<TileMesh.TileMeshUnsafe>(1, Allocator.Persistent);

			var job = new JobBuildTileMeshFromVertices {
				vertices = vertices,
				indices = indices,
				meshToGraph = meshToGraph,
				outputBuffers = outputBuffers,
				recalculateNormals = recalculateNormals,
			}.Schedule();
			return new Promise<BuildNavmeshOutput>(job, new BuildNavmeshOutput {
				tiles = outputBuffers,
			});
		}

		public void Execute () {
			var int3vertices = new NativeArray<Int3>(vertices.Length, Allocator.Temp);
			var tags = new NativeArray<int>(indices.Length / 3, Allocator.Temp, NativeArrayOptions.ClearMemory);

			new JobTransformTileCoordinates {
				vertices = vertices,
				outputVertices = int3vertices,
				matrix = meshToGraph,
			}.Execute();

			unsafe {
				UnityEngine.Assertions.Assert.IsTrue(this.outputBuffers.Length == 1);
				var tile = (TileMesh.TileMeshUnsafe*) this.outputBuffers.GetUnsafePtr();
				var outputVertices = &tile->verticesInTileSpace;
				var outputTriangles = &tile->triangles;
				var outputTags = &tile->tags;
				*outputVertices = new UnsafeAppendBuffer(0, 4, Allocator.Persistent);
				*outputTriangles = new UnsafeAppendBuffer(0, 4, Allocator.Persistent);
				*outputTags = new UnsafeAppendBuffer(0, 4, Allocator.Persistent);
				new MeshUtility.JobRemoveDuplicateVertices {
					vertices = int3vertices,
					triangles = indices,
					tags = tags,
					outputVertices = outputVertices,
					outputTriangles = outputTriangles,
					outputTags = outputTags,
				}.Execute();

				if (recalculateNormals) {
					var verticesSpan = outputVertices->AsUnsafeSpan<Int3>();
					var trianglesSpan = outputTriangles->AsUnsafeSpan<int>();
					MeshUtility.MakeTrianglesClockwise(ref verticesSpan, ref trianglesSpan);
				}
			}

			int3vertices.Dispose();
		}
	}
}
