using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Serialization;
using UnityEngine.Profiling;

namespace Pathfinding.Graphs.Navmesh {
	/// <summary>
	/// Helper for navmesh cut objects.
	/// Responsible for keeping track of which navmesh cuts have moved and coordinating graph updates to account for those changes.
	///
	/// See: navmeshcutting (view in online documentation for working links)
	/// See: <see cref="AstarPath.navmeshUpdates"/>
	/// See: <see cref="Pathfinding.NavmeshBase.enableNavmeshCutting"/>
	/// </summary>
	[System.Serializable]
	public class NavmeshUpdates {
		/// <summary>
		/// How often to check if an update needs to be done (real seconds between checks).
		/// For worlds with a very large number of NavmeshCut objects, it might be bad for performance to do this check every frame.
		/// If you think this is a performance penalty, increase this number to check less often.
		///
		/// For almost all games, this can be kept at 0.
		///
		/// If negative, no updates will be done. They must be manually triggered using <see cref="ForceUpdate"/>.
		///
		/// <code>
		/// // Check every frame (the default)
		/// AstarPath.active.navmeshUpdates.updateInterval = 0;
		///
		/// // Check every 0.1 seconds
		/// AstarPath.active.navmeshUpdates.updateInterval = 0.1f;
		///
		/// // Never check for changes
		/// AstarPath.active.navmeshUpdates.updateInterval = -1;
		/// // You will have to schedule updates manually using
		/// AstarPath.active.navmeshUpdates.ForceUpdate();
		/// </code>
		///
		/// You can also find this in the AstarPath inspector under Settings.
		/// [Open online documentation to see images]
		/// </summary>
		public float updateInterval;
		internal AstarPath astar;

		/// <summary>Last time navmesh cuts were applied</summary>
		float lastUpdateTime = float.NegativeInfinity;

		/// <summary>Stores navmesh cutting related data for a single graph</summary>
		public class NavmeshUpdateSettings {
			public TileHandler handler;
			public readonly List<IntRect> forcedReloadRects = new List<IntRect>();
			readonly NavmeshBase graph;

			public NavmeshUpdateSettings(NavmeshBase graph) {
				this.graph = graph;
			}

			public void ReloadAllTiles () {
				if (handler != null) handler.ReloadInBounds(new IntRect(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue));
			}

			public void Refresh (bool forceCreate = false) {
				if (!graph.enableNavmeshCutting) {
					if (handler != null) {
						handler.cuts.Clear();
						ReloadAllTiles();
						// Make sure the updates are applied immediately.
						// This is important because if navmesh cutting is enabled immediately after this
						// then it will call CreateTileTypesFromGraph, and we need to ensure that it is not
						// calling that when the graph still has cuts in it as they will then be baked in.
						graph.active.FlushGraphUpdates();
						graph.active.FlushWorkItems();

						forcedReloadRects.ClearFast();
						handler = null;
					}
				} else if ((handler == null && (forceCreate || NavmeshClipper.allEnabled.Count > 0)) || (handler != null && !handler.isValid)) {
					// Note: Only create a handler if there are any navmesh cuts in the scene.
					// We don't want to waste a lot of memory if navmesh cutting isn't actually used for anything
					// and even more important: we don't want to do any sporadic updates to the graph which
					// may clear the graph's tags or change it's structure (e.g from the delaunay optimization in the TileHandler).

					// The tile handler is invalid (or doesn't exist), so re-create it
					handler = new TileHandler(graph);
					for (int i = 0; i < NavmeshClipper.allEnabled.Count; i++) AddClipper(NavmeshClipper.allEnabled[i]);
					handler.CreateTileTypesFromGraph();

					// Reload in huge bounds. This will cause all tiles to be updated.
					forcedReloadRects.Add(new IntRect(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue));
				}
			}

			public void DiscardPending () {
				if (handler != null) {
					for (int j = 0; j < NavmeshClipper.allEnabled.Count; j++) {
						var cut = NavmeshClipper.allEnabled[j];
						var root = handler.cuts.GetRoot(cut);
						if (root != null) cut.NotifyUpdated(root);
					}
				}

				forcedReloadRects.Clear();
			}

			/// <summary>Called when the graph has been resized to a different tile count</summary>
			public void OnResized (IntRect newTileBounds) {
				if (handler == null) return;

				this.handler.Resize(newTileBounds);

				var characterRadius = graph.NavmeshCuttingCharacterRadius;

				// New tiles may have been created when resizing. If a cut was on the edge of the graph bounds,
				// it may intersect with the new tiles and we will need to recalculate them in that case.
				var allCuts = handler.cuts.AllItems;
				for (var cut = allCuts; cut != null; cut = cut.next) {
					var newGraphSpaceBounds = cut.obj.GetBounds(handler.graph.transform, characterRadius);
					var newTouchingTiles = handler.graph.GetTouchingTilesInGraphSpace(newGraphSpaceBounds);
					if (cut.previousBounds != newTouchingTiles) {
						handler.cuts.Dirty(cut.obj);
						handler.cuts.Move(cut.obj, newTouchingTiles);
					}
				}
			}

			/// <summary>Called when some tiles in a recast graph have been completely recalculated (e.g from scanning the graph)</summary>
			public void OnRecalculatedTiles (NavmeshTile[] tiles) {
				Refresh();
				if (handler != null) handler.OnRecalculatedTiles(tiles);

				// If the whole graph was updated then mark all navmesh cuts as being up to date.
				// If only a part of the graph was updated then a navmesh cut might be over the non-updated part
				// as well, and in that case we don't want to mark it as fully updated.
				if (graph.GetTiles().Length == tiles.Length) {
					DiscardPending();
				}
			}

			public void Dirty (NavmeshClipper obj) {
				// If we have no handler then we can ignore this. If we would later create a handler the object would be automatically dirtied anyway.
				if (handler == null) return;
				handler.cuts.Dirty(obj);
			}

			/// <summary>Called when a NavmeshCut or NavmeshAdd is enabled</summary>
			public void AddClipper (NavmeshClipper obj) {
				if (!obj.graphMask.Contains((int)graph.graphIndex)) return;

				// Without the forceCreate parameter set to true then no handler will be created
				// because there are no clippers in the scene yet. However one is being added right now.
				Refresh(true);
				if (handler == null) return;
				var characterRadius = graph.NavmeshCuttingCharacterRadius;
				var graphSpaceBounds = obj.GetBounds(graph.transform, characterRadius);
				var touchingTiles = handler.graph.GetTouchingTilesInGraphSpace(graphSpaceBounds);
				handler.cuts.Add(obj, touchingTiles);
			}

			/// <summary>Called when a NavmeshCut or NavmeshAdd is disabled</summary>
			public void RemoveClipper (NavmeshClipper obj) {
				Refresh();
				if (handler == null) return;
				var root = handler.cuts.GetRoot(obj);

				if (root != null) {
					forcedReloadRects.Add(root.previousBounds);
					handler.cuts.Remove(obj);
				}
			}
		}

		internal void OnEnable () {
			NavmeshClipper.AddEnableCallback(HandleOnEnableCallback, HandleOnDisableCallback);
		}

		internal void OnDisable () {
			NavmeshClipper.RemoveEnableCallback(HandleOnEnableCallback, HandleOnDisableCallback);
		}

		public void ForceUpdateAround (NavmeshClipper clipper) {
			var graphs = astar.graphs;

			if (graphs == null) return;

			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is NavmeshBase navmeshBase) navmeshBase.navmeshUpdateData.Dirty(clipper);
			}
		}

		/// <summary>Discards all pending updates caused by moved or modified navmesh cuts</summary>
		public void DiscardPending () {
			var graphs = astar.graphs;

			if (graphs == null) return;

			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is NavmeshBase navmeshBase) navmeshBase.navmeshUpdateData.DiscardPending();
			}
		}

		/// <summary>Called when a NavmeshCut or NavmeshAdd is enabled</summary>
		void HandleOnEnableCallback (NavmeshClipper obj) {
			var graphs = astar.graphs;

			if (graphs == null) return;

			for (int i = 0; i < graphs.Length; i++) {
				// Add the clipper to the individual graphs. Note that this automatically marks the clipper as dirty for that particular graph.
				if (graphs[i] is NavmeshBase navmeshBase) navmeshBase.navmeshUpdateData.AddClipper(obj);
			}
		}

		/// <summary>Called when a NavmeshCut or NavmeshAdd is disabled</summary>
		void HandleOnDisableCallback (NavmeshClipper obj) {
			var graphs = astar.graphs;

			if (graphs == null) return;

			for (int i = 0; i < graphs.Length; i++) {
				if (graphs[i] is NavmeshBase navmeshBase) navmeshBase.navmeshUpdateData.RemoveClipper(obj);
			}
			lastUpdateTime = float.NegativeInfinity;
		}

		/// <summary>Update is called once per frame</summary>
		internal void Update () {
			if (astar.isScanning) return;
			Profiler.BeginSample("Navmesh cutting");
			bool anyInvalidHandlers = false;
			var graphs = astar.graphs;

			if (graphs != null) {
				for (int i = 0; i < graphs.Length; i++) {
					var navmeshBase = graphs[i] as NavmeshBase;
					if (navmeshBase != null) {
						navmeshBase.navmeshUpdateData.Refresh();
						anyInvalidHandlers = navmeshBase.navmeshUpdateData.forcedReloadRects.Count > 0;
					}
				}

				if ((updateInterval >= 0 && Time.realtimeSinceStartup - lastUpdateTime > updateInterval) || anyInvalidHandlers) {
					ForceUpdate();
				}
			}
			Profiler.EndSample();
		}

		/// <summary>
		/// Checks all NavmeshCut instances and updates graphs if needed.
		/// Note: This schedules updates for all necessary tiles to happen as soon as possible.
		/// The pathfinding threads will continue to calculate the paths that they were calculating when this function
		/// was called and then they will be paused and the graph updates will be carried out (this may be several frames into the
		/// future and the graph updates themselves may take several frames to complete).
		/// If you want to force all navmesh cutting to be completed in a single frame call this method
		/// and immediately after call AstarPath.FlushWorkItems.
		///
		/// <code>
		/// // Schedule pending updates to be done as soon as the pathfinding threads
		/// // are done with what they are currently doing.
		/// AstarPath.active.navmeshUpdates.ForceUpdate();
		/// // Block until the updates have finished
		/// AstarPath.active.FlushGraphUpdates();
		/// </code>
		/// </summary>
		public void ForceUpdate () {
			lastUpdateTime = Time.realtimeSinceStartup;

			var graphs = astar.graphs;
			if (graphs == null) return;

			for (int graphIndex = 0; graphIndex < graphs.Length; graphIndex++) {
				var navmeshBase = graphs[graphIndex] as NavmeshBase;
				if (navmeshBase == null) continue;

				// Done in Update as well, but users may call ForceUpdate directly
				navmeshBase.navmeshUpdateData.Refresh();

				var handler = navmeshBase.navmeshUpdateData.handler;

				if (handler == null) continue;

				var forcedReloadRects = navmeshBase.navmeshUpdateData.forcedReloadRects;

				// Get all navmesh cuts in the scene
				var allCuts = handler.cuts.AllItems;

				if (forcedReloadRects.Count == 0) {
					bool any = false;

					// Check if any navmesh cuts need updating
					for (var cut = allCuts; cut != null; cut = cut.next) {
						if (cut.obj.RequiresUpdate(cut)) {
							any = true;
							break;
						}
					}

					// Nothing needs to be done for now
					if (!any) continue;
				}

				// Start batching tile updates which is good for performance
				// if we are updating a lot of them
				handler.StartBatchLoad();

				for (int i = 0; i < forcedReloadRects.Count; i++) {
					handler.ReloadInBounds(forcedReloadRects[i]);
				}
				forcedReloadRects.ClearFast();

				var characterRadius = handler.graph.NavmeshCuttingCharacterRadius;
				// Reload all bounds touching the previous bounds and current bounds
				// of navmesh cuts that have moved or changed in some other way
				for (var cut = allCuts; cut != null; cut = cut.next) {
					if (cut.obj.RequiresUpdate(cut)) {
						// Make sure the tile where it was is updated
						handler.ReloadInBounds(cut.previousBounds);

						var newGraphSpaceBounds = cut.obj.GetBounds(handler.graph.transform, characterRadius);
						var newTouchingTiles = handler.graph.GetTouchingTilesInGraphSpace(newGraphSpaceBounds);
						handler.cuts.Move(cut.obj, newTouchingTiles);
						handler.ReloadInBounds(newTouchingTiles);

						// Notify the navmesh cut that it has been updated in this graph
						// This will cause RequiresUpdate to return false
						// until it is changed again.
						cut.obj.NotifyUpdated(cut);
					}
				}

				handler.EndBatchLoad();
			}
		}
	}
}
