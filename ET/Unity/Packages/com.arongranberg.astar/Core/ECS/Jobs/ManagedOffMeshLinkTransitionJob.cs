#pragma warning disable 0282 // Allows the 'partial' keyword without warnings
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

namespace Pathfinding.ECS {
	using Pathfinding;

	public partial struct ManagedOffMeshLinkTransitionJob : IJobEntity {
		public EntityCommandBuffer commandBuffer;
		public float deltaTime;

		public void Execute (Entity entity, ManagedState state, ref LocalTransform transform, ref AgentMovementPlane movementPlane, ref MovementControl movementControl, ref MovementSettings movementSettings, ref AgentOffMeshLinkTraversal linkInfo, ManagedAgentOffMeshLinkTraversal managedLinkInfo) {
			unsafe {
				managedLinkInfo.context.linkInfoPtr = (AgentOffMeshLinkTraversal*)UnsafeUtility.AddressOf(ref linkInfo);
				managedLinkInfo.context.movementControlPtr = (MovementControl*)UnsafeUtility.AddressOf(ref movementControl);
				managedLinkInfo.context.movementSettingsPtr = (MovementSettings*)UnsafeUtility.AddressOf(ref movementSettings);
				managedLinkInfo.context.transformPtr = (LocalTransform*)UnsafeUtility.AddressOf(ref transform);
				managedLinkInfo.context.movementPlanePtr = (AgentMovementPlane*)UnsafeUtility.AddressOf(ref movementPlane);
				managedLinkInfo.context.managedState = state;
				managedLinkInfo.context.deltaTime = deltaTime;
				managedLinkInfo.context.entity = entity;
			}
			bool finished;
			bool error = false;
			try {
				finished = !managedLinkInfo.coroutine.MoveNext();
			} catch (System.Exception e) {
				Debug.LogException(e);
				// Teleport the agent to the end of the link as a fallback, if there's an exception
				managedLinkInfo.context.Teleport(managedLinkInfo.context.linkInfo.relativeEnd);
				finished = true;
				error = true;
			}

			if (finished) {
				if (managedLinkInfo.stateMachine != null) {
					if (error) managedLinkInfo.stateMachine.OnAbortTraversingOffMeshLink();
					else managedLinkInfo.stateMachine.OnFinishTraversingOffMeshLink(managedLinkInfo.context);
				}

				managedLinkInfo.context.Restore();
				commandBuffer.RemoveComponent<AgentOffMeshLinkTraversal>(entity);
				commandBuffer.RemoveComponent<ManagedAgentOffMeshLinkTraversal>(entity);
				// Pop the part leading up to the link, and the link itself
				state.pathTracer.PopParts(2, state.pathfindingSettings.traversalProvider, state.activePath);
			}
		}
	}

	public partial struct ManagedOffMeshLinkTransitionCleanupJob : IJobEntity {
		public void Execute (ManagedAgentOffMeshLinkTraversal managedLinkInfo) {
			managedLinkInfo.stateMachine.OnAbortTraversingOffMeshLink();
		}
	}
}
#endif
