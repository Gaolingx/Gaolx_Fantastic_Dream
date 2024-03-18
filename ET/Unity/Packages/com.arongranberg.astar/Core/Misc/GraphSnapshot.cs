using System.Collections.Generic;
using UnityEngine.Profiling;

namespace Pathfinding.Util {
	public interface IGraphSnapshot : System.IDisposable {
		void Restore(IGraphUpdateContext ctx);
	}

	/// <summary>
	/// A snapshot of parts of graphs.
	///
	/// See: <see cref="AstarPath.Snapshot"/>
	/// </summary>
	public struct GraphSnapshot : IGraphSnapshot {
		List<IGraphSnapshot> inner;

		internal GraphSnapshot (List<IGraphSnapshot> inner) {
			this.inner = inner;
		}

		public void Restore (IGraphUpdateContext ctx) {
			Profiler.BeginSample("Restoring Graph Snapshot");
			for (int i = 0; i < inner.Count; i++) {
				inner[i].Restore(ctx);
			}
			Profiler.EndSample();
		}

		public void Dispose () {
			if (inner != null) {
				for (int i = 0; i < inner.Count; i++) {
					inner[i].Dispose();
				}
				inner = null;
			}
		}
	}
}
