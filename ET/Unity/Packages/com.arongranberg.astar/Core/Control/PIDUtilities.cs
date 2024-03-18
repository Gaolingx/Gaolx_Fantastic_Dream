using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Assertions;
using Pathfinding.Util;

namespace Pathfinding.PID {
	public struct AnglePIDControlOutput2D {
		/// <summary>How much to rotate in a single time-step. In radians.</summary>
		public float rotationDelta;
		public float targetRotation;
		/// <summary>How much to move in a single time-step. In world units.</summary>
		public float2 positionDelta;

		public AnglePIDControlOutput2D(float currentRotation, float targetRotation, float rotationDelta, float moveDistance) {
			var midpointRotation = currentRotation + rotationDelta * 0.5f;
			math.sincos(midpointRotation, out float s, out float c);
			this.rotationDelta = rotationDelta;
			this.positionDelta = new float2(c, s) * moveDistance;
			this.targetRotation = targetRotation;
		}

		public static AnglePIDControlOutput2D WithMovementAtEnd (float currentRotation, float targetRotation, float rotationDelta, float moveDistance) {
			var finalRotation = currentRotation + rotationDelta;
			math.sincos(finalRotation, out float s, out float c);
			return new AnglePIDControlOutput2D {
					   rotationDelta = rotationDelta,
					   targetRotation = targetRotation,
					   positionDelta = new float2(c, s) * moveDistance,
			};
		}
	}

	public struct AnglePIDControlOutput {
		/// <summary>How much to rotate in a single time-step</summary>
		public quaternion rotationDelta;
		/// <summary>How much to move in a single time-step. In world units.</summary>
		public float3 positionDelta;
		public float maxDesiredWallDistance;

		public AnglePIDControlOutput(NativeMovementPlane movementPlane, AnglePIDControlOutput2D control2D) {
			this.rotationDelta = movementPlane.ToWorldRotationDelta(-control2D.rotationDelta);
			this.positionDelta = movementPlane.ToWorld(control2D.positionDelta, 0);
			this.maxDesiredWallDistance = 0;
			Assert.IsTrue(math.all(math.isfinite(rotationDelta.value)));
			Assert.IsTrue(math.all(math.isfinite(positionDelta)));
		}
	}
}
