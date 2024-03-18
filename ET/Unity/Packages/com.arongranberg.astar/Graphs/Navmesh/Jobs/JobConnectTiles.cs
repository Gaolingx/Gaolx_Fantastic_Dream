using Unity.Jobs;
using UnityEngine;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Connects two adjacent tiles together.
	///
	/// This only creates connections between tiles. Connections internal to a tile should be handled by <see cref="JobCalculateTriangleConnections"/>.
	///
	/// Use the <see cref="ScheduleBatch"/> method to connect a bunch of tiles efficiently using maximum parallelism.
	/// </summary>
	public struct JobConnectTiles : IJob {
		/// <summary>GCHandle referring to a NavmeshTile[] array of size tileRect.Width*tileRect.Height</summary>
		public System.Runtime.InteropServices.GCHandle tiles;
		/// <summary>Index of the first tile in the <see cref="tiles"/> array</summary>
		public int tileIndex1;
		/// <summary>Index of the second tile in the <see cref="tiles"/> array</summary>
		public int tileIndex2;
		/// <summary>Size of a tile in world units along the graph's X axis</summary>
		public float tileWorldSizeX;
		/// <summary>Size of a tile in world units along the graph's Z axis</summary>
		public float tileWorldSizeZ;
		/// <summary>Maximum vertical distance between two tiles to create a connection between them</summary>
		public float maxTileConnectionEdgeDistance;

		/// <summary>
		/// Schedule jobs to connect all the given tiles with each other while exploiting as much parallelism as possible.
		/// tilesHandle should be a GCHandle referring to a NavmeshTile[] array of size tileRect.Width*tileRect.Height.
		/// </summary>
		public static JobHandle ScheduleBatch (System.Runtime.InteropServices.GCHandle tilesHandle, JobHandle dependency, IntRect tileRect, Vector2 tileWorldSize, float maxTileConnectionEdgeDistance) {
			// First connect all tiles with an EVEN coordinate sum
			// This would be the white squares on a chess board.
			// Then connect all tiles with an ODD coordinate sum (which would be all black squares on a chess board).
			// This will prevent the different threads that do all
			// this in parallel from conflicting with each other.
			// The directions are also done separately
			// first they are connected along the X direction and then along the Z direction.
			// Looping over 0 and then 1
			var tileRectDepth = tileRect.Height;
			var tileRectWidth = tileRect.Width;
			var coordinateDependency = dependency;
			for (int coordinateSum = 0; coordinateSum <= 1; coordinateSum++) {
				var dep = coordinateDependency;
				for (int direction = 0; direction <= 1; direction++) {
					for (int z = 0; z < tileRectDepth; z++) {
						for (int x = 0; x < tileRectWidth; x++) {
							if ((x + z) % 2 == coordinateSum) {
								int tileIndex1 = x + z * tileRectWidth;
								int tileIndex2;
								if (direction == 0 && x < tileRectWidth - 1) {
									tileIndex2 = x + 1 + z * tileRectWidth;
								} else if (direction == 1 && z < tileRectDepth - 1) {
									tileIndex2 = x + (z + 1) * tileRectWidth;
								} else {
									continue;
								}

								var job = new JobConnectTiles {
									tiles = tilesHandle,
									tileIndex1 = tileIndex1,
									tileIndex2 = tileIndex2,
									tileWorldSizeX = tileWorldSize.x,
									tileWorldSizeZ = tileWorldSize.y,
									maxTileConnectionEdgeDistance = maxTileConnectionEdgeDistance,
								}.Schedule(coordinateDependency);
								dep = JobHandle.CombineDependencies(dep, job);
							}
						}
					}

					coordinateDependency = dep;
				}
			}

			return coordinateDependency;
		}

		public void Execute () {
			var tiles = (NavmeshTile[])this.tiles.Target;

			NavmeshBase.ConnectTiles(tiles[tileIndex1], tiles[tileIndex2], tileWorldSizeX, tileWorldSizeZ, maxTileConnectionEdgeDistance);
		}
	}
}
