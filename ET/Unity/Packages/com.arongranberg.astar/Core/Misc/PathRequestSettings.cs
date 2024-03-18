namespace Pathfinding {
	[System.Serializable]
	public struct PathRequestSettings {
		/// <summary>
		/// Graphs that this agent can use.
		/// This field determines which graphs will be considered when searching for the start and end nodes of a path.
		/// It is useful in numerous situations, for example if you want to make one graph for small units and one graph for large units.
		///
		/// This is a bitmask so if you for example want to make the agent only use graph index 3 then you can set this to:
		/// <code> settings.graphMask = 1 << 3; </code>
		///
		/// See: bitmasks (view in online documentation for working links)
		///
		/// Note that this field only stores which graph indices that are allowed. This means that if the graphs change their ordering
		/// then this mask may no longer be correct.
		///
		/// If you know the name of the graph you can use the <see cref="Pathfinding.GraphMask.FromGraphName"/> method:
		/// <code>
		/// GraphMask mask1 = GraphMask.FromGraphName("My Grid Graph");
		/// GraphMask mask2 = GraphMask.FromGraphName("My Other Grid Graph");
		///
		/// NNConstraint nn = NNConstraint.Walkable;
		///
		/// nn.graphMask = mask1 | mask2;
		///
		/// // Find the node closest to somePoint which is either in 'My Grid Graph' OR in 'My Other Grid Graph'
		/// var info = AstarPath.active.GetNearest(somePoint, nn);
		/// </code>
		///
		/// See: multiple-agent-types (view in online documentation for working links)
		/// </summary>
		public GraphMask graphMask;

		/// <summary>
		/// The penalty for each tag.
		///
		/// If null, all penalties will be treated as zero. Otherwise, the array should always have a length of exactly 32.
		/// </summary>
		public int[] tagPenalties;

		/// <summary>
		/// The tags which this agent can traverse.
		///
		/// Note: This field is a bitmask.
		/// See: bitmasks (view in online documentation for working links)
		/// </summary>
		public int traversableTags;

		/// <summary>
		/// Filters which nodes the agent can traverse, and can also add penalties to each traversed node.
		///
		/// In most common situations, this is left as null (which implies the default traversal provider: <see cref="DefaultITraversalProvider"/>).
		/// But if you need custom pathfinding behavior which cannot be done using the <see cref="graphMask"/>, <see cref="tagPenalties"/> and <see cref="traversableTags"/>, then setting an <see cref="ITraversalProvider"/> is a great option.
		/// It provides you a lot more control over how the pathfinding works.
		///
		/// <code>
		/// followerEntity.pathfindingSettings.traversalProvider = new MyCustomTraversalProvider();
		/// </code>
		///
		/// See: traversal_provider (view in online documentation for working links)
		/// </summary>
		public ITraversalProvider traversalProvider;

		public static PathRequestSettings Default => new PathRequestSettings {
			graphMask = GraphMask.everything,
			tagPenalties = new int[32],
			traversableTags = -1,
			traversalProvider = null,
		};
	}
}
