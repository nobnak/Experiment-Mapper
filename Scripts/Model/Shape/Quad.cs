using M.Base;
using M.UIElement;
using nobnak.Gist;
using nobnak.Gist.Extensions.GeometryExt;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace M.Model.Shape {
	[System.Serializable]
	public class Quad : BaseTriangleComplex {
		public static readonly int[] INDICES = new int[] { 0, 1, 2, 0, 2, 3 };
		public static readonly Vector4[] BARYCENTRIC_WEIGHTS = new Vector4[] {
			new Vector4(0, 0, 1, 1),
			new Vector4(0, 1, 1, 0),
			new Vector4(1, 1, 0, 0),
			new Vector4(1, 0, 0, 1)
		};
		public static readonly Vector2[] DEFAULT_INPUT = new Vector2[]{
			new Vector2(-0.5f, -0.5f),
			new Vector2(-0.5f, 0.5f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, -0.5f)
		};
		public static readonly Vector2[] DEFAULT_OUTPUT = new Vector2[] {
			new Vector2(0f, 0f),
			new Vector2(-0.5f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};

		public Quad() : base(DEFAULT_INPUT.ToArray(), DEFAULT_OUTPUT.ToArray()) {
			validator.Validation += () => {
				if (!output.TryBuildParallelorism(outputParallelized)) {
					Debug.LogWarningFormat("Failed to find w on {0}", this);
					validator.Invalidate();
				} else {
					Notify();
				}
			};
		}

		#region interface
		#region ITriangleComplex
		public override IList<int> Indices {
			get { return INDICES; }
		}
		public override IList<Vector4> BarycentricWeights {
			get { return BARYCENTRIC_WEIGHTS; }
		}
		#endregion
		#endregion

		#region member
		#endregion
	}
}
