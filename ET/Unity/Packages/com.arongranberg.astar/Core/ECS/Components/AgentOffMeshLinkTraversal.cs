#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.Util;
	using UnityEngine;

	/// <summary>
	/// Holds unmanaged information about an off-mesh link that the agent is currently traversing.
	/// This component is added to the agent when it starts traversing an off-mesh link.
	/// It is removed when the agent has finished traversing the link.
	///
	/// See: <see cref="ManagedAgentOffMeshLinkTraversal"/>
	/// </summary>
	public struct AgentOffMeshLinkTraversal : IComponentData {
		/// <summary>\copydocref{PathTracer.LinkInfo.relativeStart}</summary>
		public float3 relativeStart;

		/// <summary>\copydocref{PathTracer.LinkInfo.relativeEnd}</summary>
		public float3 relativeEnd;

		/// <summary>\copydocref{PathTracer.LinkInfo.relativeStart}. Deprecated: Use relativeStart instead</summary>
		[System.Obsolete("Use relativeStart instead")]
		public float3 firstPosition => relativeStart;

		/// <summary>\copydocref{PathTracer.LinkInfo.relativeEnd}. Deprecated: Use relativeEnd instead</summary>
		[System.Obsolete("Use relativeEnd instead")]
		public float3 secondPosition => relativeEnd;

		/// <summary>\copydocref{PathTracer.LinkInfo.isReverse}</summary>
		public bool isReverse;
	}

	/// <summary>
	/// Holds managed information about an off-mesh link that the agent is currently traversing.
	/// This component is added to the agent when it starts traversing an off-mesh link.
	/// It is removed when the agent has finished traversing the link.
	///
	/// See: <see cref="AgentOffMeshLinkTraversal"/>
	/// </summary>
	public class ManagedAgentOffMeshLinkTraversal : IComponentData, System.ICloneable, ICleanupComponentData {
		/// <summary>Internal context used to pass component data to the coroutine</summary>
		internal AgentOffMeshLinkTraversalContext context;

		/// <summary>Coroutine which is used to traverse the link</summary>
		public System.Collections.IEnumerator coroutine;
		public IOffMeshLinkStateMachine stateMachine;

		public object Clone () {
			return new ManagedAgentOffMeshLinkTraversal {
					   context = null,
					   coroutine = null,
					   stateMachine = this.stateMachine,
			};
		}
	}

	public struct MovementTarget {
		internal bool isReached;
		public bool reached => isReached;
	}

	public class AgentOffMeshLinkTraversalContext {
		internal unsafe AgentOffMeshLinkTraversal* linkInfoPtr;
		internal unsafe MovementControl* movementControlPtr;
		internal unsafe MovementSettings* movementSettingsPtr;
		internal unsafe LocalTransform* transformPtr;
		internal unsafe AgentMovementPlane* movementPlanePtr;
		public Entity entity;
		public ManagedState managedState;
		public OffMeshLinks.OffMeshLinkSource link;
		bool disabledRVO;

		/// <summary>
		/// Delta time since the last link simulation.
		///
		/// During high time scales, the simulation may run multiple substeps per frame.
		///
		/// This is not the same as Time.deltaTime. Inside the link coroutine, you should always use this field instead of Time.deltaTime.
		/// </summary>
		public float deltaTime;

		GameObject gameObjectCache;

		/// <summary>
		/// GameObject associated with the agent.
		///
		/// In most cases, an agent is associated with an agent, but this is not always the case.
		/// For example, if you have created an entity without using the <see cref="FollowerEntity"/> component, this property may return null.
		///
		/// Note: When directly modifying the agent's transform during a link traversal, you should use the <see cref="transform"/> property instead of modifying the GameObject's transform.
		/// </summary>
		public GameObject gameObject {
			get {
				if (gameObjectCache == null) {
					var follower = BatchedEvents.Find<FollowerEntity, Entity>(entity, (follower, entity) => follower.entity == entity);
					if (follower != null) gameObjectCache = follower.gameObject;
				}
				return gameObjectCache;
			}
		}

		public ref LocalTransform transform {
			get {
				unsafe {
					return ref *transformPtr;
				}
			}
		}

		public ref MovementSettings movementSettings {
			get {
				unsafe {
					return ref *movementSettingsPtr;
				}
			}
		}

		public ref MovementControl movementControl {
			get {
				unsafe {
					return ref *movementControlPtr;
				}
			}
		}

		public ref AgentOffMeshLinkTraversal linkInfo {
			get {
				unsafe {
					return ref *linkInfoPtr;
				}
			}
		}

		public ref NativeMovementPlane movementPlane {
			get {
				unsafe {
					return ref movementPlanePtr->value;
				}
			}
		}

		public void DisableLocalAvoidance () {
			if (managedState.enableLocalAvoidance) {
				disabledRVO = true;
				managedState.enableLocalAvoidance = false;
			}
		}

		public void Restore () {
			if (disabledRVO) {
				managedState.enableLocalAvoidance = true;
				disabledRVO = false;
			}
		}

		public void Teleport (float3 position) {
			transform.Position = position;
		}

		/// <summary>
		/// Move towards a point while ignoring the navmesh.
		/// This method should be called repeatedly until the returned <see cref="MovementTarget.reached"/> property is true.
		///
		/// Returns: A <see cref="MovementTarget"/> struct which can be used to check if the target has been reached.
		///
		/// Note: This method completely ignores the navmesh.
		///
		/// TODO: The gravity property is not yet implemented. Gravity is always applied.
		/// </summary>
		/// <param name="position">The position to move towards.</param>
		/// <param name="rotation">The rotation to rotate towards.</param>
		/// <param name="gravity">If true, gravity will be applied to the agent.</param>
		/// <param name="slowdown">If true, the agent will slow down as it approaches the target.</param>
		public MovementTarget MoveTowards (float3 position, quaternion rotation, bool gravity, bool slowdown) {
			var dirInPlane = movementPlane.ToPlane(position - transform.Position);
			var remainingDistance = math.length(dirInPlane);
			var maxSpeed = movementSettings.follower.Speed(slowdown ? remainingDistance : float.PositiveInfinity);
			var speed = movementSettings.follower.Accelerate(movementControl.speed, movementSettings.follower.slowdownTime, deltaTime);
			speed = math.min(speed, maxSpeed);

			var targetRot = movementPlane.ToPlane(rotation);
			var currentRot = movementPlane.ToPlane(transform.Rotation);
			var remainingRot = Mathf.Abs(AstarMath.DeltaAngle(currentRot, targetRot));
			movementControl = new MovementControl {
				targetPoint = position,
				endOfPath = position,
				speed = speed,
				maxSpeed = speed * 1.1f,
				hierarchicalNodeIndex = -1,
				overrideLocalAvoidance = true,
				targetRotation = targetRot,
				targetRotationHint = targetRot,
				targetRotationOffset = 0,
				rotationSpeed = movementSettings.follower.rotationSpeed,
			};

			return new MovementTarget {
					   isReached = remainingDistance <= (slowdown ? 0.01f : speed * (1/30f)) && remainingRot < math.radians(1),
			};
		}
	}
}

// ctx.MoveTowards (position, rotation, rvo = Auto | Disabled | AutoUnstoppable, gravity = auto|disabled) -> { reached() }

// MovementTarget { ... }
// while (!movementTarget.reached) {
// 	ctx.SetMovementTarget(movementTarget);
// 	yield return null;
// }
// yield return ctx.MoveTo(position, rotation)
// ctx.TeleportTo(position, rotation)
#endif
