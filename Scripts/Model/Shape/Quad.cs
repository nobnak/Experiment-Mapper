using M.Base;
using nobnak.Gist;
using nobnak.Gist.Extensions.GeometryExt;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace M.Model.Shape {
	[System.Serializable]
	public class Quad : ITriangleComplex {
		public static readonly int[] INDICES = new int[] { 0, 1, 2, 0, 2, 3 };
		public static readonly Vector4[] BARYCENTRIC_WEIGHTS = new Vector4[] {
			new Vector4(0, 0, 1, 1),
			new Vector4(0, 1, 1, 0),
			new Vector4(1, 1, 0, 0),
			new Vector4(1, 0, 0, 1)
		};

		public string name = typeof(Quad).Name;

		[SerializeField]
		protected readonly Vector2[] input = new Vector2[4];
		[SerializeField]
		protected readonly Vector2[] output = new Vector2[4];

		protected readonly Validator validator = new Validator();
		protected readonly Vector3[] outputParallelized = new Vector3[4];

		public Quad() {
			input[0] = new Vector2(-0.5f, -0.5f);
			input[1] = new Vector2(-0.5f, 0.5f);
			input[2] = new Vector2(0.5f, 0.5f);
			input[3] = new Vector2(0.5f, -0.5f);

			output[0] = new Vector2(0f, 0f);
			output[1] = new Vector2(-0.5f, 1f);
			output[2] = new Vector2(1f, 1f);
			output[3] = new Vector2(1f, 0f);

			validator.Validation += () => {
				if (!output.TryBuildParallelorism(outputParallelized)) {
					Debug.LogWarningFormat("Failed to find w on {0}", this);
					validator.Invalidate();
				}
			};
		}

		#region interface
		public override string ToString() {
			return string.Format("<{0}:\ninput={1}\noutput={2}\nparallel={3}>",
				typeof(Quad).Name,
				string.Join(",", input.Select(v => v.ToString()).ToArray()),
				string.Join(",", output.Select(v => v.ToString()).ToArray()),
				string.Join(",", outputParallelized.Select(v => v.ToString()).ToArray()));
		}
		public IList<Vector3> VertexOutput {
			get {
				validator.Validate();
				return outputParallelized;
			}
		}
		public IList<Vector2> VertexInput {
			get {
				validator.Validate();
				return input;
			}
		}
		public IList<int> Indices {
			get { return INDICES; }
		}
		public IList<Vector4> BarycentricWeights {
			get { return BARYCENTRIC_WEIGHTS; }
		}
		#endregion
	}
}
