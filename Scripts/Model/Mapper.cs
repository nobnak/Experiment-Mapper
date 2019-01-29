using M.Base;
using M.Model.Shape;
using nobnak.Gist.Extensions.Array;
using nobnak.Gist.GPUBuffer;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace M.Model {

	public class Mapper : System.IDisposable {

		public const string NAMESPACE = "M";

		protected MapperMaterial mat;
		protected GPUList<Vector3> vout = new GPUList<Vector3>();
		protected GPUList<Vector2> vin = new GPUList<Vector2>();
		protected GPUList<int> indices = new GPUList<int>();
		protected GPUList<Vector4> barycentric = new GPUList<Vector4>();

		protected List<ITriangleComplex> triangles = new List<ITriangleComplex>();

		public Mapper() {
			mat = new MapperMaterial();

			triangles.Add(new Quad());

			Rebuild();
		}

		#region interface
		public void Update(RenderTexture src, RenderTexture dst) {
			mat.VertexOutputs = vout;
			mat.VertexInputs = vin;
			mat.Indices = indices;
			mat.Barys = barycentric;
			mat.Blit(src, dst);
		}

		#region IDisposable
		public void Dispose() {
			vout.Dispose();
			vin.Dispose();
			indices.Dispose();
			barycentric.Dispose();

			mat.Dispose();
		}
		#endregion

		#endregion

		private void Rebuild() {
			vout.Clear();
			vin.Clear();
			indices.Clear();
			barycentric.Clear();
			foreach (var t in triangles) {
				var offset = vout.Count;
				vout.AddRange(t.VertexOutput);
				vin.AddRange(t.VertexInput);
				indices.AddRange(t.Indices.Select(i => i + offset));
				barycentric.AddRange(t.BarycentricWeights);
			}
		}
	}
}
