#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;

	/// <summary>Copies <see cref="MovementControl"/> to <see cref="ResolvedMovement"/> when no local avoidance is used</summary>
	[BurstCompile]
	[UpdateAfter(typeof(FollowerControlSystem))]
	[UpdateAfter(typeof(RVOSystem))] // Has to execute after RVOSystem in case that system detects that some agents should not be simulated using the RVO system anymore.
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[RequireMatchingQueriesForUpdate]
	public partial struct FallbackResolveMovementSystem : ISystem {
		EntityQuery entityQuery;

		public void OnCreate (ref SystemState state) {
			entityQuery = state.GetEntityQuery(new EntityQueryDesc {
				All = new ComponentType[] {
					ComponentType.ReadWrite<ResolvedMovement>(),
					ComponentType.ReadOnly<MovementControl>(),
					ComponentType.ReadOnly<SimulateMovement>()
				},
				Options = EntityQueryOptions.FilterWriteGroup
			});
		}

		public void OnDestroy (ref SystemState state) { }

		public void OnUpdate (ref SystemState systemState) {
			new CopyJob {}.Schedule(entityQuery);
		}

		[BurstCompile]
		public partial struct CopyJob : IJobEntity {
			public void Execute (in MovementControl control, ref ResolvedMovement resolved) {
				resolved.targetPoint = control.targetPoint;
				resolved.speed = control.speed;
				resolved.turningRadiusMultiplier = 1.0f;
				resolved.targetRotation = control.targetRotation;
				resolved.targetRotationHint = control.targetRotationHint;
				resolved.targetRotationOffset = control.targetRotationOffset;
				resolved.rotationSpeed = control.rotationSpeed;
			}
		}
	}
}
#endif
