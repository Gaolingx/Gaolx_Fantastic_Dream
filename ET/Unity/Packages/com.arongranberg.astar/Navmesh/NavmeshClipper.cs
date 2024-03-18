namespace Pathfinding {
	using Pathfinding.Util;
	using Pathfinding.Graphs.Util;
	using UnityEngine;
	using System.Collections.Generic;

	/// <summary>Base class for the NavmeshCut and NavmeshAdd components</summary>
	[ExecuteAlways]
	public abstract class NavmeshClipper : VersionedMonoBehaviour {
		/// <summary>Called every time a NavmeshCut/NavmeshAdd component is enabled.</summary>
		static System.Action<NavmeshClipper> OnEnableCallback;

		/// <summary>Called every time a NavmeshCut/NavmeshAdd component is disabled.</summary>
		static System.Action<NavmeshClipper> OnDisableCallback;

		static readonly List<NavmeshClipper> all = new List<NavmeshClipper>();
		int listIndex = -1;

		/// <summary>
		/// Which graphs that are affected by this component.
		///
		/// You can use this to make a graph ignore a particular navmesh cut altogether.
		///
		/// Note that navmesh cuts can only affect navmesh/recast graphs.
		///
		/// If you change this field during runtime you must disable the component and enable it again for the changes to be detected.
		///
		/// See: <see cref="NavmeshBase.enableNavmeshCutting"/>
		/// </summary>
		public GraphMask graphMask = GraphMask.everything;

		public static void AddEnableCallback (System.Action<NavmeshClipper> onEnable,  System.Action<NavmeshClipper> onDisable) {
			OnEnableCallback += onEnable;
			OnDisableCallback += onDisable;
		}

		public static void RemoveEnableCallback (System.Action<NavmeshClipper> onEnable,  System.Action<NavmeshClipper> onDisable) {
			OnEnableCallback -= onEnable;
			OnDisableCallback -= onDisable;
		}

		/// <summary>
		/// All navmesh clipper components in the scene.
		/// Not ordered in any particular way.
		/// Warning: Do not modify this list
		/// </summary>
		public static List<NavmeshClipper> allEnabled { get { return all; } }

		protected virtual void OnEnable () {
			if (!Application.isPlaying) return;

			if (OnEnableCallback != null) OnEnableCallback(this);
			listIndex = all.Count;
			all.Add(this);
		}

		protected virtual void OnDisable () {
			if (!Application.isPlaying) return;

			// Efficient removal (the list doesn't need to be ordered).
			// Move the last item in the list to the slot occupied by this item
			// and then remove the last slot.
			all[listIndex] = all[all.Count-1];
			all[listIndex].listIndex = listIndex;
			all.RemoveAt(all.Count-1);
			listIndex = -1;
			if (OnDisableCallback != null) OnDisableCallback(this);
		}

		internal abstract void NotifyUpdated(GridLookup<NavmeshClipper>.Root previousState);
		public abstract Rect GetBounds(GraphTransform transform, float radiusMargin);
		public abstract bool RequiresUpdate(GridLookup<NavmeshClipper>.Root previousState);
		public abstract void ForceUpdate();
	}
}
