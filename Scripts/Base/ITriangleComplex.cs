using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Base {

	public interface ITriangleComplex {

		IList<Vector3> VertexOutput { get; }
		IList<Vector2> VertexInput { get; }
		IList<int> Indices { get; }
		IList<Vector4> BarycentricWeights { get; }
	}
}