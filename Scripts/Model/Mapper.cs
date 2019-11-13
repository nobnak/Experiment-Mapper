using M.Model.Shape;
using nobnak.Gist;
using nobnak.Gist.Extensions.Array;
using nobnak.Gist.GPUBuffer;
using nobnak.Gist.ObjectExt;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace M.Model {

	public class Mapper : System.IDisposable, IList<BaseTriangleComplex> {
		public event System.Action<RenderTexture, RenderTexture, OutputFlags> OnRender;
		public event System.Action<RenderTexture> OnBlendTexCreated;

		[System.Flags]
		public enum OutputFlags {
			None = 0,
			InputVertex = 1 << 0,
			EdgeBlend = 1 << 2,
			WireFrame = 1 << 3,
		}

		public const string NAMESPACE = "M";

		protected MapperMaterial mat;
		protected GPUList<Vector3> vout = new GPUList<Vector3>();
		protected GPUList<Vector2> vin = new GPUList<Vector2>();
		protected GPUList<int> indices = new GPUList<int>();
		protected GPUList<Vector4> barycentric = new GPUList<Vector4>();

		protected Validator validator = new Validator();
		protected List<BaseTriangleComplex> triangles = new List<BaseTriangleComplex>();

		protected RenderTexture blendTex;

		public Mapper() {
			mat = new MapperMaterial();

			validator.Validation += () => {
				Rebuild();
			};
		}

		#region interface
		public Validator Validator { get { return validator; } }
		public RenderTexture BlendTex { get { return BlendTex; } }
		public void Update(RenderTexture src, RenderTexture dst, Color clearColor = default(Color)) {
			validator.Validate();
			mat.VertexOutputs = vout;
			mat.VertexInputs = vin;
			mat.Indices = indices;
			mat.Barys = barycentric;

			if (blendTex == null || blendTex.width != src.width || blendTex.height != src.height) {
				ReleaseBlendTex();
				blendTex = new RenderTexture(src.width, src.height, 0, 
					RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				blendTex.antiAliasing = QualitySettings.antiAliasing;
				blendTex.autoGenerateMips = false;
				blendTex.useMipMap = false;
				blendTex.wrapMode = TextureWrapMode.Clamp;

				var accumBlendWeightTex = RenderTexture.GetTemporary(blendTex.descriptor);
				using (new RenderTextureActivator(accumBlendWeightTex)) {
					GL.Clear(true, true, Color.clear);
					mat.TargetPass = MapperMaterial.PassEnum.AccumEdgeBlend;
					mat.OutputVertex = MapperMaterial.KwOutputVertexEnum.OUTPUT_VIN;
					mat.Blit(null, accumBlendWeightTex);
				}
				using (new RenderTextureActivator(blendTex)) {
					GL.Clear(true, true, Color.clear);
					mat.TargetPass = MapperMaterial.PassEnum.NormalizeEdgeBlend;
					mat.OutputVertex = default(MapperMaterial.KwOutputVertexEnum);
					mat.Blit(accumBlendWeightTex, blendTex);
				}
				RenderTexture.ReleaseTemporary(accumBlendWeightTex);
				mat.BlendTex = blendTex;
				NotiryOnBlendTexCreated();
			}

			using (new RenderTextureActivator(dst))
				GL.Clear(true, true, clearColor);

			if ((CurrFlags & OutputFlags.InputVertex) != 0)
				Graphics.Blit(src, dst);

			mat.TargetPass = MapperMaterial.PassEnum.Projection;
			SetOutputVertex(CurrFlags);
			mat.Blit(src, dst);

			if ((CurrFlags & OutputFlags.EdgeBlend) != 0) {
				var pass = MapperMaterial.PassEnum.EdgeBlend;
				Graphics.Blit(null, dst, mat.SetPass(pass), (int)pass);
			}

			if ((CurrFlags & OutputFlags.WireFrame) != 0) {
				mat.TargetPass = MapperMaterial.PassEnum.Wireframe;
				mat.Blit(src, dst);
			}

			NotifyPostRender(src, dst);
		}

#region ICollection<ITriangleComplex>
		public int Count {
			get { return triangles.Count; }
		}
		public bool IsReadOnly { get { return false; } }
		public OutputFlags CurrFlags { get; set; }

		public void Add(BaseTriangleComplex item) {
			validator.Invalidate();
			triangles.Add(item);
			item.Changed += ListenChanged;
		}
		public void Clear() {
			foreach (var item in triangles.ToArray())
				Remove(item);
		}
		public bool Contains(BaseTriangleComplex item) {
			return triangles.Contains(item);
		}
		public void CopyTo(BaseTriangleComplex[] array, int arrayIndex) {
			triangles.CopyTo(array, arrayIndex);
		}
		public bool Remove(BaseTriangleComplex item) {
			validator.Invalidate();
			item.Changed -= ListenChanged;
			return triangles.Remove(item);
		}
		public BaseTriangleComplex this[int index] {
			get {
				validator.Validate();
				return triangles[index];
			}
			set {
				validator.Invalidate();
				triangles[index] = value;
			}
		}
		public int IndexOf(BaseTriangleComplex item) {
			validator.Validate();
			return triangles.IndexOf(item);
		}

		public void Insert(int index, BaseTriangleComplex item) {
			validator.Invalidate();
			triangles.Insert(index, item);
			item.Changed += ListenChanged;
		}

		public void RemoveAt(int index) {
			validator.Invalidate();
			var item = triangles[index];
			if (item != null) {
				item.Changed -= ListenChanged;
				triangles.RemoveAt(index);
			}
		}
		public IEnumerator<BaseTriangleComplex> GetEnumerator() {
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

			ReleaseBlendTex();
			mat.Dispose();
		}

#endregion

#endregion

#region member
		protected virtual void Rebuild() {
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

			ReleaseBlendTex();
		}
		protected virtual void SetOutputVertex(OutputFlags flags) {
			mat.OutputVertex = ((flags & OutputFlags.InputVertex) != 0)
				? MapperMaterial.KwOutputVertexEnum.OUTPUT_VIN
				: default(MapperMaterial.KwOutputVertexEnum);
		}
		protected virtual void ListenChanged() {
			validator.Invalidate();
		}
		private void ReleaseBlendTex() {
			blendTex.DestroySelf();
			blendTex = null;
		}
		private void NotifyPostRender(RenderTexture src, RenderTexture dst) {
			if (OnRender != null)
				OnRender(src, dst, CurrFlags);
		}
		private void NotiryOnBlendTexCreated() {
			if (OnBlendTexCreated != null)
				OnBlendTexCreated(blendTex);
		}
#endregion
	}
}
