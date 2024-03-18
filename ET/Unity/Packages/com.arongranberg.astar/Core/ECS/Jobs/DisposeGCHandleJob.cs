using Unity.Jobs;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Pathfinding.ECS {
	/// <summary>Disposes a GCHandle when the job executes</summary>
	public partial struct DisposeGCHandleJob : IJob {
		public GCHandle handle;

		public void Execute () {
			handle.Free();
		}
	}
}
