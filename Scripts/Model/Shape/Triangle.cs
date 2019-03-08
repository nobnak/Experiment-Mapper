using M.Base;
using nobnak.Gist;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Model.Shape {
	[System.Serializable]
	public class Triangle : ITriangleComplex {
		public string name = typeof(Triangle).Name;
		public Vector2[] vertexOutput = new Vector2[] {
			new Vector2(-1f, -1f),
			new Vector2(-0.5f, 0f),
			new Vector2(0f, -1f)
		};
		public Vector2[] vertexInput = new Vector2[] {
			new Vector2(-1f, 0f),
			new Vector2(-0.5f, 1f),
			new Vector2(0f, 0f)
		};

		protected Validator validator = new Validator();
		protected readonly Vector3[] vertexOutputParallelized = new Vector3[3];

		public event System.Action Changed;

		public Triangle() {
			validator.Validation += () => {
				for (var i = 0; i < vertexOutput.Length; i++) {
					var v = vertexOutput[i];
					vertexOutputParallelized[i] = new Vector3(v.x, v.y, 1f);
				}
				Notify();
			};
		}

		#region interface
		public IList<Vector3> VertexOutput {
			get {
				validator.Validate();
				return vertexOutputParallelized;
			}
		}
		public IList<Vector2> VertexOutputRaw {
			get {
				validator.Validate();
				return vertexOutput;
			}
		}
		public IList<Vector2> VertexInput {
			get {
				return vertexInput;
			}
		}

		public IList<int> Indices {
			get {
				return new int[] { 0, 1, 2 };
			}
		}

		public IList<Vector4> BarycentricWeights {
			get {
				return new Vector4[] {
					new Vector4(0, 0, 1, 1),
					new Vector4(0, 1, 0, 0),
					new Vector4(1, 0, 0, 0)
				};
			}
		}
		public void GUI() {

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
