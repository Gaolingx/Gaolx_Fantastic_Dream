#pragma warning disable CS0282 // "There is no defined ordering between fields in multiple declarations of partial struct"
#if MODULE_ENTITIES
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Profiling;
using Unity.Entities;
using Unity.Transforms;

namespace Pathfinding {
	using Pathfinding.Drawing;
	using Pathfinding.Util;
	using Palette = Pathfinding.Drawing.Palette.Colorbrewer.Set1;
	using System;
	using Pathfinding.PID;
	using Pathfinding.ECS.RVO;
	using Pathfinding.ECS;

	/// <summary>
	/// Movement script that uses ECS.
	///
	/// Warning: This script is still in beta and may change in the future. It aims to be much more robust than AIPath/RichAI, but there may still be rough edges.
	///
	/// This script is a replacement for the <see cref="AIPath"/> and <see cref="RichAI"/> scripts.
	///
	/// This script is a movement script. It takes care of moving an agent along a path, updating the path, and so on.
	///
	/// The intended way to use this script is to use these two components:
	/// - <see cref="FollowerEntity"/>
	/// - <see cref="AIDestinationSetter"/> (optional, you can instead set the <see cref="destination"/> property manually)
	///
	/// Of note is that this component shouldn't be used with a <see cref="Seeker"/> component.
	/// It instead has its own settings for pathfinding, which are stored in the <see cref="pathfindingSettings"/> field.
	///
	/// <b>Features</b>
	///
	/// - Uses Unity's ECS (Entity Component System) to move the agent. This means it is highly-performant and is able to utilize multiple threads.
	/// - Supports local avoidance (see local-avoidance) (view in online documentation for working links).
	/// - Supports movement in both 2D and 3D games.
	/// - Supports movement on spherical on non-planar worlds (see spherical) (view in online documentation for working links).
	/// - Supports movement on grid graphs as well as navmesh/recast graphs.
	/// - Does <b>not</b> support movement on point graphs at the moment. This may be added in a future update.
	/// - Supports time-scales greater than 1. The agent will automatically run multiple simulation steps per frame if the time-scale is greater than 1, to ensure stability.
	/// - Supports off-mesh links. Subscribe to the <see cref="onTraverseOffMeshLink"/> event to handle this.
	/// - Knows which node it is traversing at all times (see <see cref="currentNode)"/>.
	/// - Automatically stops when trying to reach a crowded destination when using local avoidance.
	/// - Clamps the agent to the navmesh at all times.
	/// - Follows paths very smoothly.
	/// - Can keep a desired distance to walls.
	/// - Can approach its destination with a desired facing direction.
	///
	/// <b>%ECS</b>
	///
	/// This script uses Unity's ECS (Entity Component System) to move the agent. This means it is highly-performant and is able to utilize multiple threads.
	/// Internally, an entity is created for the agent with the following components:
	///
	/// - LocalTransform
	/// - <see cref="MovementState"/>
	/// - <see cref="MovementSettings"/>
	/// - <see cref="MovementControl"/>
	/// - <see cref="ManagedState"/>
	/// - <see cref="SearchState"/>
	/// - <see cref="MovementStatistics"/>
	/// - <see cref="AgentCylinderShape"/>
	/// - <see cref="ResolvedMovement"/>
	/// - <see cref="GravityState"/>
	/// - <see cref="DestinationPoint"/>
	/// - <see cref="AgentMovementPlane"/>
	/// - <see cref="SimulateMovement"/> - tag component (if <see cref="canMove"/> is enabled)
	/// - <see cref="SimulateMovementRepair"/> - tag component
	/// - <see cref="SimulateMovementControl"/> - tag component
	/// - <see cref="SimulateMovementFinalize"/> - tag component
	/// - <see cref="SyncPositionWithTransform"/> - tag component (if <see cref="updatePosition"/> is enabled)
	/// - <see cref="SyncRotationWithTransform"/> - tag component (if <see cref="updateRotation"/> is enabled)
	/// - <see cref="OrientationYAxisForward"/> - tag component (if <see cref="orientation"/> is <see cref="OrientationMode"/>.YAxisForward)
	/// - <see cref="ECS.RVO.RVOAgent"/> (if local avoidance is enabled)
	///
	/// Then this script barely does anything by itself. It is a thin wrapper around the ECS components.
	/// Instead, actual movement calculations are carried out by the following systems:
	///
	/// - <see cref="SyncTransformsToEntitiesSystem"/> - Updates the entity's transform from the GameObject.
	/// - <see cref="MovementPlaneFromGraphSystem"/> - Updates the agent's movement plane.
	/// - <see cref="SyncDestinationTransformSystem"/> - Updates the destination point if the destination transform moves.
	/// - <see cref="FollowerControlSystem"/> - Calculates how the agent wants to move.
	/// - <see cref="RVOSystem"/> - Local avoidance calculations.
	/// - <see cref="FallbackResolveMovementSystem"/> - NOOP system for if local avoidance is disabled.
	/// - <see cref="AIMoveSystem"/> - Performs the actual movement.
	///
	/// In fact, as long as you create the appropriate ECS components, you do not even need this script. You can use the systems directly.
	///
	/// This is <b>not</b> a baked component. That is, this script will continue to work even in standalone games. It is designed to be easily used
	/// without having to care too much about the underlying ECS implementation.
	///
	/// <b>Differences compared to AIPath and RichAI</b>
	///
	/// This movement script has been written to remedy several inconsistency issues with other movement scrips, to provide very smooth movement,
	/// and "just work" for most games.
	///
	/// For example, it goes to great lengths to ensure
	/// that the <see cref="reachedDestination"/> and <see cref="reachedEndOfPath"/> properties are as accurate as possible at all times, even before it has had time to recalculate its path to account for a new <see cref="destination"/>.
	/// It does this by locally repairing the path (if possible) immediately when the destination changes instead of waiting for a path recalculation.
	/// This also has a bonus effect that the agent can often work just fine with moving targets, even if it almost never recalculates its path (though the repaired path may not always be optimal),
	/// and it leads to very responsive movement.
	///
	/// In contrast to other movement scripts, this movement script does not use path modifiers at all.
	/// Instead, this script contains its own internal <see cref="FunnelModifier"/> which it uses to simplify the path before it follows it.
	/// In also doesn't use a separate <see cref="RVOController"/> component for local avoidance, but instead it stores local avoidance settings in <see cref="rvoSettings"/>.
	///
	/// <b>Best practices for good performance</b>
	///
	/// Using ECS components has some downsides. Accessing properties on this script is significantly slower compared to accessing properties on other movement scripts.
	/// This is because on each property access, the script has to make sure no jobs are running concurrently, which is a relatively expensive operation.
	/// Slow is a relative term, though. This only starts to matter if you have lots of agents, maybe a hundred or so. So don't be scared of using it.
	///
	/// But if you have a lot of agents, it is recommended to not access properties on this script more often than required. Avoid setting fields to the same value over and over again every frame, for example.
	/// If you have a moving target, try to use the <see cref="AIDestinationSetter"/> component instead of setting the <see cref="destination"/> property manually, as that is faster than setting the <see cref="destination"/> property every frame.
	///
	/// You can instead write custom ECS systems to access the properties on the ECS components directly. This is much faster.
	/// For example, if you want to make the agent follow a particular entity, you could create a new DestinationEntity component which just holds an entity reference,
	/// and then create a system that every frame copies that entity's position to the <see cref="DestinationPoint.destination"/> field (a component that this entity will always have).
	///
	/// This script has some optional parts. Local avoidance, for example. Local avoidance is used to make sure that agents do not overlap each other.
	/// However, if you do not need it, you can disable it to improve performance.
	/// </summary>
	[AddComponentMenu("Pathfinding/AI/Follower Entity (2D,3D)")]
	[UniqueComponent(tag = "ai")]
	[UniqueComponent(tag = "rvo")]
	public sealed partial class FollowerEntity : VersionedMonoBehaviour, IAstarAI, ISerializationCallbackReceiver {
		[SerializeField]
		AgentCylinderShape shape = new AgentCylinderShape {
			height = 2,
			radius = 0.5f,
		};
		[SerializeField]
		MovementSettings movement = new MovementSettings {
			follower = new PIDMovement {
				rotationSpeed = 600,
				speed = 5,
				maxRotationSpeed = 720,
				maxOnSpotRotationSpeed = 720,
				slowdownTime = 0.5f,
				desiredWallDistance = 0.5f,
				allowRotatingOnSpot = true,
				leadInRadiusWhenApproachingDestination = 1f,
			},
			movementPlaneSource = MovementPlaneSource.Graph,
			stopDistance = 0.2f,
			rotationSmoothing = 0f,
			groundMask = -1,
			isStopped = false,
		};

		[SerializeField]
		ManagedState managedState = new ManagedState {
			enableLocalAvoidance = false,
			pathfindingSettings = PathRequestSettings.Default,
		};

		/// <summary>
		/// Determines which direction the agent moves in.
		///
		/// See: <see cref="orientation"/>
		/// </summary>
		[SerializeField]
		OrientationMode orientationBacking;

		/// <summary>Cached transform component</summary>
		Transform tr;

		/// <summary>
		/// Entity which this movement script represents.
		///
		/// An entity will be created when this script is enabled, and destroyed when this script is disabled.
		///
		/// Check the class documentation to see which components it usually has, and what systems typically affect it.
		/// </summary>
		public Entity entity { get; private set; }

		static EntityAccess<DestinationPoint> destinationPointAccessRW = new EntityAccess<DestinationPoint>(false);
		static EntityAccess<DestinationPoint> destinationPointAccessRO = new EntityAccess<DestinationPoint>(true);
		static EntityAccess<AgentMovementPlane> movementPlaneAccessRW = new EntityAccess<AgentMovementPlane>(false);
		static EntityAccess<AgentMovementPlane> movementPlaneAccessRO = new EntityAccess<AgentMovementPlane>(false);
		static EntityAccess<MovementState> movementStateAccessRW = new EntityAccess<MovementState>(false);
		static EntityAccess<MovementState> movementStateAccessRO = new EntityAccess<MovementState>(true);
		static EntityAccess<MovementStatistics> movementOutputAccessRW = new EntityAccess<MovementStatistics>(false);
		static EntityAccess<ResolvedMovement> resolvedMovementAccessRO = new EntityAccess<ResolvedMovement>(true);
		static EntityAccess<ResolvedMovement> resolvedMovementAccessRW = new EntityAccess<ResolvedMovement>(false);
		static EntityAccess<MovementControl> movementControlAccessRO = new EntityAccess<MovementControl>(true);
		static EntityAccess<MovementControl> movementControlAccessRW = new EntityAccess<MovementControl>(false);
		static ManagedEntityAccess<ManagedState> managedStateAccessRO = new ManagedEntityAccess<ManagedState>(true);
		static ManagedEntityAccess<ManagedState> managedStateAccessRW = new ManagedEntityAccess<ManagedState>(false);
		static EntityAccess<LocalTransform> localTransformAccessRO = new EntityAccess<LocalTransform>(true);
		static EntityAccess<LocalTransform> localTransformAccessRW = new EntityAccess<LocalTransform>(false);
		static EntityAccess<AgentCylinderShape> agentCylinderShapeAccessRO = new EntityAccess<AgentCylinderShape>(true);
		static EntityAccess<AgentCylinderShape> agentCylinderShapeAccessRW = new EntityAccess<AgentCylinderShape>(false);
		static EntityAccess<MovementSettings> movementSettingsAccessRO = new EntityAccess<MovementSettings>(true);
		static EntityAccess<MovementSettings> movementSettingsAccessRW = new EntityAccess<MovementSettings>(false);
		static EntityAccess<AgentOffMeshLinkTraversal> agentOffMeshLinkTraversalRO = new EntityAccess<AgentOffMeshLinkTraversal>(true);
		static EntityStorageCache entityStorageCache;

		static EntityArchetype archetype;
		static World achetypeWorld;

		void OnEnable () {
			scratchReferenceCount++;

			var world = World.DefaultGameObjectInjectionWorld;
			if (!archetype.Valid || achetypeWorld != world) {
				if (world == null) throw new Exception("World.DefaultGameObjectInjectionWorld is null. Has the world been destroyed?");
				achetypeWorld = world;
				archetype = world.EntityManager.CreateArchetype(
					typeof(LocalTransform),
					typeof(MovementState),
					typeof(MovementSettings),
					typeof(MovementControl),
					typeof(ManagedState),
					typeof(SearchState),
					typeof(MovementStatistics),
					typeof(AgentCylinderShape),
					typeof(ResolvedMovement),
					typeof(DestinationPoint),
					typeof(AgentMovementPlane),
					typeof(SimulateMovement),
					typeof(SimulateMovementRepair),
					typeof(SimulateMovementControl),
					typeof(SimulateMovementFinalize),
					typeof(SyncPositionWithTransform),
					typeof(SyncRotationWithTransform)
					);
			}

			FindComponents();

			entity = world.EntityManager.CreateEntity(archetype);
			var pos = tr.position;
			// This GameObject may be in a hierarchy, but the entity will not be. So we copy the world orientation to the entity's local transform component
			world.EntityManager.SetComponentData(entity, LocalTransform.FromPositionRotationScale(pos, tr.rotation, tr.localScale.x));
			world.EntityManager.SetComponentData(entity, new MovementState {
				followerState = new PIDMovement.PersistentState {
					maxDesiredWallDistance = 0,
				},
				// Set reasonable defaults before the simulation starts
				nextCorner = pos,
				endOfPath = pos,
				closestOnNavmesh = pos,
				hierarchicalNodeIndex = -1,
				remainingDistanceToEndOfPart = float.PositiveInfinity,
				reachedDestination = false,
				reachedEndOfPath = false,
				reachedDestinationAndOrientation = false,
				reachedEndOfPathAndOrientation = false,
				traversingLastPart = false,
				pathTracerVersion = 0,
			});
#if UNITY_EDITOR
			world.EntityManager.SetName(entity, "Follower Entity");
#endif
			world.EntityManager.SetComponentData(entity, new AgentMovementPlane {
				// TODO: Handle 2D movement when Y is forward
				// TODO! Also add unit tests
				value = new NativeMovementPlane(tr.rotation),
			});
			world.EntityManager.SetComponentData(entity, new DestinationPoint {
				destination = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity),
			});
			world.EntityManager.SetComponentData(entity, movement);
			if (!managedState.pathTracer.isCreated) {
				managedState.pathTracer = new PathTracer(Allocator.Persistent);
			}
			world.EntityManager.SetComponentData(entity, managedState);
			world.EntityManager.SetComponentData(entity, new MovementStatistics {
				estimatedVelocity = float3.zero,
				lastPosition = pos,
			});
			world.EntityManager.SetComponentData(entity, shape);
			if (managedState.enableGravity) {
				world.EntityManager.AddComponent<GravityState>(entity);
			}
			if (orientation == OrientationMode.YAxisForward) {
				world.EntityManager.AddComponent<OrientationYAxisForward>(entity);
			}

			// Register with the BatchedEvents system
			// This is used not for the events, but because it keeps track of a TransformAccessArray
			// of all components. This is then used by the SyncEntitiesToTransformsJob.
			BatchedEvents.Add(this, BatchedEvents.Event.None, (components, ev) => {});

			var runtimeBakers = GetComponents<IRuntimeBaker>();
			for (int i = 0; i < runtimeBakers.Length; i++) if (((MonoBehaviour)runtimeBakers[i]).enabled) runtimeBakers[i].OnCreatedEntity(world, entity);
		}

		internal void RegisterRuntimeBaker (IRuntimeBaker baker) {
			if (entityExists) baker.OnCreatedEntity(World.DefaultGameObjectInjectionWorld, entity);
		}

		void Start () {
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			managedStateAccessRW.Update(entityManager);
			movementPlaneAccessRO.Update(entityManager);
			if (!managedState.pathTracer.hasPath && AstarPath.active != null) {
				var nearest = AstarPath.active.GetNearest(position, NNConstraint.Walkable);
				if (nearest.node != null) {
					var storage = entityManager.GetStorageInfo(entity);
					var movementPlane = movementPlaneAccessRO[storage];
					managedState.pathTracer.SetFromSingleNode(nearest.node, nearest.position, movementPlane.value);
					managedState.pathTracer.UpdateEnd(new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity), PathTracer.RepairQuality.High, movementPlane.value, null, null);
				}
			}
		}

		/// <summary>
		/// Called when the component is disabled or about to be destroyed.
		///
		/// This is also called by Unity when an undo/redo event is performed. This means that
		/// when an undo event happens the entity will get destroyed and then re-created.
		/// </summary>
		void OnDisable () {
			scratchReferenceCount--;
			if (scratchReferenceCount == 0) {
				if (indicesScratch.IsCreated) indicesScratch.Dispose();
				if (nextCornersScratch.IsCreated) nextCornersScratch.Dispose();
			}

			BatchedEvents.Remove(this);
			CancelCurrentPathRequest();
			if (World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.IsCreated) World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(entity);
			managedState.pathTracer.Dispose();
			managedState.autoRepath.Reset();
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		public float radius {
			get => shape.radius;
			set {
				this.shape.radius = value;
				if (entityStorageCache.GetComponentData(entity, ref agentCylinderShapeAccessRW, out var shape)) {
					shape.value.radius = value;
				}
			}
		}

		/// <summary>
		/// Height of the agent in world units.
		/// This is visualized in the scene view as a yellow cylinder around the character.
		///
		/// This value is used for various heuristics, and for visualization purposes.
		/// For example, the destination is only considered reached if the destination is not above the agent's head, and it's not more than half the agent's height below its feet.
		///
		/// If local lavoidance is enabled, this is also used to filter out collisions with agents and obstacles that are too far above or below the agent.
		/// </summary>
		public float height {
			get => shape.height;
			set {
				this.shape.height = value;
				if (entityStorageCache.GetComponentData(entity, ref agentCylinderShapeAccessRW, out var shape)) {
					shape.value.height = value;
				}
			}
		}

		/// <summary>Pathfinding settings</summary>
		public ref PathRequestSettings pathfindingSettings {
			get {
				// Complete any job dependencies
				// Need RW because this getter has a ref return.
				entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var _);
				return ref managedState.pathfindingSettings;
			}
		}

		/// <summary>Local avoidance settings</summary>
		public ref RVOAgent rvoSettings {
			get {
				// Complete any job dependencies
				// Need RW because this getter has a ref return.
				entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var _);
				return ref managedState.rvoSettings;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::position</summary>
		public Vector3 position {
			get {
				// Make sure we are not waiting for a job to update the world position
				if (entityStorageCache.GetComponentData(entity, ref localTransformAccessRO, out var localTransform)) {
					return localTransform.value.Position;
				} else {
					return transform.position;
				}
			}
			set {
				if (entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) {
					// Update path and other properties using our new position
					if (entityManager.HasComponent<SyncPositionWithTransform>(entity)) {
						transform.position = value;
					}
					movementStateAccessRW.Update(entityManager);
					managedStateAccessRW.Update(entityManager);
					agentCylinderShapeAccessRO.Update(entityManager);
					movementSettingsAccessRO.Update(entityManager);
					destinationPointAccessRO.Update(entityManager);
					movementPlaneAccessRO.Update(entityManager);
					localTransformAccessRW.Update(entityManager);

					ref var localTransform = ref localTransformAccessRW[storage];
					localTransform.Position = value;
					ref var movementState = ref movementStateAccessRW[storage];
					movementState.positionOffset = float3.zero;
					if (managedState.pathTracer.hasPath) {
						Profiler.BeginSample("RepairStart");
						ref var movementPlane = ref movementPlaneAccessRO[storage];
						var oldVersion = managedState.pathTracer.version;
						managedState.pathTracer.UpdateStart(value, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
						Profiler.EndSample();
						if (managedState.pathTracer.version != oldVersion) {
							Profiler.BeginSample("EstimateNative");
							ref var shape = ref agentCylinderShapeAccessRO[storage];
							ref var movementSettings = ref movementSettingsAccessRO[storage];
							ref var destinationPoint = ref destinationPointAccessRO[storage];
							if (!nextCornersScratch.IsCreated) nextCornersScratch = new NativeList<float3>(4, Allocator.Persistent);
							RepairPathJob.Execute(
								ref localTransform,
								ref movementState,
								ref shape,
								ref movementPlane,
								ref destinationPoint,
								managedState,
								in movementSettings,
								nextCornersScratch,
								ref indicesScratch,
								Allocator.Persistent,
								false
								);
							Profiler.EndSample();
						}
					}
				} else {
					transform.position = value;
				}
			}
		}

		/// <summary>
		/// True if the agent is currently traversing an off-mesh link.
		///
		/// See: offmeshlinks (view in online documentation for working links)
		/// See: <see cref="onTraverseOffMeshLink"/>
		/// See: <see cref="offMeshLink"/>
		/// </summary>
		public bool isTraversingOffMeshLink {
			get {
				if (!entityExists) return false;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				return entityManager.HasComponent<AgentOffMeshLinkTraversal>(entity);
			}
		}

		/// <summary>
		/// The off-mesh link that the agent is currently traversing.
		///
		/// This will be a default <see cref="PathTracer.LinkInfo"/> if the agent is not traversing an off-mesh link (the <see cref="PathTracer.LinkInfo.link"/> field will be null).
		///
		/// Note: If the off-mesh link is destroyed while the agent is traversing it, this property will still return the link.
		/// But be careful about accessing properties like <see cref="OffMeshLinkSource.gameObject"/>, as that may refer to a destroyed gameObject.
		///
		/// See: offmeshlinks (view in online documentation for working links)
		/// See: <see cref="onTraverseOffMeshLink"/>
		/// See: <see cref="isTraversingOffMeshLink"/>
		/// </summary>
		public PathTracer.LinkInfo offMeshLink {
			get {
				if (entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage) && entityManager.HasComponent<ManagedAgentOffMeshLinkTraversal>(entity)) {
					agentOffMeshLinkTraversalRO.Update(entityManager);
					var linkTraversal = agentOffMeshLinkTraversalRO[storage];
					var linkTraversalManaged = entityManager.GetComponentData<ManagedAgentOffMeshLinkTraversal>(entity);
					return new PathTracer.LinkInfo {
							   relativeStart = linkTraversal.relativeStart,
							   relativeEnd = linkTraversal.relativeEnd,
							   isReverse = linkTraversal.isReverse,
							   link = linkTraversalManaged.context.link,
					};
				} else {
					return default;
				}
			}
		}

		/// <summary>
		/// Callback to be called when an agent starts traversing an off-mesh link.
		///
		/// The handler will be called when the agent starts traversing an off-mesh link.
		/// It allows you to to control the agent for the full duration of the link traversal.
		///
		/// Use the passed context struct to get information about the link and to control the agent.
		///
		/// <code>
		/// using UnityEngine;
		/// using Pathfinding;
		/// using System.Collections;
		/// using Pathfinding.ECS;
		///
		/// namespace Pathfinding.Examples {
		///     public class FollowerJumpLink : MonoBehaviour, IOffMeshLinkHandler, IOffMeshLinkStateMachine {
		///         // Register this class as the handler for off-mesh links when the component is enabled
		///         void OnEnable() => GetComponent<NodeLink2>().onTraverseOffMeshLink = this;
		///         void OnDisable() => GetComponent<NodeLink2>().onTraverseOffMeshLink = null;
		///
		///         IOffMeshLinkStateMachine IOffMeshLinkHandler.GetOffMeshLinkStateMachine(AgentOffMeshLinkTraversalContext context) => this;
		///
		///         void IOffMeshLinkStateMachine.OnFinishTraversingOffMeshLink (AgentOffMeshLinkTraversalContext context) {
		///             Debug.Log("An agent finished traversing an off-mesh link");
		///         }
		///
		///         void IOffMeshLinkStateMachine.OnAbortTraversingOffMeshLink () {
		///             Debug.Log("An agent aborted traversing an off-mesh link");
		///         }
		///
		///         IEnumerable IOffMeshLinkStateMachine.OnTraverseOffMeshLink (AgentOffMeshLinkTraversalContext ctx) {
		///             var start = (Vector3)ctx.linkInfo.relativeStart;
		///             var end = (Vector3)ctx.linkInfo.relativeEnd;
		///             var dir = end - start;
		///
		///             // Disable local avoidance while traversing the off-mesh link.
		///             // If it was enabled, it will be automatically re-enabled when the agent finishes traversing the link.
		///             ctx.DisableLocalAvoidance();
		///
		///             // Move and rotate the agent to face the other side of the link.
		///             // When reaching the off-mesh link, the agent may be facing the wrong direction.
		///             while (!ctx.MoveTowards(
		///                 position: start,
		///                 rotation: Quaternion.LookRotation(dir, ctx.movementPlane.up),
		///                 gravity: true,
		///                 slowdown: true).reached) {
		///                 yield return null;
		///             }
		///
		///             var bezierP0 = start;
		///             var bezierP1 = start + Vector3.up*5;
		///             var bezierP2 = end + Vector3.up*5;
		///             var bezierP3 = end;
		///             var jumpDuration = 1.0f;
		///
		///             // Animate the AI to jump from the start to the end of the link
		///             for (float t = 0; t < jumpDuration; t += ctx.deltaTime) {
		///                 ctx.transform.Position = AstarSplines.CubicBezier(bezierP0, bezierP1, bezierP2, bezierP3, Mathf.SmoothStep(0, 1, t / jumpDuration));
		///                 yield return null;
		///             }
		///         }
		///     }
		/// }
		/// </code>
		///
		/// Warning: Off-mesh links can be destroyed or disabled at any moment. The built-in code will attempt to make the agent continue following the link even if it is destroyed,
		/// but if you write your own traversal code, you should be aware of this.
		///
		/// You can alternatively set the corresponding property property on the off-mesh link (<see cref="NodeLink2.onTraverseOffMeshLink"/>) to specify a callback for a specific off-mesh link.
		///
		/// Note: The agent's off-mesh link handler takes precedence over the link's off-mesh link handler, if both are set.
		///
		/// See: offmeshlinks (view in online documentation for working links) for more details and example code
		/// See: <see cref="isTraversingOffMeshLink"/>
		/// </summary>
		public IOffMeshLinkHandler onTraverseOffMeshLink {
			get => managedState.onTraverseOffMeshLink;
			set {
				// Complete any job dependencies
				entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var _);
				managedState.onTraverseOffMeshLink = value;
			}
		}

		/// <summary>
		/// Node which the agent is currently traversing.
		///
		/// You can, for example, use this to make the agent use a different animation when traversing nodes with a specific tag.
		///
		/// Note: Will be null if the agent does not have a path, or if the node under the agent has just been destroyed by a graph update.
		///
		/// When traversing an off-mesh link, this will return the final non-link node in the path before the agent started traversing the link.
		/// </summary>
		public GraphNode currentNode {
			get {
				if (!entityExists) return null;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				// Complete any job dependencies
				managedStateAccessRO.Update(entityManager);
				var node = managedState.pathTracer.startNode;
				if (node == null || node.Destroyed) return null;
				return node;
			}
		}

		/// <summary>
		/// Rotation of the agent.
		/// In world space.
		///
		/// The entity internally always treats the Z axis as forward, but this property respects the <see cref="orientation"/> field. So it
		/// will return either a rotation with the Y axis as forward, or Z axis as forward, depending on the <see cref="orientation"/> field.
		///
		/// This will return the agent's rotation even if <see cref="updateRotation"/> is false.
		///
		/// See: <see cref="position"/>
		/// </summary>
		public Quaternion rotation {
			get {
				if (entityStorageCache.GetComponentData(entity, ref localTransformAccessRO, out var localTransform)) {
					var r = localTransform.value.Rotation;
					if (orientation == OrientationMode.YAxisForward) r = math.mul(r, SyncTransformsToEntitiesSystem.ZAxisForwardToYAxisForward);
					return r;
				} else {
					return transform.rotation;
				}
			}
			set {
				if (entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) {
					// Update path and other properties using our new position
					if (entityManager.HasComponent<SyncRotationWithTransform>(entity)) {
						transform.rotation = value;
					}

					if (orientation == OrientationMode.YAxisForward) value = math.mul(value, SyncTransformsToEntitiesSystem.YAxisForwardToZAxisForward);
					localTransformAccessRW.Update(entityManager);
					localTransformAccessRW[storage].Rotation = value;
				} else {
					transform.rotation = value;
				}
			}
		}

		/// <summary>
		/// Determines which layers the agent will stand on.
		///
		/// The agent will use a raycast each frame to check if it should stop falling.
		///
		/// This layer mask should ideally not contain the agent's own layer, if the agent has a collider,
		/// as this may cause it to try to stand on top of itself.
		/// </summary>
		public LayerMask groundMask {
			get => movement.groundMask;
			set {
				movement.groundMask = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value.groundMask = value;
				}
			}
		}

		/// <summary>
		/// Enables or disables debug drawing for this agent.
		///
		/// This is a bitmask with multiple flags so that you can choose exactly what you want to debug.
		///
		/// See: <see cref="PIDMovement.DebugFlags"/>
		/// See: bitmasks (view in online documentation for working links)
		/// </summary>
		public PIDMovement.DebugFlags debugFlags {
			get => movement.debugFlags;
			set {
				movement.debugFlags = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value.debugFlags = value;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::maxSpeed</summary>
		public float maxSpeed {
			get => movement.follower.speed;
			set {
				movement.follower.speed = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value.follower.speed = value;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::velocity</summary>
		public Vector3 velocity => entityExists ? (Vector3)World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<MovementStatistics>(entity).estimatedVelocity : Vector3.zero;

		/// <summary>\copydoc Pathfinding::IAstarAI::desiredVelocity</summary>
		public Vector3 desiredVelocity {
			get {
				if (entityStorageCache.GetComponentData(entity, ref resolvedMovementAccessRO, out var resolvedMovement)) {
					var dt = Mathf.Max(Time.deltaTime, 0.0001f);
					return Vector3.ClampMagnitude((Vector3)resolvedMovement.value.targetPoint - position, dt * resolvedMovement.value.speed) / dt;
				} else {
					return Vector3.zero;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::desiredVelocityWithoutLocalAvoidance</summary>
		public Vector3 desiredVelocityWithoutLocalAvoidance {
			get {
				if (entityStorageCache.GetComponentData(entity, ref movementControlAccessRO, out var movementControl)) {
					var dt = Mathf.Max(Time.deltaTime, 0.0001f);
					return Vector3.ClampMagnitude((Vector3)movementControl.value.targetPoint - position, dt * movementControl.value.speed) / dt;
				} else {
					return Vector3.zero;
				}
			}
			set => throw new NotImplementedException("The FollowerEntity does not support setting this property. If you want to override the movement, you'll need to write a custom entity component system.");
		}

		/// <summary>
		/// Approximate remaining distance along the current path to the end of the path.
		///
		/// The agent does not know the true distance at all times, so this is an approximation.
		/// It tends to be a bit lower than the true distance.
		///
		/// Note: This is the distance to the end of the path, which may or may not be the same as the destination.
		/// If the character cannot reach the destination it will try to move as close as possible to it.
		///
		/// This value will update immediately if the <see cref="destination"/> property is changed, or if the agent is moved using the <see cref="position"/> property or the <see cref="Teleport"/> method.
		///
		/// If the agent has no path, or if the current path is stale (e.g. if the graph has been updated close to the agent, and it hasn't had time to recalculate its path), this will return positive infinity.
		///
		/// See: <see cref="reachedDestination"/>
		/// See: <see cref="reachedEndOfPath"/>
		/// See: <see cref="pathPending"/>
		/// </summary>
		public float remainingDistance {
			get {
				if (!entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) return float.PositiveInfinity;

				movementStateAccessRO.Update(entityManager);
				managedStateAccessRO.Update(entityManager);
				// TODO: Should this perhaps only check if the start/end points are stale, and ignore the case when the graph is updated and some nodes are destroyed?
				if (managedState.pathTracer.hasPath && !managedState.pathTracer.isStale) {
					ref var movementState = ref movementStateAccessRO[storage];
					return movementState.remainingDistanceToEndOfPart + Vector3.Distance(managedState.pathTracer.endPointOfFirstPart, managedState.pathTracer.endPoint);
				} else {
					return float.PositiveInfinity;
				}
			}
		}

		/// <summary>\copydocref{MovementSettings.stopDistance}</summary>
		public float stopDistance {
			get => movement.stopDistance;
			set {
				if (movement.stopDistance != value) {
					movement.stopDistance = value;
					if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
						movementSettings.value.stopDistance = value;
					}
				}
			}
		}

		/// <summary>\copydocref{MovementSettings.rotationSmoothing}</summary>
		public float rotationSmoothing {
			get => movement.rotationSmoothing;
			set {
				if (movement.rotationSmoothing != value) {
					movement.rotationSmoothing = value;
					if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
						movementSettings.value.rotationSmoothing = value;
					}
				}
			}
		}

		/// <summary>
		/// True if the ai has reached the <see cref="destination"/>.
		///
		/// The agent considers the destination reached when it is within <see cref="stopDistance"/> world units from the <see cref="destination"/>.
		/// Additionally, the destination must not be above the agent's head, and it must not be more than half the agent's height below its feet.
		///
		/// If a facing direction was specified when setting the destination, this will only return true once the agent is approximately facing the correct orientation.
		///
		/// This value will be updated immediately when the <see cref="destination"/> is changed.
		///
		/// <code>
		/// IEnumerator Start () {
		///     ai.destination = somePoint;
		///     // Start to search for a path to the destination immediately
		///     ai.SearchPath();
		///     // Wait until the agent has reached the destination
		///     while (!ai.reachedDestination) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		///
		/// Note: The agent may not be able to reach the destination. In that case this property may never become true. Sometimes <see cref="reachedEndOfPath"/> is more appropriate.
		///
		/// See: <see cref="stopDistance"/>
		/// See: <see cref="remainingDistance"/>
		/// See: <see cref="reachedEndOfPath"/>
		/// </summary>
		public bool reachedDestination => entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var movementState) ? movementState.value.reachedDestinationAndOrientation : false;

		/// <summary>
		/// True if the agent has reached the end of the current path.
		///
		/// The agent considers the end of the path reached when it is within <see cref="stopDistance"/> world units from the end of the path.
		/// Additionally, the end of the path must not be above the agent's head, and it must not be more than half the agent's height below its feet.
		///
		/// If a facing direction was specified when setting the destination, this will only return true once the agent is approximately facing the correct orientation.
		///
		/// This value will be updated immediately when the <see cref="destination"/> is changed.
		///
		/// Note: Reaching the end of the path does not imply that it has reached its desired destination, as the destination may not even be possible to reach.
		/// Sometimes <see cref="reachedDestination"/> is more appropriate.
		///
		/// See: <see cref="remainingDistance"/>
		/// See: <see cref="reachedDestination"/>
		/// </summary>
		public bool reachedEndOfPath => entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var movementState) ? movementState.value.reachedEndOfPathAndOrientation : false;

		/// <summary>
		/// End point of path the agent is currently following.
		/// If the agent has no path (or if it's not calculated yet), this will return the <see cref="destination"/> instead.
		/// If the agent has no destination it will return the agent's current position.
		///
		/// The end of the path is usually identical or very close to the <see cref="destination"/>, but it may differ
		/// if the path for example was blocked by a wall, so that the agent couldn't get any closer.
		///
		/// See: <see cref="GetRemainingPath"/>
		/// </summary>
		public Vector3 endOfPath {
			get {
				if (entityExists) {
					// Make sure we block to ensure no managed state changes are made in jobs while we are reading from it
					managedStateAccessRO.Update(World.DefaultGameObjectInjectionWorld.EntityManager);
					if (hasPath) return managedState.pathTracer.endPoint;
					var d = destination;
					if (float.IsFinite(d.x)) return d;
				}
				return position;
			}
		}

		static NativeList<float3> nextCornersScratch;
		static NativeArray<int> indicesScratch;
		static int scratchReferenceCount = 0;

		/// <summary>\copydoc Pathfinding::IAstarAI::destination</summary>

		/// <summary>
		/// Position in the world that this agent should move to.
		///
		/// If no destination has been set yet, then (+infinity, +infinity, +infinity) will be returned.
		///
		/// Setting this property will immediately try to repair the path if the agent already has a path.
		/// This will also immediately update properties like <see cref="reachedDestination"/>, <see cref="reachedEndOfPath"/> and <see cref="remainingDistance"/>.
		///
		/// The agent may do a full path recalculation if the local repair was not sufficient,
		/// but this will at earliest happen in the next simulation step.
		///
		/// <code>
		/// IEnumerator Start () {
		///     ai.destination = somePoint;
		///     // Wait until the AI has reached the destination
		///     while (!ai.reachedEndOfPath) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		///
		/// See: <see cref="SetDestination"/>, which also allows you to set a facing direction for the agent.
		/// </summary>
		public Vector3 destination {
			get => entityStorageCache.GetComponentData(entity, ref destinationPointAccessRO, out var destination) ? (Vector3)destination.value.destination : Vector3.positiveInfinity;
			set => SetDestination(value, default);
		}

		/// <summary>
		/// Direction the agent will try to face when it reaches the destination.
		///
		/// If this is zero, the agent will not try to face any particular direction.
		///
		/// The following video shows three agents, one with no facing direction set, and then two agents with varying values of the <see cref="PIDMovement.leadInRadiusWhenApproachingDestination;lead in radius"/>.
		/// [Open online documentation to see videos]
		///
		/// See: <see cref="MovementSettings.follower.leadInRadiusWhenApproachingDestination"/>
		/// See: <see cref="SetDestination"/>
		/// </summary>
		Vector3 destinationFacingDirection {
			get => entityStorageCache.GetComponentData(entity, ref destinationPointAccessRO, out var destination) ? (Vector3)destination.value.facingDirection : Vector3.zero;
		}

		/// <summary>
		/// Set the position in the world that this agent should move to.
		///
		/// This method will immediately try to repair the path if the agent already has a path.
		/// This will also immediately update properties like <see cref="reachedDestination"/>, <see cref="reachedEndOfPath"/> and <see cref="remainingDistance"/>.
		/// The agent may do a full path recalculation if the local repair was not sufficient,
		/// but this will at earliest happen in the next simulation step.
		///
		/// If you are setting a destination and want to know when the agent has reached that destination,
		/// then you could use either <see cref="reachedDestination"/> or <see cref="reachedEndOfPath"/>.
		///
		/// You may also set a facing direction for the agent. If set, the agent will try to approach the destination point
		/// with the given heading. <see cref="reachedDestination"/> and <see cref="reachedEndOfPath"/> will only return true once the agent is approximately facing the correct direction.
		/// The <see cref="MovementSettings.follower.leadInRadiusWhenApproachingDestination"/> field controls how wide an arc the agent will try to use when approaching the destination.
		///
		/// The following video shows three agents, one with no facing direction set, and then two agents with varying values of the <see cref="PIDMovement.leadInRadiusWhenApproachingDestination;lead in radius"/>.
		/// [Open online documentation to see videos]
		///
		/// <code>
		/// IEnumerator Start () {
		///     ai.SetDestination(somePoint, Vector3.right);
		///     // Wait until the AI has reached the destination and is rotated to the right in world space
		///     while (!ai.reachedEndOfPath) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		///
		/// See: <see cref="destination"/>
		/// </summary>
		public void SetDestination (float3 destination, float3 facingDirection = default) {
			AssertEntityExists();
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			movementStateAccessRW.Update(entityManager);
			managedStateAccessRW.Update(entityManager);
			agentCylinderShapeAccessRO.Update(entityManager);
			movementSettingsAccessRO.Update(entityManager);
			localTransformAccessRO.Update(entityManager);
			destinationPointAccessRW.Update(entityManager);
			movementPlaneAccessRO.Update(entityManager);

			var storage = entityManager.GetStorageInfo(entity);
			destinationPointAccessRW[storage] = new DestinationPoint {
				destination = destination,
				facingDirection = facingDirection,
			};

			// If we already have a path, we try to repair it immediately.
			// This ensures that the #reachedDestination and #reachedEndOfPath flags are as up to date as possible.
			if (managedState.pathTracer.hasPath) {
				Profiler.BeginSample("RepairEnd");
				ref var movementPlane = ref movementPlaneAccessRO[storage];
				managedState.pathTracer.UpdateEnd(destination, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
				Profiler.EndSample();
				ref var movementState = ref movementStateAccessRW[storage];
				if (movementState.pathTracerVersion != managedState.pathTracer.version) {
					Profiler.BeginSample("EstimateNative");
					ref var shape = ref agentCylinderShapeAccessRO[storage];
					ref var movementSettings = ref movementSettingsAccessRO[storage];
					ref var localTransform = ref localTransformAccessRO[storage];
					ref var destinationPoint = ref destinationPointAccessRW[storage];
					if (!nextCornersScratch.IsCreated) nextCornersScratch = new NativeList<float3>(4, Allocator.Persistent);
					RepairPathJob.Execute(
						ref localTransform,
						ref movementState,
						ref shape,
						ref movementPlane,
						ref destinationPoint,
						managedState,
						in movementSettings,
						nextCornersScratch,
						ref indicesScratch,
						Allocator.Persistent,
						false
						);
					Profiler.EndSample();
				}
			}
		}

		/// <summary>
		/// Policy for when the agent recalculates its path.
		///
		/// See: <see cref="AutoRepathPolicy"/>
		/// </summary>
		public AutoRepathPolicy autoRepath {
			get {
				// Ensure there are no race conditions when accessing the managed state.
				// We can't quite guarantee this, since a user may keep a reference to the autoRepathPolicy
				// but this should catch 99% of all cases.
				managedStateAccessRW.Update(World.DefaultGameObjectInjectionWorld.EntityManager);
				return managedState.autoRepath;
			}
		}

		/// <summary>
		/// \copydoc Pathfinding::IAstarAI::canSearch
		/// Deprecated: This has been superseded by <see cref="autoRepath.mode"/>.
		/// </summary>
		[System.Obsolete("This has been superseded by autoRepath.mode")]
		public bool canSearch {
			get {
				return managedState.autoRepath.mode != AutoRepathPolicy.Mode.Never;
			}
			set {
				if (value) {
					if (managedState.autoRepath.mode == AutoRepathPolicy.Mode.Never) {
						managedState.autoRepath.mode = AutoRepathPolicy.Mode.EveryNSeconds;
					}
				} else {
					managedState.autoRepath.mode = AutoRepathPolicy.Mode.Never;
				}
			}
		}

		/// <summary>
		/// Enables or disables movement completely.
		/// If you want the agent to stand still, but still react to local avoidance and use gravity: use <see cref="isStopped"/> instead.
		///
		/// Disabling this will remove the <see cref="SimulateMovement"/> component from the entity, which prevents
		/// most systems from running for this entity.
		///
		/// See: <see cref="autoRepath"/>
		/// See: <see cref="isStopped"/>
		/// </summary>
		public bool canMove {
			get {
				if (!entityExists) return true;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				return entityManager.HasComponent<SimulateMovement>(entity);
			}
			set => ToggleComponent<SimulateMovement>(entity, value, true);
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::movementPlane</summary>
		public NativeMovementPlane movementPlane => entityStorageCache.GetComponentData(entity, ref movementPlaneAccessRO, out var movementPlane) ? movementPlane.value.value : new NativeMovementPlane(rotation);

		/// <summary>
		/// Enables or disables gravity.
		///
		/// If gravity is enabled, the agent will accelerate downwards, and use a raycast to check if it should stop falling.
		///
		/// See: <see cref="groundMask"/>
		/// </summary>
		public bool enableGravity {
			get {
				if (!entityExists) return managedState.enableGravity;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				return entityManager.HasComponent<GravityState>(entity);
			}
			set {
				if (managedState.enableGravity != value) {
					managedState.enableGravity = value;
					ToggleComponent<GravityState>(entity, value, false);
				}
			}
		}

		/// <summary>\copydocref{ManagedState.enableLocalAvoidance}</summary>
		public bool enableLocalAvoidance {
			get => managedState.enableLocalAvoidance;
			set => managedState.enableLocalAvoidance = value;
		}

		/// <summary>
		/// Determines if the character's position should be coupled to the Transform's position.
		/// If false then all movement calculations will happen as usual, but the GameObject that this component is attached to will not move.
		/// Instead, only the <see cref="position"/> property and the internal entity's position will change.
		///
		/// This is useful if you want to control the movement of the character using some other means, such
		/// as root motion, but still want the AI to move freely.
		///
		/// See: <see cref="canMove"/> which in contrast to this field will disable all movement calculations.
		/// See: <see cref="updateRotation"/>
		/// </summary>
		public bool updatePosition {
			get {
				if (!entityExists) return true;
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				return entityManager.HasComponent<SyncPositionWithTransform>(entity);
			}
			set => ToggleComponent<SyncPositionWithTransform>(entity, value, true);
		}

		/// <summary>
		/// Determines which direction the agent moves in.
		/// For 3D games you most likely want the ZAxisIsForward option as that is the convention for 3D games.
		/// For 2D games you most likely want the YAxisIsForward option as that is the convention for 2D games.
		///
		/// When using ZAxisForard, the +Z axis will be the forward direction of the agent, +Y will be upwards, and +X will be the right direction.
		/// When using YAxisForward, the +Y axis will be the forward direction of the agent, +Z will be upwards, and +X will be the right direction.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public OrientationMode orientation {
			get => orientationBacking;
			set {
				if (orientationBacking != value) {
					orientationBacking = value;
					ToggleComponent<OrientationYAxisForward>(entity, value == OrientationMode.YAxisForward, false);
				}
			}
		}

		/// <summary>
		/// Determines if the character's rotation should be coupled to the Transform's rotation.
		/// If false then all movement calculations will happen as usual, but the GameObject that this component is attached to will not rotate.
		/// Instead, only the <see cref="rotation"/> property and the internal entity's rotation will change.
		///
		/// You can enable <see cref="PIDMovement.DebugFlags"/>.Rotation in <see cref="debugFlags"/> to draw a gizmos arrow in the scene view to indicate the agent's internal rotation.
		///
		/// See: <see cref="updatePosition"/>
		/// See: <see cref="rotation"/>
		/// See: <see cref="orientation"/>
		/// </summary>
		public bool updateRotation {
			get {
				if (!entityExists) return true;
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				return entityManager.HasComponent<SyncRotationWithTransform>(entity);
			}
			set => ToggleComponent<SyncRotationWithTransform>(entity, value, true);
		}

		/// <summary>Adds or removes a component from an entity</summary>
		static void ToggleComponent<T>(Entity entity, bool enabled, bool mustExist) where T : struct, IComponentData {
			var world = World.DefaultGameObjectInjectionWorld;
			if (world == null || !world.EntityManager.Exists(entity)) {
				if (!mustExist) throw new System.InvalidOperationException("Entity does not exist. You can only access this if the component is active and enabled.");
				return;
			}
			if (enabled) {
				world.EntityManager.AddComponent<T>(entity);
			} else {
				world.EntityManager.RemoveComponent<T>(entity);
			}
		}

		/// <summary>
		/// True if this agent currently has a valid path that it follows.
		///
		/// This is true if the agent has a path and the path is not stale.
		///
		/// A path may become stale if the graph is updated close to the agent and it hasn't had time to recalculate its path yet.
		/// </summary>
		public bool hasPath {
			get {
				// Ensure no jobs are writing to the managed state while we are reading from it
				if (entityExists) managedStateAccessRO.Update(World.DefaultGameObjectInjectionWorld.EntityManager);
				return !managedState.pathTracer.isStale;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::pathPending</summary>
		public bool pathPending {
			get {
				if (!entityExists) return false;
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				managedStateAccessRO.Update(entityManager);
				return managedState.pendingPath != null;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::isStopped</summary>
		public bool isStopped {
			get => movement.isStopped;
			set {
				if (movement.isStopped != value) {
					movement.isStopped = value;
					if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
						movementSettings.value.isStopped = value;
					}
				}
			}
		}

		/// <summary>
		/// Various movement settings.
		///
		/// Some of these settings are exposed on the FollowerEntity directly. For example <see cref="maxSpeed"/>.
		///
		/// Note: The return value is a struct. If you want to change some settings, you'll need to modify the returned struct and then assign it back to this property.
		/// </summary>
		public MovementSettings movementSettings {
			get => movement;
			set {
				movement = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value = value;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::steeringTarget</summary>
		public Vector3 steeringTarget => entityStorageCache.GetComponentData(entity, ref movementStateAccessRO, out var movementState) ? (Vector3)movementState.value.nextCorner : position;

		/// <summary>\copydoc Pathfinding::IAstarAI::onSearchPath</summary>
		Action IAstarAI.onSearchPath {
			get => null;
			set => throw new NotImplementedException("The FollowerEntity does not support this property.");
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::FinalizeMovement</summary>
		void IAstarAI.FinalizeMovement (Vector3 nextPosition, Quaternion nextRotation) {
			throw new InvalidOperationException("The FollowerEntity component does not support FinalizeMovement. Use an ECS system to override movement instead");
		}

		/// <summary>\copydocref{IAstarAI.GetRemainingPath}</summary>
		public void GetRemainingPath (List<Vector3> buffer, out bool stale) {
			if (!entityExists) {
				buffer.Add(position);
				stale = true;
				return;
			}

			var ms = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<ManagedState>(entity);
			if (ms.pathTracer.hasPath) {
				var nativeBuffer = new NativeList<float3>(Allocator.Temp);
				var scratch = new NativeArray<int>(8, Allocator.Temp);
				ms.pathTracer.GetNextCorners(nativeBuffer, int.MaxValue, ref scratch, Allocator.Temp, ms.pathfindingSettings.traversalProvider, ms.activePath);
				for (int i = 0; i < nativeBuffer.Length; i++) {
					buffer.Add(nativeBuffer[i]);
				}
			} else {
				buffer.Add(position);
			}
			stale = ms.pathTracer.isStale;
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::Move</summary>
		public void Move (Vector3 deltaPosition) {
			throw new NotImplementedException();
		}

		void IAstarAI.MovementUpdate (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			throw new InvalidOperationException("The FollowerEntity component does not support MovementUpdate. Use an ECS system to override movement instead");
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::SearchPath</summary>
		public void SearchPath () {
			var dest = destination;
			if (!float.IsFinite(dest.x)) return;

			SetPath(ABPath.Construct(position, dest, null), false);
		}

		void AssertEntityExists () {
			if (World.DefaultGameObjectInjectionWorld == null || !World.DefaultGameObjectInjectionWorld.EntityManager.Exists(entity)) throw new System.InvalidOperationException("Entity does not exist. You can only access this if the component is active and enabled.");
		}

		/// <summary>
		/// True if this component's entity exists.
		///
		/// This is typically true if the component is active and enabled and the game is running.
		///
		/// See: <see cref="entity"/>
		/// </summary>
		public bool entityExists => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.Exists(entity);

		void CancelCurrentPathRequest () {
			if (entityExists) {
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				managedStateAccessRW.Update(entityManager);
				managedState.CancelCurrentPathRequest();
			}
		}

		void ClearPath() => ClearPath(entity);

		static void ClearPath (Entity entity) {
			if (entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) {
				agentOffMeshLinkTraversalRO.Update(entityManager);

				if (agentOffMeshLinkTraversalRO.HasComponent(storage)) {
					// Agent is traversing an off-mesh link. We must abort this link traversal.
					var managedInfo = entityManager.GetComponentData<ManagedAgentOffMeshLinkTraversal>(entity);
					if (managedInfo.stateMachine != null) managedInfo.stateMachine.OnAbortTraversingOffMeshLink();
					managedInfo.context.Restore();
					entityManager.RemoveComponent<AgentOffMeshLinkTraversal>(entity);
					entityManager.RemoveComponent<ManagedAgentOffMeshLinkTraversal>(entity);
					// We need to get the storage info again, because the entity will have been moved to another chunk
					entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out entityManager, out storage);
				}

				managedStateAccessRW.Update(entityManager);
				movementStateAccessRW.Update(entityManager);
				localTransformAccessRO.Update(entityManager);
				movementPlaneAccessRO.Update(entityManager);
				resolvedMovementAccessRW.Update(entityManager);
				movementControlAccessRW.Update(entityManager);

				ref var movementState = ref movementStateAccessRW[storage];
				ref var localTransform = ref localTransformAccessRO[storage];
				ref var movementPlane = ref movementPlaneAccessRO[storage];
				ref var resolvedMovement = ref resolvedMovementAccessRW[storage];
				ref var controlOutput = ref movementControlAccessRW[storage];
				var managedState = managedStateAccessRW[storage];

				managedState.ClearPath();
				managedState.CancelCurrentPathRequest();
				movementState.SetPathIsEmpty(localTransform.Position);

				// This emulates what the ControlJob does when the agent has no path.
				// This ensures that properties like #desiredVelocity return the correct value immediately after the path has been cleared.
				resolvedMovement.targetPoint = localTransform.Position;
				resolvedMovement.speed = 0;
				resolvedMovement.targetRotation = movementPlane.value.ToPlane(localTransform.Rotation);
				controlOutput.endOfPath = movementState.endOfPath;
				controlOutput.speed = 0f;
				controlOutput.targetPoint = localTransform.Position;
			}
		}

		/// <summary>
		/// Make the AI follow the specified path.
		///
		/// In case the path has not been calculated, the script will schedule the path to be calculated.
		/// This means the AI may not actually start to follow the path until in a few frames when the path has been calculated.
		/// The <see cref="pathPending"/> field will, as usual, return true while the path is being calculated.
		///
		/// In case the path has already been calculated, it will immediately replace the current path the AI is following.
		///
		/// If you pass null path, then the current path will be cleared and the agent will stop moving.
		/// The agent will also abort traversing any off-mesh links it is currently traversing.
		/// Note than unless you have also disabled <see cref="canSearch"/>, then the agent will soon recalculate its path and start moving again.
		///
		/// Note: Stopping the agent by passing a null path works. But this will stop the agent instantly, and it will not be able to use local avoidance or know its place on the navmesh.
		/// Usually it's better to set <see cref="isStopped"/> to false, which will make the agent slow down smoothly.
		///
		/// You can disable the automatic path recalculation by setting the <see cref="canSearch"/> field to false.
		///
		/// Note: This call will be ignored if the agent is currently traversing an off-mesh link.
		/// Furthermore, if the agent starts traversing an off-mesh link, the current path request will be canceled (if one is currently in progress).
		///
		/// <code>
		/// IEnumerator Start () {
		///     var pointToAvoid = enemy.position;
		///     // Make the AI flee from an enemy.
		///     // The path will be about 20 world units long (the default cost of moving 1 world unit is 1000).
		///     var path = FleePath.Construct(ai.position, pointToAvoid, 1000 * 20);
		///     ai.SetPath(path);
		///
		///     while (!ai.reachedEndOfPath) {
		///         yield return null;
		///     }
		/// }
		/// </code>
		/// </summary>
		/// <param name="path">The path to follow.</param>
		/// <param name="updateDestinationFromPath">If true, the \reflink{destination} property will be set to the end point of the path. If false, the previous destination value will be kept.
		///     If you pass a path which has no well defined destination before it is calculated (e.g. a MultiTargetPath or RandomPath), then the destination will be first be cleared, but once the path has been calculated, it will be set to the end point of the path.</param>
		public void SetPath(Path path, bool updateDestinationFromPath = true) => SetPath(entity, path, updateDestinationFromPath);

		/// <summary>\copydocref{SetPath(Path,bool)}</summary>
		public static void SetPath (Entity entity, Path path, bool updateDestinationFromPath = true) {
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!entityManager.Exists(entity)) throw new System.InvalidOperationException("Entity does not exist. You can only assign a path if the component is active and enabled.");

			managedStateAccessRW.Update(entityManager);
			movementPlaneAccessRO.Update(entityManager);
			agentOffMeshLinkTraversalRO.Update(entityManager);
			movementStateAccessRW.Update(entityManager);
			localTransformAccessRO.Update(entityManager);
			destinationPointAccessRW.Update(entityManager);

			var storage = entityManager.GetStorageInfo(entity);

			bool isTraversingOffMeshLink = agentOffMeshLinkTraversalRO.HasComponent(storage);
			if (isTraversingOffMeshLink) {
				// Agent is traversing an off-mesh link. We ignore any path updates during this time.
				// TODO: Race condition when adding off mesh link component?
				// TODO: If passing null, should we clear the whole path after the off-mesh link?
				return;
			}

			if (path == null) {
				ClearPath(entity);
				return;
			}

			var managedState = managedStateAccessRW[storage];
			ref var movementPlane = ref movementPlaneAccessRO[storage];
			ref var movementState = ref movementStateAccessRW[storage];
			ref var localTransform = ref localTransformAccessRO[storage];
			ref var destination = ref destinationPointAccessRW[storage];

			if (updateDestinationFromPath && path is ABPath abPath) {
				// If the user supplies a new ABPath manually, they probably want the agent to move to that point.
				// So by default we update the destination to match the path.
				if (abPath.endPointKnownBeforeCalculation) {
					destination = new DestinationPoint { destination = abPath.originalEndPoint, facingDirection = default };
				} else {
					// If the destination is not known, we set it to positive infinity.
					// This is the case for MultiTargetPath and RandomPath, for example.
					destination = new DestinationPoint { destination = Vector3.positiveInfinity, facingDirection = default };
				}
			}

			ManagedState.SetPath(path, managedState, in movementPlane, ref destination);

			if (path.IsDone()) {
				agentCylinderShapeAccessRO.Update(entityManager);
				movementSettingsAccessRO.Update(entityManager);

				// This remaining part ensures that the path tracer is fully up to date immediately after the path has been assigned.
				// So that things like GetRemainingPath, and various properties like reachedDestination are up to date immediately.
				managedState.pathTracer.UpdateStart(localTransform.Position, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
				managedState.pathTracer.UpdateEnd(destination.destination, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);

				if (movementState.pathTracerVersion != managedState.pathTracer.version) {
					if (!nextCornersScratch.IsCreated) nextCornersScratch = new NativeList<float3>(4, Allocator.Persistent);
					ref var shape = ref agentCylinderShapeAccessRO[storage];
					ref var movementSettings = ref movementSettingsAccessRO[storage];
					RepairPathJob.Execute(
						ref localTransform,
						ref movementState,
						ref shape,
						ref movementPlane,
						ref destination,
						managedState,
						in movementSettings,
						nextCornersScratch,
						ref indicesScratch,
						Allocator.Persistent,
						false
						);
				}
			}
		}

		/// <summary>
		/// Instantly move the agent to a new position.
		///
		/// The current path will be cleared by default.
		///
		/// This method is preferred for long distance teleports. If you only move the agent a very small distance (so that it is reasonable that it can keep its current path),
		/// then setting the <see cref="position"/> property is preferred.
		/// Setting the <see cref="position"/> property very far away from the agent could cause the agent to fail to move the full distance, as it can get blocked by the navmesh.
		///
		/// See: Works similarly to Unity's NavmeshAgent.Warp.
		/// See: <see cref="position"/>
		/// See: <see cref="SearchPath"/>
		/// </summary>
		public void Teleport (Vector3 newPosition, bool clearPath = true) {
			if (clearPath) ClearPath();

			if (entityExists) {
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				movementOutputAccessRW.Update(entityManager);
				managedStateAccessRW.Update(entityManager);
				movementPlaneAccessRO.Update(entityManager);
				var storage = entityManager.GetStorageInfo(entity);
				ref var movementOutput = ref movementOutputAccessRW[storage];
				movementOutput.lastPosition = newPosition;
				managedState.CancelCurrentPathRequest();
				if (AstarPath.active != null) {
					// TODO: Should we use the from-above distance metric here?
					// This would fail when used on a spherical world and the agent was teleported
					// to another part of the sphere.
					var nearest = AstarPath.active.GetNearest(newPosition, NNConstraint.Walkable);
					if (nearest.node != null) {
						var movementPlane = movementPlaneAccessRO[storage];
						managedState.pathTracer.SetFromSingleNode(nearest.node, nearest.position, movementPlane.value);
					}
				}

				// Note: Since we are starting from a completely new path,
				// setting the position will also cause the path tracer to repair the destination.
				// Therefore we don't have to also set the destination here.
				position = newPosition;
			} else {
				position = newPosition;
			}
		}

		void FindComponents () {
			tr = transform;
		}

		static readonly Color ShapeGizmoColor = new Color(240/255f, 213/255f, 30/255f);

		public override void DrawGizmos () {
			if (!Application.isPlaying || !enabled) FindComponents();

			var color = ShapeGizmoColor;
			var destination = this.destination;
			var rotation = this.rotation;

			if (orientation == OrientationMode.YAxisForward) {
				Draw.Circle(position, rotation * Vector3.forward, shape.radius * tr.localScale.x, color);
			} else {
				Draw.WireCylinder(position, rotation * Vector3.up, tr.localScale.y * shape.height, shape.radius * tr.localScale.x, color);
			}

			if (!updateRotation) {
				Draw.ArrowheadArc(position, rotation * Vector3.forward, shape.radius * tr.localScale.x * 1.05f, color);
			}

			if (!float.IsPositiveInfinity(destination.x) && Application.isPlaying) {
				var dir = destinationFacingDirection;
				if (dir != Vector3.zero) {
					Draw.xz.ArrowheadArc(destination, dir, 0.25f, Color.blue);
				}
				Draw.xz.Circle(destination, 0.2f, Color.blue);
			}
		}

		[System.Flags]
		enum FollowerEntityMigrations {
			MigratePathfindingSettings = 1 << 0,
		}

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			if (migrations.TryMigrateFromLegacyFormat(out var _legacyVersion)) {
				// Only 1 version of the previous version format existed for this component
				if (this.pathfindingSettings.tagPenalties.Length != 0) migrations.MarkMigrationFinished((int)FollowerEntityMigrations.MigratePathfindingSettings);
			}

			if (migrations.AddAndMaybeRunMigration((int)FollowerEntityMigrations.MigratePathfindingSettings, unityThread)) {
				if (TryGetComponent<Seeker>(out var seeker)) {
					this.pathfindingSettings = new PathRequestSettings {
						graphMask = seeker.graphMask,
						traversableTags = seeker.traversableTags,
						tagPenalties = seeker.tagPenalties,
					};
					UnityEngine.Object.DestroyImmediate(seeker);
				} else {
					this.pathfindingSettings = PathRequestSettings.Default;
				}
			}
		}

#if UNITY_EDITOR
		/// <summary>\cond IGNORE_IN_DOCS</summary>

		/// <summary>
		/// Copies all settings from this component to the entity's components.
		///
		/// Note: This is an internal method and you should never need to use it yourself.
		/// Typically it is used by the editor to keep the entity's state in sync with the component's state.
		/// </summary>
		public void SyncWithEntity () {
			if (!entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) return;

			this.position = this.position;
			movementSettingsAccessRW.Update(entityManager);
			managedStateAccessRW.Update(entityManager);
			agentCylinderShapeAccessRW.Update(entityManager);

			SyncWithEntity(managedStateAccessRW[storage], ref agentCylinderShapeAccessRW[storage], ref movementSettingsAccessRW[storage]);
			ToggleComponent<GravityState>(entity, enableGravity, false);
			ToggleComponent<OrientationYAxisForward>(entity, orientation == OrientationMode.YAxisForward, false);
		}

		/// <summary>
		/// Copies all settings from this component to the entity's components.
		///
		/// Note: This is an internal method and you should never need to use it yourself.
		/// </summary>
		public void SyncWithEntity (ManagedState managedState, ref AgentCylinderShape shape, ref MovementSettings movementSettings) {
			movementSettings = this.movement;
			shape = this.shape;
			// Copy all fields to the managed state object.
			// Don't copy the PathTracer or the onTraverseOffMeshLink, though, since they are not serialized
			managedState.autoRepath = this.managedState.autoRepath;
			managedState.rvoSettings = this.managedState.rvoSettings;
			managedState.enableLocalAvoidance = this.managedState.enableLocalAvoidance;
			// Replace this instance of the managed state with the entity component
			this.managedState = managedState;
			// Note: RVO settings are copied every frame automatically before local avoidance simulations
		}

		static List<FollowerEntity> needsSyncWithEntityList = new List<FollowerEntity>();

		void ISerializationCallbackReceiver.OnBeforeSerialize () {}

		void ISerializationCallbackReceiver.OnAfterDeserialize () {
			UpgradeSerializedData(false);

			// This is (among other times) called after an undo or redo event has happened.
			// In that case, the entity's state might be out of sync with this component's state,
			// so we need to sync the two together. Unfortunately this method is called
			// from Unity's separate serialization thread, so we cannot access the entity directly.
			// Instead we add this component to a list and make sure to process them in the next
			// editor update.
			lock (needsSyncWithEntityList) {
				needsSyncWithEntityList.Add(this);
				if (needsSyncWithEntityList.Count == 1) {
					UnityEditor.EditorApplication.update += SyncWithEntities;
				}
			}
		}

		static void SyncWithEntities () {
			lock (needsSyncWithEntityList) {
				for (int i = 0; i < needsSyncWithEntityList.Count; i++) {
					needsSyncWithEntityList[i].SyncWithEntity();
				}
				needsSyncWithEntityList.Clear();
				UnityEditor.EditorApplication.update -= SyncWithEntities;
			}
		}

		/// <summary>\endcond</summary>
#endif
	}
}
#else
namespace Pathfinding {
	public sealed partial class FollowerEntity : VersionedMonoBehaviour {
		public void Start () {
			UnityEngine.Debug.LogError("The FollowerEntity component requires at least version 1.0 of the 'Entities' package to be installed. You can install it using the Unity package manager.");
		}

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			// Since most of the code for this component is stripped out, we should just preserve the current state,
			// and not try to migrate anything.
			// If we don't do this, then the base code will log an error about an unknown migration already being done.
			migrations.IgnoreMigrationAttempt();
		}
	}
}
#endif
