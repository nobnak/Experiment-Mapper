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
	public abstract class BaseTriangleComplex : ITriangleComplex {
		public event System.Action Changed;

		public Vector2[] input;
		public Vector2[] output;

		protected readonly Validator validator = new Validator();
		protected readonly Vector3[] outputParallelized;

		protected readonly TextVector2[] inputTexts;
		protected readonly TextVector2[] outputTexts;

		public BaseTriangleComplex(Vector2[] input, Vector2[] output) {
			this.input = input;
			this.output = output;

			outputParallelized = new Vector3[output.Length];

			inputTexts = new TextVector2[input.Length];
			outputTexts = new TextVector2[output.Length];
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
				this.GetType().Name,
				string.Join(",", input.Select(v => v.ToString()).ToArray()),
				string.Join(",", output.Select(v => v.ToString()).ToArray()),
				string.Join(",", outputParallelized.Select(v => v.ToString()).ToArray()));
		}

		#region ITriangleComplex
		public abstract IList<int> Indices { get; }
		public abstract IList<Vector4> BarycentricWeights { get; }

		public virtual IList<Vector3> VertexOutput {
			get {
				validator.Validate();
				return outputParallelized;
			}
		}
		public virtual IList<Vector2> VertexOutputRaw {
			get {
				validator.Validate();
				return output;
			}
		}
		public virtual IList<Vector2> VertexInput {
			get {
				validator.Validate();
				return input;
			}
		}

		public virtual void GUI() {
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
		public virtual void Invalidate() {
			validator.Invalidate();
		}
		#endregion
		#endregion

		#region member
		protected virtual void Notify() {
			if (Changed != null)
				Changed();
		}
		#endregion
	}
}
