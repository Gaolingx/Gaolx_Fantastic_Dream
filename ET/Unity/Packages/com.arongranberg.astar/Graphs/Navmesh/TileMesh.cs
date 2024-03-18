using Pathfinding.Util;

namespace Pathfinding.Graphs.Navmesh {
	/// <summary>
	/// A tile in a navmesh graph.
	///
	/// This is an intermediate representation used when building the navmesh, and also in some cases for serializing the navmesh to a portable format.
	///
	/// See: <see cref="NavmeshTile"/> for the representation used for pathfinding.
	/// </summary>
	public struct TileMesh {
		public int[] triangles;
		public Int3[] verticesInTileSpace;
		/// <summary>One tag per triangle</summary>
		public uint[] tags;

		/// <summary>Unsafe version of <see cref="TileMesh"/></summary>
		public struct TileMeshUnsafe {
			/// <summary>Three indices per triangle, of type int</summary>
			public Unity.Collections.LowLevel.Unsafe.UnsafeAppendBuffer triangles;
			/// <summary>One vertex per triangle, of type Int3</summary>
			public Unity.Collections.LowLevel.Unsafe.UnsafeAppendBuffer verticesInTileSpace;
			/// <summary>One tag per triangle, of type uint</summary>
			public Unity.Collections.LowLevel.Unsafe.UnsafeAppendBuffer tags;

			public void Dispose () {
				triangles.Dispose();
				verticesInTileSpace.Dispose();
				tags.Dispose();
			}

			public TileMesh ToManaged () {
				return new TileMesh {
						   triangles = Memory.UnsafeAppendBufferToArray<int>(triangles),
						   verticesInTileSpace = Memory.UnsafeAppendBufferToArray<Int3>(verticesInTileSpace),
						   tags = Memory.UnsafeAppendBufferToArray<uint>(tags),
				};
			}
		}
	}
}
