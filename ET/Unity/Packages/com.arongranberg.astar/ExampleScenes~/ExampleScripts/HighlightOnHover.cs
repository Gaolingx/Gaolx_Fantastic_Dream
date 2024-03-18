using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>Activates a GameObject when the cursor is over this object.</summary>
	public class HighlightOnHover : VersionedMonoBehaviour {
		public GameObject highlight;

		void Start () {
			highlight.SetActive(false);
		}

		public void OnMouseEnter () {
			highlight.SetActive(true);
		}

		public void OnMouseExit () {
			highlight.SetActive(false);
		}
	}
}
