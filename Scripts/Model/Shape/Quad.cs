using M.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Model.Shape {

	public class Quad : ITriangleComplex {
		public IList<Vector3> VertexOutput {
			get {
				return new Vector3[] {
					new Vector3(0f, 0f, 1f),
					new Vector3(0f, 1f, 1f),
					new Vector3(1f, 1f, 1f),
					new Vector3(1f, 0f, 1f)
				};
			}
		}

		public IList<Vector2> VertexInput {
			get {
				return new Vector2[] {
					new Vector2(-0.5f, -0.5f),
					new Vector2(-0.5f, 0.5f),
					new Vector2(0.5f, 0.5f),
					new Vector2(0.5f, -0.5f)
				};
			}
		}

		public IList<int> Indices {
			get {
				return new int[] { 0, 1, 2, 0, 2, 3 };
			}
		}

		public IList<Vector4> BarycentricWeights {
			get {
				return new Vector4[] {
					new Vector4(0, 0, 1, 1),
					new Vector4(0, 1, 1, 0),
					new Vector4(1, 1, 0, 0),
					new Vector4(1, 0, 0, 1)
				};
			}
		}
	}
}
