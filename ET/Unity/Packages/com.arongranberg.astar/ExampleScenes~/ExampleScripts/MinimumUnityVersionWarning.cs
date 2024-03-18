#pragma warning disable IDE0051

using System.Collections;
using UnityEngine;
using Pathfinding.Util;

namespace Pathfinding.Examples {
	[ExecuteInEditMode]
	public class MinimumUnityVersionWarning : MonoBehaviour {
#if !MODULE_ENTITIES || !UNITY_2022_2_OR_NEWER
		bool requiresUnity2022_2;
		bool requiresEntities;


		void Awake () {
			requiresEntities = UnityCompatibility.FindAnyObjectByType<Pathfinding.FollowerEntity>() != null || UnityCompatibility.FindAnyObjectByType<Pathfinding.Examples.LightweightRVO>() != null;
			// Box colliders from scenes created in Unity 2022+ are not compatible with older versions of Unity. They will end with the wrong size.
			// The minimum version of the entitites package also requires Unity 2022
			requiresUnity2022_2 = UnityCompatibility.FindAnyObjectByType<BoxCollider>() != null || requiresEntities;
		}

		IEnumerator Start () {
			// Catch dynamically spawned prefabs
			yield return null;
			Awake();
		}
#endif

		void OnGUI () {
#if !UNITY_2022_2_OR_NEWER
			if (requiresUnity2022_2) {
				var rect = new Rect(Screen.width/2 - 325, Screen.height/2 - 30, 650, 60);
				GUILayout.BeginArea(rect, "", "box");
				GUILayout.Label("<b>Unity version too low</b>\nThis example scene can unfortunately not be played in your version of Unity, due to compatibility issues.\nYou must upgrade to Unity 2022.2 or later");
				GUILayout.EndArea();
				return;
			}
#endif

#if !MODULE_ENTITIES
			if (requiresEntities) {
				var rect = new Rect(Screen.width/2 - 325, Screen.height/2 - 30, 650, 80);
				GUILayout.BeginArea(rect, "", "box");
				GUILayout.Label("<b>Just one more step</b>\nThis example scene requires version 1.0 or higher of the <b>Entities</b> package to be installed\nYou can install it from the Unity Package Manager");
#if UNITY_EDITOR
				if (GUILayout.Button("Install")) {
					UnityEditor.PackageManager.Client.Add("com.unity.entities");
				}
#endif
				GUILayout.EndArea();
				return;
			}
#endif
		}
	}
}
