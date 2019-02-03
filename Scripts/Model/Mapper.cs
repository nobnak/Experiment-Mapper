using M.Base;
using M.Model.Shape;
using nobnak.Gist;
using nobnak.Gist.Extensions.Array;
using nobnak.Gist.GPUBuffer;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace M.Model {

	public class Mapper : System.IDisposable, ICollection<ITriangleComplex> {

		public const string NAMESPACE = "M";

		protected MapperMaterial mat;
		protected GPUList<Vector3> vout = new GPUList<Vector3>();
		protected GPUList<Vector2> vin = new GPUList<Vector2>();
		protected GPUList<int> indices = new GPUList<int>();
		protected GPUList<Vector4> barycentric = new GPUList<Vector4>();

		protected Validator validator = new Validator();
		protected List<ITriangleComplex> triangles = new List<ITriangleComplex>();

		public Mapper() {
			mat = new MapperMaterial();

			validator.Validation += () => {
				Rebuild();
			};
		}

		#region interface
		public void Update(RenderTexture src, RenderTexture dst) {
			validator.Validate();
			mat.VertexOutputs = vout;
			mat.VertexInputs = vin;
			mat.Indices = indices;
			mat.Barys = barycentric;
			mat.Blit(src, dst);
		}

		#region ICollection<ITriangleComplex>
		public int Count {
			get { return triangles.Count; }
		}
		public bool IsReadOnly { get { return false; } }
		public void Add(ITriangleComplex item) {
			validator.Invalidate();
			triangles.Add(item);
		}
		public void Clear() {
			validator.Invalidate();
			triangles.Clear();
		}
		public bool Contains(ITriangleComplex item) {
			return triangles.Contains(item);
		}
		public void CopyTo(ITriangleComplex[] array, int arrayIndex) {
			triangles.CopyTo(array, arrayIndex);
		}
		public bool Remove(ITriangleComplex item) {
			validator.Invalidate();
			return triangles.Remove(item);
		}
		public IEnumerator<ITriangleComplex> GetEnumerator() {
			return triangles.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return triangles.GetEnumerator();
		}
		#endregion

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

		#region member
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

			Debug.LogFormat(
				"Mapper.Rebuild:vout={0},vin={1},indices={2},bary={3}",
				vout.Count, vin.Count, indices.Count, barycentric.Count);
		}

		#endregion
	}
}
