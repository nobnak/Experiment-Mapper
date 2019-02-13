using M.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Model.Shape {
	[System.Serializable]
	public class Triangle : ITriangleComplex {
		public string name = typeof(Triangle).Name;
		public Vector3[] vertexOutput = new Vector3[] {
			new Vector3(-1f, -1f, 1f),
			new Vector3(-0.5f, 0f, 1f),
			new Vector3(0f, -1f, 1f)
		};
		public Vector2[] vertexInput = new Vector2[] {
			new Vector2(-1f, 0f),
			new Vector2(-0.5f, 1f),
			new Vector2(0f, 0f)
		};


	public IList<Vector3> VertexOutput {
			get {
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
	}
}
