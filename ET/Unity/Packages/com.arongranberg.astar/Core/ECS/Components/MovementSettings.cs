#if MODULE_ENTITIES
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;
	using Pathfinding.PID;

	/// <summary>How to calculate which direction is "up" for the agent</summary>
	public enum MovementPlaneSource : byte {
		/// <summary>
		/// The graph's natural up direction will be used to align the agent.
		/// This is the most common option.
		/// </summary>
		Graph,
		/// <summary>
		/// The agent will be aligned with the ground normal.
		///
		/// This is useful when you have a spherical world, or some other strange shape.
		///
		/// Note: If you have a somewhat flat world, and you want to align the agent to the ground, this is not the option you want.
		/// Instead, you might want to disable <see cref="FollowerEntity.updateRotation"/> and then align the transform using a custom script.
		///
		/// Warning: Using this option carries a performance penalty.
		/// </summary>
		Raycast,
	}

	[System.Serializable]
	public struct MovementSettings : IComponentData {
		/// <summary>Additional movement settings</summary>
		public PIDMovement follower;

		/// <summary>Flags for enabling debug rendering in the scene view</summary>
		public PIDMovement.DebugFlags debugFlags;

		/// <summary>
		/// How far away from the destination should the agent aim to stop, in world units.
		///
		/// If the agent is within this distance from the destination point it will be considered to have reached the destination.
		///
		/// Even if you want the agent to stop precisely at a given point, it is recommended to keep this slightly above zero.
		/// If it is exactly zero, the agent may have a hard time deciding that it
		/// has actually reached the end of the path, due to floating point errors and such.
		///
		/// Note: This will not be multiplied the agent's scale.
		/// </summary>
		public float stopDistance;

		/// <summary>
		/// How much to smooth the visual rotation of the agent.
		///
		/// This does not affect movement, but smoothes out how the agent rotates visually.
		///
		/// Recommended values are between 0.0 and 0.5.
		/// A value of zero will disable smoothing completely.
		///
		/// The smoothing is done primarily using an exponential moving average, but with
		/// a small linear term to make the rotation converge faster when the agent is almost facing the desired direction.
		///
		/// Adding smoothing will make the visual rotation of the agent lag a bit behind the actual rotation.
		/// Too much smoothing may make the agent seem sluggish, and appear to move sideways.
		///
		/// The unit for this field is seconds.
		/// </summary>
		public float rotationSmoothing;
		public float positionSmoothing;

		/// <summary>
		/// Layer mask to use for ground placement.
		/// Make sure this does not include the layer of any colliders attached to this gameobject.
		///
		/// See: <see cref="GravityState"/>
		/// See: https://docs.unity3d.com/Manual/Layers.html
		/// </summary>
		public LayerMask groundMask;

		/// <summary>
		/// How to calculate which direction is "up" for the agent.
		/// See: <see cref="MovementPlaneSource"/>
		/// </summary>
		public MovementPlaneSource movementPlaneSource;

		/// <summary>\copydocref{IAstarAI.isStopped}</summary>
		public bool isStopped;
	}
}
#endif
