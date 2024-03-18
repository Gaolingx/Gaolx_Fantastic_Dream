using Unity.Jobs;
using Unity.Mathematics;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Adds nodes to the hierarchical graph dirty list.
	///
	/// This is used when updating and scanning the graph to mark nodes as dirty so that the hierarchical graph can recalculate connected components and obstacle contours.
	///
	/// See: <see cref="HierarchicalGraph"/>
	/// </summary>
	public struct JobDirtyNodes : IJob {
		// Target=GridNodeBase[]. This is a managed type, we need to trick Unity to allow this inside of a job
		public System.Runtime.InteropServices.GCHandle nodesHandle;
		/// <summary>(width, height, depth) of the array that the <see cref="nodesHandle"/> refers to</summary>
		public int3 nodeArrayBounds;
		public IntBounds dataBounds;

		public void Execute () {
			var nodes = (GridNodeBase[])nodesHandle.Target;
			var hGraph = AstarPath.active.hierarchicalGraph;
			for (int y = dataBounds.min.y; y < dataBounds.max.y; y++) {
				for (int z = dataBounds.min.z; z < dataBounds.max.z; z++) {
					var rowOffset = y*nodeArrayBounds.x*nodeArrayBounds.z + z*nodeArrayBounds.x;
					for (int x = dataBounds.min.x; x < dataBounds.max.x; x++) {
						var node = nodes[rowOffset + x];
						// Marking nodes as dirty is not safe to do in parallel.
						// Graphs are scanned in parallel, so that would mean this is unsafe.
						// But luckily when a graph is scanned, all nodes will already be dirty,
						// so these calls are essentially no-ops (and safe).
						// It will only do stuff when we do a graph update, which
						// are not done in parallel (yet).
						if (node != null) hGraph.AddDirtyNode(node);
					}
				}
			}
		}
	}
}
