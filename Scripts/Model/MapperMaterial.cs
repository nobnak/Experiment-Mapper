using nobnak.Gist.GPUBuffer;
using nobnak.Gist.Scoped;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Model {

	public class MapperMaterial : System.IDisposable {

		public const string PATH_MATERIAL = "Mapper/Mapper";

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
		public ComputeBuffer VertexOutputs { get; set; }
		public ComputeBuffer VertexInputs { get; set; }
		public ComputeBuffer Indices { get; set; }
		public ComputeBuffer Barys { get; set; }

		public void Blit(RenderTexture src, RenderTexture dst) {
			using (new RenderTextureActivator(dst)) {
				mat.SetTexture(ID_MAIN_TEX, src);
				mat.SetBuffer(ID_VERTEX_OUTPUT, VertexOutputs);
				mat.SetBuffer(ID_VERTEX_INPUT, VertexInputs);
				mat.SetBuffer(ID_INDICES, Indices);
				mat.SetBuffer(ID_BARY_WEIGHTS, Barys);
				mat.SetPass(0);
				Graphics.DrawProcedural(MeshTopology.Triangles, Indices.count);
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
