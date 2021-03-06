using nobnak.Gist.GPUBuffer;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Model {

	public class MapperMaterial : System.IDisposable {
		public enum KwOutputVertexEnum { ___ = 0, OUTPUT_VIN }
		public enum PassEnum {
			Projection = 0,
			AccumEdgeBlend = 1,
			NormalizeEdgeBlend = 2,
			EdgeBlend = 3,
			Frame = 4,
			Grid = 5,
		}

		public const string PATH_MATERIAL = "Mapper/Mapper";

		public const string KW_OUTPUT_VIN = "OUTPUT_VIN";

		public static readonly int ID_MAIN_TEX = Shader.PropertyToID("_MainTex");
		public static readonly int ID_BLEND_TEX = Shader.PropertyToID("_BlendTex");

		public static readonly int ID_VERTEX_OUTPUT = Shader.PropertyToID("voutputs");
		public static readonly int ID_VERTEX_INPUT = Shader.PropertyToID("vinputs");
		public static readonly int ID_INDICES = Shader.PropertyToID("indices");
		public static readonly int ID_BARY_WEIGHTS = Shader.PropertyToID("barys");
		public static readonly int ID_Feature = Shader.PropertyToID("_Feature");

		protected Material mat;

		public MapperMaterial() {
			mat = Resources.Load<Material>(PATH_MATERIAL);
		}

		#region interface
		public Texture BlendTex { get; set; }
		public GPUList<Vector3> VertexOutputs { get; set; }
		public GPUList<Vector2> VertexInputs { get; set; }
		public GPUList<int> Indices { get; set; }
		public GPUList<Vector4> Barys { get; set; }
		public KwOutputVertexEnum OutputVertex { get; set; }
		public PassEnum TargetPass { get; set; }

		public void Blit(RenderTexture src, RenderTexture dst) {
			using (new RenderTextureActivator(dst)) {

				mat.shaderKeywords = null;
				if (OutputVertex != default(KwOutputVertexEnum))
					mat.EnableKeyword(OutputVertex.ToString());

				mat.SetTexture(ID_MAIN_TEX, src);
				SetPass((int)TargetPass);
				Graphics.DrawProcedural(MeshTopology.Triangles, Indices.Count);
			}
		}
		public Material SetPass(int pass) {
			mat.SetTexture(ID_BLEND_TEX, BlendTex);
			mat.SetBuffer(ID_VERTEX_OUTPUT, VertexOutputs);
			mat.SetBuffer(ID_VERTEX_INPUT, VertexInputs);
			mat.SetBuffer(ID_INDICES, Indices);
			mat.SetBuffer(ID_BARY_WEIGHTS, Barys);
			mat.SetPass(pass);
			return mat;
		}
		public Material SetPass(MapperMaterial.PassEnum pass) {
			return SetPass((int)pass);
		}
		#region static
		public static implicit operator Material (MapperMaterial mm) {
			return mm.mat;
		}
		#endregion
		#endregion

		#region IDisposable
		public void Dispose() {
		}
		#endregion
	}
}
