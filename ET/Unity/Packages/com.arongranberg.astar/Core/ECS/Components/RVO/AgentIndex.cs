using Pathfinding.RVO;
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Transforms;
#endif
using UnityEngine;
using Unity.Mathematics;

namespace Pathfinding.ECS.RVO {
	using Pathfinding.RVO;

	/// <summary>
	/// Index of an RVO agent in the local avoidance simulation.
	///
	/// If this component is present, that indicates that the agent is part of a local avoidance simulation.
	/// The <see cref="RVOSystem"/> is responsible for adding and removing this component as necessary.
	/// Any other systems should only concern themselves with the <see cref="RVOAgent"/> component.
	///
	/// Warning: This component does not support cloning. You must not clone entities that use this component.
	/// There doesn't seem to be any way to make this work with the Unity.Entities API at the moment.
	/// </summary>
#if MODULE_ENTITIES
	[WriteGroup(typeof(ResolvedMovement))]
#endif
	public readonly struct AgentIndex
#if MODULE_ENTITIES
		: Unity.Entities.ICleanupComponentData
#endif
	{
		internal const int DeletedBit = 1 << 31;
		internal const int IndexMask = (1 << 24) - 1;
		internal const int VersionOffset = 24;
		internal const int VersionMask = 0b1111_111 << VersionOffset;

		public readonly int packedAgentIndex;
		public int Index => packedAgentIndex & IndexMask;
		public int Version => packedAgentIndex & VersionMask;
		public bool Valid => (packedAgentIndex & DeletedBit) == 0;

		public AgentIndex(int packedAgentIndex) {
			this.packedAgentIndex = packedAgentIndex;
		}

		public AgentIndex(int version, int index) {
			version <<= VersionOffset;
			UnityEngine.Assertions.Assert.IsTrue((index & IndexMask) == index);
			packedAgentIndex = (version & VersionMask) | (index & IndexMask);
		}

		public AgentIndex WithIncrementedVersion () {
			return new AgentIndex((((packedAgentIndex & VersionMask) + (1 << VersionOffset)) & VersionMask) | Index);
		}

		public AgentIndex WithDeleted () {
			return new AgentIndex(packedAgentIndex | DeletedBit);
		}
	}
}
