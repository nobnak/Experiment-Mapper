using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Base {

	public interface ITriangleComplex {
		event System.Action Changed;

		IList<Vector3> VertexOutput { get; }
		IList<Vector2> VertexOutputRaw { get; }
		IList<Vector2> VertexInput { get; }
		IList<int> Indices { get; }
		IList<Vector4> BarycentricWeights { get; }

		void GUI();
		void Invalidate();
	}
}