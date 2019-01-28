using nobnak.Gist.GPUBuffer;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Model {

	public class Mapper : System.IDisposable {

		public const string NAMESPACE = "M";

		protected MapperMaterial mat;
		protected GPUList<Vector3> vout = new GPUList<Vector3>();
		protected GPUList<Vector2> vin = new GPUList<Vector2>();
		protected GPUList<int> indices = new GPUList<int>();
		protected GPUList<Vector4> barycentric = new GPUList<Vector4>();

		public Mapper() {
			mat = new MapperMaterial();

			vout.Add(new Vector3(0f, 0f, 1f));
			vout.Add(new Vector3(0f, 1f, 1f));
			vout.Add(new Vector3(1f, 1f, 1f));
			vout.Add(new Vector3(1f, 0f, 1f));

			vin.Add(new Vector2(-0.5f, -0.5f));
			vin.Add(new Vector2(-0.5f, 0.5f));
			vin.Add(new Vector2(0.5f, 0.5f));
			vin.Add(new Vector2(0.5f, -0.5f));

			indices.Add(0);
			indices.Add(1);
			indices.Add(2);
			indices.Add(0);
			indices.Add(2);
			indices.Add(3);

			barycentric.Add(new Vector4(0, 0, 1, 1));
			barycentric.Add(new Vector4(0, 1, 1, 0));
			barycentric.Add(new Vector4(1, 1, 0, 0));
			barycentric.Add(new Vector4(1, 0, 0, 1));
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
	}
}
