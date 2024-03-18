using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Pathfinding.Jobs;
using Pathfinding.Util;
using System.Data;
using UnityEngine.Assertions;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Calculates the grid connections for all nodes.
	///
	/// This is a IJobParallelForBatch job. Calculating the connections in multiple threads is faster,
	/// but due to hyperthreading (used on most intel processors) the individual threads will become slower.
	/// It is still worth it though.
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast, CompileSynchronously = true)]
	public struct JobCalculateGridConnections : IJobParallelForBatched {
		public float maxStepHeight;
		/// <summary>Normalized up direction</summary>
		public Vector3 up;
		public IntBounds bounds;
		public int3 arrayBounds;
		public NumNeighbours neighbours;
		public bool use2D;
		public bool cutCorners;
		public bool maxStepUsesSlope;
		public float characterHeight;
		public bool layeredDataLayout;

		[ReadOnly]
		public UnsafeSpan<bool> nodeWalkable;

		[ReadOnly]
		public UnsafeSpan<float4> nodeNormals;

		[ReadOnly]
		public UnsafeSpan<Vector3> nodePositions;

		/// <summary>All bitpacked node connections</summary>
		[WriteOnly]
		public UnsafeSpan<ulong> nodeConnections;

		public bool allowBoundsChecks => false;


		/// <summary>
		/// Check if a connection to node B is valid.
		/// Node A is assumed to be walkable already
		/// </summary>
		public static bool IsValidConnection (float4 nodePosA, float4 nodeNormalA, bool nodeWalkableB, float4 nodePosB, float4 nodeNormalB, bool maxStepUsesSlope, float maxStepHeight, float4 up) {
			if (!nodeWalkableB) return false;

			if (!maxStepUsesSlope) {
				// Check their differences along the Y coordinate (well, the up direction really. It is not necessarily the Y axis).
				return math.abs(math.dot(up, nodePosB - nodePosA)) <= maxStepHeight;
			} else {
				float4 v = nodePosB - nodePosA;
				float heightDifference = math.dot(v, up);

				// Check if the step is small enough.
				// This is a fast path for the common case.
				if (math.abs(heightDifference) <= maxStepHeight) return true;

				float4 v_flat = (v - heightDifference * up) * 0.5f;

				// Math!
				// Calculates the approximate offset along the up direction
				// that the ground will have moved at the midpoint between the
				// nodes compared to the nodes' center points.
				float NDotU = math.dot(nodeNormalA, up);
				float offsetA = -math.dot(nodeNormalA - NDotU * up, v_flat);

				NDotU = math.dot(nodeNormalB, up);
				float offsetB = math.dot(nodeNormalB - NDotU * up, v_flat);

				// Check the height difference with slopes taken into account.
				// Note that since we also do the heightDifference check above we will ensure slope offsets do not increase the height difference.
				// If we allowed this then some connections might not be valid near the start of steep slopes.
				return math.abs(heightDifference + offsetB - offsetA) <= maxStepHeight;
			}
		}

		public void Execute (int start, int count) {
			if (layeredDataLayout) ExecuteLayered(start, count);
			else ExecuteFlat(start, count);
		}

		public void ExecuteFlat (int start, int count) {
			if (maxStepHeight <= 0 || use2D) maxStepHeight = float.PositiveInfinity;

			float4 up = new float4(this.up.x, this.up.y, this.up.z, 0);

			NativeArray<int> neighbourOffsets = new NativeArray<int>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < 8; i++) neighbourOffsets[i] = GridGraph.neighbourZOffsets[i] * arrayBounds.x + GridGraph.neighbourXOffsets[i];
			var nodePositions = this.nodePositions.Reinterpret<float3>();

			// The loop is parallelized over z coordinates
			start += bounds.min.z;
			for (int z = start; z < start + count; z++) {
				var initialConnections = 0xFF;

				// Disable connections to out-of-bounds nodes
				// See GridNode.HasConnectionInDirection
				if (z == 0) initialConnections &= ~((1 << 0) | (1 << 7) | (1 << 4));
				if (z == arrayBounds.z - 1) initialConnections &= ~((1 << 2) | (1 << 5) | (1 << 6));

				for (int x = bounds.min.x; x < bounds.max.x; x++) {
					int nodeIndex = z * arrayBounds.x + x;
					if (!nodeWalkable[nodeIndex]) {
						nodeConnections[nodeIndex] = 0;
						continue;
					}

					// Bitpacked connections
					// bit 0 is set if connection 0 is enabled
					// bit 1 is set if connection 1 is enabled etc.
					int conns = initialConnections;

					// Disable connections to out-of-bounds nodes
					if (x == 0) conns &= ~((1 << 3) | (1 << 6) | (1 << 7));
					if (x == arrayBounds.x - 1) conns &= ~((1 << 1) | (1 << 4) | (1 << 5));

					float4 pos = new float4(nodePositions[nodeIndex], 0);
					float4 normal = nodeNormals[nodeIndex];

					for (int i = 0; i < 8; i++) {
						int neighbourIndex = nodeIndex + neighbourOffsets[i];
						if ((conns & (1 << i)) != 0 && !IsValidConnection(pos, normal, nodeWalkable[neighbourIndex], new float4(nodePositions[neighbourIndex], 0), nodeNormals[neighbourIndex], maxStepUsesSlope, maxStepHeight, up)) {
							// Enable connection i
							conns &= ~(1 << i);
						}
					}

					nodeConnections[nodeIndex] = (ulong)GridNode.FilterDiagonalConnections(conns, neighbours, cutCorners);
				}
			}
		}

		public void ExecuteLayered (int start, int count) {
			if (maxStepHeight <= 0 || use2D) maxStepHeight = float.PositiveInfinity;

			float4 up = new float4(this.up.x, this.up.y, this.up.z, 0);

			NativeArray<int> neighbourOffsets = new NativeArray<int>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < 8; i++) neighbourOffsets[i] = GridGraph.neighbourZOffsets[i] * arrayBounds.x + GridGraph.neighbourXOffsets[i];

			var layerStride = arrayBounds.z*arrayBounds.x;
			start += bounds.min.z;
			for (int y = bounds.min.y; y < bounds.max.y; y++) {
				// The loop is parallelized over z coordinates
				for (int z = start; z < start + count; z++) {
					for (int x = bounds.min.x; x < bounds.max.x; x++) {
						// Bitpacked connections
						ulong conns = 0;
						int nodeIndexXZ = z * arrayBounds.x + x;
						int nodeIndex = nodeIndexXZ + y * layerStride;
						float4 pos = new float4(nodePositions[nodeIndex], 0);
						float4 normal = nodeNormals[nodeIndex];

						if (nodeWalkable[nodeIndex]) {
							var ourY = math.dot(up, pos);

							float ourHeight;
							if (y == arrayBounds.y-1 || !math.any(nodeNormals[nodeIndex + layerStride])) {
								ourHeight = float.PositiveInfinity;
							} else {
								var nodeAboveNeighbourPos = new float4(nodePositions[nodeIndex + layerStride], 0);
								ourHeight = math.max(0, math.dot(up, nodeAboveNeighbourPos) - ourY);
							}

							for (int i = 0; i < 8; i++) {
								int nx = x + GridGraph.neighbourXOffsets[i];
								int nz = z + GridGraph.neighbourZOffsets[i];

								// Check if the new position is inside the grid
								int conn = LevelGridNode.NoConnection;
								if (nx >= 0 && nz >= 0 && nx < arrayBounds.x && nz < arrayBounds.z) {
									int neighbourStartIndex = nodeIndexXZ + neighbourOffsets[i];
									for (int y2 = 0; y2 < arrayBounds.y; y2++) {
										var neighbourIndex = neighbourStartIndex + y2 * layerStride;
										float4 nodePosB = new float4(nodePositions[neighbourIndex], 0);
										var neighbourY = math.dot(up, nodePosB);
										// Is there a node above this one
										float neighbourHeight;
										if (y2 == arrayBounds.y-1 || !math.any(nodeNormals[neighbourIndex + layerStride])) {
											neighbourHeight = float.PositiveInfinity;
										} else {
											var nodeAboveNeighbourPos = new float4(nodePositions[neighbourIndex + layerStride], 0);
											neighbourHeight = math.max(0, math.dot(up, nodeAboveNeighbourPos) - neighbourY);
										}

										float bottom = math.max(neighbourY, ourY);
										float top = math.min(neighbourY + neighbourHeight, ourY + ourHeight);

										float dist = top-bottom;

										if (dist >= characterHeight && IsValidConnection(pos, normal, nodeWalkable[neighbourIndex], new float4(nodePositions[neighbourIndex], 0), nodeNormals[neighbourIndex], maxStepUsesSlope, maxStepHeight, up)) {
											conn = y2;
										}
									}
								}

								conns |= (ulong)conn << LevelGridNode.ConnectionStride*i;
							}
						} else {
							conns = LevelGridNode.AllConnectionsMask;
						}

						nodeConnections[nodeIndex] = conns;
					}
				}
			}
		}
	}
}
