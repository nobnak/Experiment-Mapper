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

	public class Mapper : System.IDisposable, IList<ITriangleComplex> {
		public event System.Action<RenderTexture, RenderTexture, Flags> AfterOnUpdate;
		[System.Flags]
		public enum Flags { None = 0, Output_InputVertex = 1 }

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
		public void Update(RenderTexture src, RenderTexture dst, Flags flags = 0) {
			validator.Validate();
			SetFlags(flags);
			mat.VertexOutputs = vout;
			mat.VertexInputs = vin;
			mat.Indices = indices;
			mat.Barys = barycentric;
			mat.Blit(src, dst);

			if (AfterOnUpdate != null)
				AfterOnUpdate(src, dst, flags);
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
		public ITriangleComplex this[int index] {
			get {
				validator.Validate();
				return triangles[index];
			}
			set {
				validator.Invalidate();
				triangles[index] = value;
			}
		}
		public int IndexOf(ITriangleComplex item) {
			validator.Validate();
			return triangles.IndexOf(item);
		}

		public void Insert(int index, ITriangleComplex item) {
			validator.Invalidate();
			triangles.Insert(index, item);
		}

		public void RemoveAt(int index) {
			validator.Invalidate();
			triangles.RemoveAt(index);
		}
		public IEnumerator<ITriangleComplex> GetEnumerator() {
			return triangles.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			throw new System.NotImplementedException();
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
		}
		private void SetFlags(Flags flags) {
			mat.OutputVertex = ((flags & Flags.Output_InputVertex) != 0)
				? MapperMaterial.OutputVertexEnum.OUTPUT_VIN
				: default(MapperMaterial.OutputVertexEnum);
		}
		#endregion
	}
}
