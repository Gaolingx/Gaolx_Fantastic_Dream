#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.PID;

	public struct MovementState : IComponentData {
		/// <summary>State of the PID controller for the movement</summary>
		public PIDMovement.PersistentState followerState;

		/// <summary>The next corner in the path</summary>
		public float3 nextCorner;

		/// <summary>
		/// The end of the current path.
		/// Note that the agent may be heading towards an off-mesh link which is not the same as this point.
		/// </summary>
		public float3 endOfPath;

		/// <summary>
		/// The closest point on the navmesh to the agent.
		/// The agent will be snapped to this point.
		/// </summary>
		public float3 closestOnNavmesh;
		public float3 positionOffset;

		/// <summary>
		/// The index of the hierarchical node that the agent is currently in.
		/// Will be -1 if the hierarchical node index is not known.
		///
		/// This field is valid during all system updates in the <see cref="AIMovementSystemGroup"/>. It is not guaranteed to be valid after that group has finished running, as graph updates may have changed the graph.
		///
		/// See: <see cref="HierarchicalGraph"/>
		/// </summary>
		public int hierarchicalNodeIndex;

		/// <summary>The remaining distance until the end of the path, or the next off-mesh link</summary>
		public float remainingDistanceToEndOfPart;

		/// <summary>
		/// The current additional rotation that is applied to the agent.
		/// This is used by the local avoidance system to rotate the agent, without this causing a feedback loop.
		///
		/// See: <see cref="ResolvedMovement.targetRotationOffset"/>
		/// </summary>
		public float rotationOffset;

		/// <summary>
		/// An additional, purely visual, rotation offset.
		/// This is used for rotation smoothing, but does not affect the movement of the agent.
		/// </summary>
		public float rotationOffset2;

		/// <summary>
		/// Version number of <see cref="PathTracer.version"/> when the movement state was last updated.
		/// In particular, <see cref="closestOnNavmesh"/>, <see cref="nextCorner"/>, <see cref="endOfPath"/>, <see cref="remainingDistanceToEndOfPart"/>, <see cref="reachedDestination"/> and <see cref="reachedEndOfPath"/> will only
		/// be considered up to date if this is equal to the current version number of the path tracer.
		/// </summary>
		public ushort pathTracerVersion;

		/// <summary>Bitmask for various flags</summary>
		ushort flags;

		const int ReachedDestinationFlag = 1 << 0;
		const int reachedDestinationAndOrientationFlag = 1 << 1;
		const int ReachedEndOfPathFlag = 1 << 2;
		const int reachedEndOfPathAndOrientationFlag = 1 << 3;
		const int ReachedEndOfPartFlag = 1 << 4;
		const int TraversingLastPartFlag = 1 << 5;

		/// <summary>
		/// True if the agent has reached its destination.
		/// The destination will be considered reached if all of these conditions are met:
		/// - The agent has a path
		/// - The path is not stale
		/// - The destination is not significantly below the agent's feet.
		/// - The destination is not significantly above the agent's head.
		/// - The agent is on the last part of the path (there are no more remaining off-mesh links).
		/// - The remaining distance to the end of the path + the distance from the end of the path to the destination is less than <see cref="MovementSettings.stopDistance"/>.
		/// </summary>
		public bool reachedDestination {
			get => (flags & ReachedDestinationFlag) != 0;
			set => flags = (ushort)((flags & ~ReachedDestinationFlag) | (value ? ReachedDestinationFlag : 0));
		}

		/// <summary>
		/// True if the agent has reached its destination and is facing the desired orientation.
		/// This will become true if all of these conditions are met:
		/// - <see cref="reachedDestination"/> is true
		/// - The agent is facing the desired facing direction as specified in <see cref="DestinationPoint.facingDirection"/>.
		/// </summary>
		public bool reachedDestinationAndOrientation {
			get => (flags & reachedDestinationAndOrientationFlag) != 0;
			set => flags = (ushort)((flags & ~reachedDestinationAndOrientationFlag) | (value ? reachedDestinationAndOrientationFlag : 0));
		}

		/// <summary>
		/// True if the agent has reached the end of the path.
		/// The end of the path will be considered reached if all of these conditions are met:
		/// - The agent has a path
		/// - The path is not stale
		/// - The end of the path is not significantly below the agent's feet.
		/// - The end of the path is not significantly above the agent's head.
		/// - The agent is on the last part of the path (there are no more remaining off-mesh links).
		/// - The remaining distance to the end of the path is less than <see cref="MovementSettings.stopDistance"/>.
		/// </summary>
		public bool reachedEndOfPath {
			get => (flags & ReachedEndOfPathFlag) != 0;
			set => flags = (ushort)((flags & ~ReachedEndOfPathFlag) | (value ? ReachedEndOfPathFlag : 0));
		}

		/// <summary>
		/// True if the agent has reached its destination and is facing the desired orientation.
		/// This will become true if all of these conditions are met:
		/// - <see cref="reachedEndOfPath"/> is true
		/// - The agent is facing the desired facing direction as specified in <see cref="DestinationPoint.facingDirection"/>.
		/// </summary>
		public bool reachedEndOfPathAndOrientation {
			get => (flags & reachedEndOfPathAndOrientationFlag) != 0;
			set => flags = (ushort)((flags & ~reachedEndOfPathAndOrientationFlag) | (value ? reachedEndOfPathAndOrientationFlag : 0));
		}

		/// <summary>
		/// True if the agent has reached the end of the current part in the path.
		/// The end of the current part will be considered reached if all of these conditions are met:
		/// - The agent has a path
		/// - The path is not stale
		/// - The end of the current part is not significantly below the agent's feet.
		/// - The end of the current part is not significantly above the agent's head.
		/// - The remaining distance to the end of the part is not significantly larger than the agent's radius.
		/// </summary>
		public bool reachedEndOfPart {
			get => (flags & ReachedEndOfPartFlag) != 0;
			set => flags = (ushort)((flags & ~ReachedEndOfPartFlag) | (value ? ReachedEndOfPartFlag : 0));
		}

		/// <summary>
		/// True if the agent is traversing the last part of the path.
		///
		/// If false, the agent will have to traverse at least one off-mesh link before it gets to its destination.
		/// </summary>
		public bool traversingLastPart {
			get => (flags & TraversingLastPartFlag) != 0;
			set => flags = (ushort)((flags & ~TraversingLastPartFlag) | (value ? TraversingLastPartFlag : 0));
		}

		/// <summary>Sets the appropriate fields to indicate that the agent has no path</summary>
		public void SetPathIsEmpty (UnityEngine.Vector3 agentPosition) {
			nextCorner = agentPosition;
			endOfPath = agentPosition;
			closestOnNavmesh = agentPosition;
			hierarchicalNodeIndex = -1;
			remainingDistanceToEndOfPart = float.PositiveInfinity;
			reachedEndOfPath = false;
			reachedDestination = false;
			reachedEndOfPart = false;
			reachedDestinationAndOrientation = false;
			reachedEndOfPathAndOrientation = false;
			traversingLastPart = true;
		}
	}
}
#endif
