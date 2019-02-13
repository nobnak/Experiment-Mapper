using nobnak.Gist.GPUBuffer;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Model {

	public class MapperMaterial : System.IDisposable {
		public enum OutputVertexEnum { ___ = 0, OUTPUT_VIN }

		public const string PATH_MATERIAL = "Mapper/Mapper";

		public const string KW_OUTPUT_VIN = "OUTPUT_VIN";

		public static readonly int ID_MAIN_TEX = Shader.PropertyToID("_MainTex");
		public static readonly int ID_VERTEX_OUTPUT = Shader.PropertyToID("voutputs");
		public static readonly int ID_VERTEX_INPUT = Shader.PropertyToID("vinputs");
		public static readonly int ID_INDICES = Shader.PropertyToID("indices");
		public static readonly int ID_BARY_WEIGHTS = Shader.PropertyToID("barys");

		protected Material mat;

		public MapperMaterial() {
			mat = Resources.Load<Material>(PATH_MATERIAL);
		}

		#region interface
		public GPUList<Vector3> VertexOutputs { get; set; }
		public GPUList<Vector2> VertexInputs { get; set; }
		public GPUList<int> Indices { get; set; }
		public GPUList<Vector4> Barys { get; set; }
		public OutputVertexEnum OutputVertex { get; set; }

		public void Blit(RenderTexture src, RenderTexture dst) {
			using (new RenderTextureActivator(dst)) {

				mat.shaderKeywords = null;
				if (OutputVertex != default(OutputVertexEnum))
					mat.EnableKeyword(OutputVertex.ToString());

				mat.SetTexture(ID_MAIN_TEX, src);
				mat.SetBuffer(ID_VERTEX_OUTPUT, VertexOutputs);
				mat.SetBuffer(ID_VERTEX_INPUT, VertexInputs);
				mat.SetBuffer(ID_INDICES, Indices);
				mat.SetBuffer(ID_BARY_WEIGHTS, Barys);
				mat.SetPass(0);
				Graphics.DrawProcedural(MeshTopology.Triangles, Indices.Count);
			}
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
