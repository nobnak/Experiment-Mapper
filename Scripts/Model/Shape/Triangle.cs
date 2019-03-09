using M.Base;
using M.UIElement;
using nobnak.Gist;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace M.Model.Shape {
	[System.Serializable]
	public class Triangle : BaseTriangleComplex {
		public static readonly Vector2[] DEFAULT_OUTPUT = new Vector2[] {
			new Vector2(-1f, -1f),
			new Vector2(-0.5f, 0f),
			new Vector2(0f, -1f)
		};
		public static readonly Vector2[] DEFAULT_INPUT = new Vector2[] {
			new Vector2(-1f, 0f),
			new Vector2(-0.5f, 1f),
			new Vector2(0f, 0f)
		};

		public Triangle() : base(DEFAULT_INPUT.ToArray(), DEFAULT_OUTPUT.ToArray()) {
			validator.Validation += () => {
				for (var i = 0; i < output.Length; i++) {
					var v = output[i];
					outputParallelized[i] = new Vector3(v.x, v.y, 1f);
				}
				Notify();
			};
		}

		#region interface
		#region ITriangleComplex
		public override IList<int> Indices {
			get {
				return new int[] { 0, 1, 2 };
			}
		}

		public override IList<Vector4> BarycentricWeights {
			get {
				return new Vector4[] {
					new Vector4(0, 0, 1, 1),
					new Vector4(0, 1, 0, 0),
					new Vector4(1, 0, 0, 0)
				};
			}
		}
		#endregion
		#endregion

		#region member
		#endregion
	}
}
