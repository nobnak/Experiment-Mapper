using M.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Behaviour {

	[ExecuteInEditMode]
	public class PostEffectMapper : MonoBehaviour {

		protected Mapper mapper;

		#region unity
		private void OnEnable() {
			mapper = new Mapper();
		}
		private void OnDisable() {
			if (mapper != null) {
				mapper.Dispose();
				mapper = null;
			}
		}
		private void OnRenderImage(RenderTexture source, RenderTexture destination) {
			mapper.Update(source, destination);
		}
		#endregion
	}
}
