#if MODULE_ENTITIES
using UnityEditor;
using UnityEngine;
using Pathfinding.RVO;
using Pathfinding.ECS;
using System.Linq;

namespace Pathfinding {
	[CustomEditor(typeof(FollowerEntity), true)]
	[CanEditMultipleObjects]
	public class FollowerEntityEditor : EditorBase {
		bool debug = false;
		bool tagPenaltiesOpen;

		protected override void OnDisable () {
			base.OnDisable();
			EditorPrefs.SetBool("FollowerEntity.debug", debug);
			EditorPrefs.SetBool("FollowerEntity.tagPenaltiesOpen", tagPenaltiesOpen);
		}

		protected override void OnEnable () {
			base.OnEnable();
			debug = EditorPrefs.GetBool("FollowerEntity.debug", false);
			tagPenaltiesOpen = EditorPrefs.GetBool("FollowerEntity.tagPenaltiesOpen", false);
		}

		public override bool RequiresConstantRepaint () {
			// When the debug inspector is open we want to update it every frame
			// as the agent can move
			return debug && Application.isPlaying;
		}

		protected void AutoRepathInspector () {
			var mode = FindProperty("managedState.autoRepath.mode");

			PropertyField(mode, "Recalculate paths automatically");
			if (!mode.hasMultipleDifferentValues) {
				var modeValue = (AutoRepathPolicy.Mode)mode.enumValueIndex;
				EditorGUI.indentLevel++;
				if (modeValue == AutoRepathPolicy.Mode.EveryNSeconds) {
					FloatField("managedState.autoRepath.period", min: 0f);
				} else if (modeValue == AutoRepathPolicy.Mode.Dynamic) {
					var maxInterval = FindProperty("managedState.autoRepath.maximumPeriod");
					FloatField(maxInterval, min: 0f);
					Slider("managedState.autoRepath.sensitivity", 1.0f, 20.0f);
					if (PropertyField("managedState.autoRepath.visualizeSensitivity")) {
						EditorGUILayout.HelpBox("When the game is running the sensitivity will be visualized as a magenta circle. The path will be recalculated immediately if the destination is outside the circle and more quickly if it is close to the edge.", MessageType.None);
					}
					EditorGUILayout.HelpBox("The path will be recalculated at least every " + maxInterval.floatValue.ToString("0.0") + " seconds, but more often if the destination changes quickly", MessageType.None);
				}
				EditorGUI.indentLevel--;
			}
		}

		protected void DebugInspector () {
			debug = EditorGUILayout.Foldout(debug, "Debug info");
			if (debug) {
				EditorGUI.indentLevel++;
				EditorGUI.BeginDisabledGroup(true);
				if (targets.Length == 1) {
					var ai = target as FollowerEntity;
					EditorGUILayout.Toggle("Reached Destination", ai.reachedDestination);
					EditorGUILayout.Toggle("Reached End Of Path", ai.reachedEndOfPath);
					EditorGUILayout.Toggle("Has Path", ai.hasPath);
					EditorGUILayout.Toggle("Path Pending", ai.pathPending);
					if (ai.isTraversingOffMeshLink) {
						EditorGUILayout.Toggle("Traversing off-mesh link", true);
					}
					EditorGUI.EndDisabledGroup();

					var newDestination = EditorGUILayout.Vector3Field("Destination", ai.destination);
					if (ai.entityExists) ai.destination = newDestination;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.LabelField("Remaining distance", ai.remainingDistance.ToString("0.00"));
					EditorGUILayout.LabelField("Speed", ai.velocity.magnitude.ToString("0.00"));
				} else {
					int nReachedDestination = 0;
					int nReachedEndOfPath = 0;
					int nPending = 0;
					for (int i = 0; i < targets.Length; i++) {
						var ai = targets[i] as IAstarAI;
						if (ai.reachedDestination) nReachedDestination++;
						if (ai.reachedEndOfPath) nReachedEndOfPath++;
						if (ai.pathPending) nPending++;
					}
					EditorGUILayout.LabelField("Reached Destination", nReachedDestination + " of " + targets.Length);
					EditorGUILayout.LabelField("Reached End Of Path", nReachedEndOfPath + " of " + targets.Length);
					EditorGUILayout.LabelField("Path Pending", nPending + " of " + targets.Length);
				}
				EditorGUI.EndDisabledGroup();
				EditorGUI.indentLevel--;
			}
		}

		void PathfindingSettingsInspector () {
			bool anyCustomTraversalProvider = this.targets.Any(s => (s as FollowerEntity).pathfindingSettings.traversalProvider != null);
			if (anyCustomTraversalProvider) {
				EditorGUILayout.HelpBox("Custom traversal provider active", MessageType.None);
			}

			PropertyField("managedState.pathfindingSettings.graphMask", "Traversable Graphs");

			tagPenaltiesOpen = EditorGUILayout.Foldout(tagPenaltiesOpen, new GUIContent("Tags", "Settings for each tag"));
			if (tagPenaltiesOpen) {
				EditorGUI.indentLevel++;
				var traversableTags = this.targets.Select(s => (s as FollowerEntity).pathfindingSettings.traversableTags).ToArray();
				SeekerEditor.TagsEditor(FindProperty("managedState.pathfindingSettings.tagPenalties"), traversableTags);
				for (int i = 0; i < targets.Length; i++) {
					(targets[i] as FollowerEntity).pathfindingSettings.traversableTags = traversableTags[i];
				}
				EditorGUI.indentLevel--;
			}
		}

		protected override void Inspector () {
			Undo.RecordObjects(targets, "Modify FollowerEntity settings");
			EditorGUI.BeginChangeCheck();
			Section("Shape");
			FloatField("shape.radius", min: 0.01f);
			FloatField("shape.height", min: 0.01f);
			Popup("orientationBacking", new [] { new GUIContent("ZAxisForward (for 3D games)"), new GUIContent("YAxisForward (for 2D games)") }, "Orientation");

			Section("Movement");
			FloatField("movement.follower.speed", min: 0f);
			FloatField("movement.follower.rotationSpeed", min: 0f);
			var maxRotationSpeed = FindProperty("movement.follower.rotationSpeed");
			FloatField("movement.follower.maxRotationSpeed", min: maxRotationSpeed.hasMultipleDifferentValues ? 0f : maxRotationSpeed.floatValue);
			if (ByteAsToggle("movement.follower.allowRotatingOnSpotBacking", "Allow Rotating On The Spot")) {
				EditorGUI.indentLevel++;
				FloatField("movement.follower.maxOnSpotRotationSpeed", min: 0f);
				FloatField("movement.follower.slowdownTimeWhenTurningOnSpot", min: 0f);
				EditorGUI.indentLevel--;
			}
			Slider("movement.positionSmoothing", left: 0f, right: 0.5f);
			Slider("movement.rotationSmoothing", left: 0f, right: 0.5f);
			FloatField("movement.follower.slowdownTime", min: 0f);
			FloatField("movement.stopDistance", min: 0f);
			FloatField("movement.follower.leadInRadiusWhenApproachingDestination", min: 0f);
			FloatField("movement.follower.desiredWallDistance", min: 0f);

			if (PropertyField("managedState.enableGravity", "Gravity")) {
				EditorGUI.indentLevel++;
				PropertyField("movement.groundMask", "Raycast Ground Mask");
				EditorGUI.indentLevel--;
			}
			var movementPlaneSource = FindProperty("movement.movementPlaneSource");
			PropertyField(movementPlaneSource);
			if (AstarPath.active != null && AstarPath.active.data.graphs != null) {
				var possiblySpherical = AstarPath.active.data.navmesh != null && !AstarPath.active.data.navmesh.RecalculateNormals;
				if (!possiblySpherical && !movementPlaneSource.hasMultipleDifferentValues && (MovementPlaneSource)movementPlaneSource.intValue == MovementPlaneSource.Raycast) {
					EditorGUILayout.HelpBox("Using raycasts as the movement plane source is only recommended if you have a spherical or otherwise non-planar world. It has a performance overhead.", MessageType.Info);
				}
			}

			Section("Pathfinding");
			PathfindingSettingsInspector();
			AutoRepathInspector();


			if (SectionEnableable("Local Avoidance", "managedState.enableLocalAvoidance")) {
				if (Application.isPlaying && RVOSimulator.active == null && !EditorUtility.IsPersistent(target)) {
					EditorGUILayout.HelpBox("There is no enabled RVOSimulator component in the scene. A single global RVOSimulator component is required for local avoidance.", MessageType.Warning);
				}
				FloatField("managedState.rvoSettings.agentTimeHorizon", min: 0f, max: 20.0f);
				FloatField("managedState.rvoSettings.obstacleTimeHorizon", min: 0f, max: 20.0f);
				PropertyField("managedState.rvoSettings.maxNeighbours");
				ClampInt("managedState.rvoSettings.maxNeighbours", min: 0, max: SimulatorBurst.MaxNeighbourCount);
				PropertyField("managedState.rvoSettings.layer");
				PropertyField("managedState.rvoSettings.collidesWith");
				Slider("managedState.rvoSettings.priority", left: 0f, right: 1.0f);
				PropertyField("managedState.rvoSettings.locked");
			}

			Section("Debug");
			PropertyField("movement.debugFlags", "Movement Debug Rendering");
			PropertyField("managedState.rvoSettings.debug", "Local Avoidance Debug Rendering");
			DebugInspector();

			if (EditorGUI.EndChangeCheck()) {
				for (int i = 0; i < targets.Length; i++) {
					var script = targets[i] as FollowerEntity;
					script.SyncWithEntity();
				}
			}
		}
	}
}
#else
using UnityEditor;
using UnityEngine;
using Pathfinding.ECS;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Pathfinding {
	// This inspector is only used if the Entities package is not installed
	[CustomEditor(typeof(FollowerEntity), true)]
	[CanEditMultipleObjects]
	public class FollowerEntityEditor : EditorBase {
		static AddRequest addRequest;

		protected override void Inspector () {
			if (addRequest != null) {
				if (addRequest.Status == StatusCode.Success) {
					addRequest = null;

					// If we get this far, unity did not successfully reload the assemblies.
					// Who knows what went wrong. Quite possibly restarting Unity will resolve the issue.
					EditorUtility.DisplayDialog("Installed Entities package", "The entities package has been installed. You may have to restart the editor for changes to take effect.", "Ok");
				} else if (addRequest.Status == StatusCode.Failure) {
					EditorGUILayout.HelpBox("Failed to install the Entities package. Please install it manually using the Package Manager." + (addRequest.Error != null ? "\n" + addRequest.Error.message : ""), MessageType.Error);
				} else {
					EditorGUILayout.HelpBox("Installing entities package...", MessageType.None);
				}
			} else {
				EditorGUILayout.HelpBox("This component requires the Entities package to be installed. Please install it using the Package Manager.", MessageType.Error);
				if (GUILayout.Button("Install entities package")) {
					addRequest = Client.Add("com.unity.entities");
				}
			}
		}
	}
}
#endif
