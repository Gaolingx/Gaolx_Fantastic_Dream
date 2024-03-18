using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples.RTS {
	public class RTSHarvestableResource : MonoBehaviour {
		public float value;
		public ResourceType resourceType;

		public bool harvestable {
			get {
				return value > 0;
			}
		}
	}

	public enum ResourceType {
		Crystal
	}
}
