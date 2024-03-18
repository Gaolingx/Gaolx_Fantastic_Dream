using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Transforms vertices from voxel coordinates to tile coordinates.
	///
	/// This essentially constitutes multiplying the vertices by the <see cref="matrix"/>.
	///
	/// Note: The input space is in raw voxel coordinates, the output space is in tile coordinates stored in millimeters (as is typical for the Int3 struct. See <see cref="Int3.Precision"/>).
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobTransformTileCoordinates : IJob {
		/// <summary>Element type Int3</summary>
		public unsafe UnsafeAppendBuffer* vertices;
		public Matrix4x4 matrix;

		public void Execute () {
			unsafe {
				int vertexCount = vertices->Length / UnsafeUtility.SizeOf<Int3>();
				for (int i = 0; i < vertexCount; i++) {
					// Transform from voxel indices to a proper Int3 coordinate, then convert it to a Vector3 float coordinate
					var vPtr1 = (Int3*)vertices->Ptr + i;
					var p = new Vector3(vPtr1->x, vPtr1->y, vPtr1->z);
					*vPtr1 = (Int3)matrix.MultiplyPoint3x4(p);
				}
			}
		}
	}
}
