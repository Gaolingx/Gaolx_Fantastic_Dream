#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using UnityEngine;

namespace Pathfinding.ECS {
	using Pathfinding;

	[UpdateBefore(typeof(FollowerControlSystem))]
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[RequireMatchingQueriesForUpdateAttribute]
	public partial struct SyncDestinationTransformSystem : ISystem {
		public void OnCreate (ref SystemState state) {}
		public void OnDestroy (ref SystemState state) {}

		public void OnUpdate (ref SystemState systemState) {
			foreach (var(point, destinationSetter) in SystemAPI.Query<RefRW<DestinationPoint>, AIDestinationSetter>()) {
				if (destinationSetter.target != null) {
					point.ValueRW = new DestinationPoint {
						destination = destinationSetter.target.position,
						facingDirection = destinationSetter.useRotation ? destinationSetter.target.forward : Vector3.zero
					};
				}
			}
		}
	}
}
#endif
