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
	public class Quad : ITriangleComplex {
		public static readonly int[] INDICES = new int[] { 0, 1, 2, 0, 2, 3 };
		public static readonly Vector4[] BARYCENTRIC_WEIGHTS = new Vector4[] {
			new Vector4(0, 0, 1, 1),
			new Vector4(0, 1, 1, 0),
			new Vector4(1, 1, 0, 0),
			new Vector4(1, 0, 0, 1)
		};

		public string name = typeof(Quad).Name;
		public Vector2[] input = new Vector2[]{
			new Vector2(-0.5f, -0.5f),
			new Vector2(-0.5f, 0.5f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0.5f, -0.5f)
		};
		public Vector2[] output = new Vector2[] {
			new Vector2(0f, 0f),
			new Vector2(-0.5f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};

		protected readonly Validator validator = new Validator();
		protected readonly Vector3[] outputParallelized = new Vector3[4];

		protected readonly TextVector2[] inputTexts = new TextVector2[4];
		protected readonly TextVector2[] outputTexts = new TextVector2[4];

		public event System.Action Changed;

		public Quad() {
			validator.Validation += () => {
				if (!output.TryBuildParallelorism(outputParallelized)) {
					Debug.LogWarningFormat("Failed to find w on {0}", this);
					validator.Invalidate();
				} else {
					Notify();
				}
			};

			for (var i = 0; i < inputTexts.Length; i++) {
				var curr = i;
				inputTexts[curr] = new TextVector2(input[curr]);
				outputTexts[curr] = new TextVector2(output[curr]);

				inputTexts[curr].Changed += r => {
					validator.Invalidate();
					input[curr] = inputTexts[curr].Value;
				};
				outputTexts[curr].Changed += r => {
					validator.Invalidate();
					output[curr] = outputTexts[curr].Value;
				};
			}
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
		public IList<Vector2> VertexOutputRaw {
			get {
				validator.Validate();
				return output;
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
		public void GUI() {
			using (new GUILayout.VerticalScope()) {
				GUILayout.Label("Input");
				for (var i = 0; i < inputTexts.Length; i++) {
					var text = inputTexts[i];
					using (new GUILayout.HorizontalScope())
						for (var j = 0; j < 2; j++)
							text[j] = GUILayout.TextField(text[j]);
				}
				GUILayout.Label("Output");
				for (var i = 0; i < outputTexts.Length; i++) {
					var text = outputTexts[i];
					using (new GUILayout.HorizontalScope())
						for (var j = 0; j < 2; j++)
							text[j] = GUILayout.TextField(text[j]);
				}
			}
		}
		public void Invalidate() {
			validator.Invalidate();
		}
		#endregion

		#region member
		protected virtual void Notify() {
			if (Changed != null)
				Changed();
		}
		#endregion
	}
}
