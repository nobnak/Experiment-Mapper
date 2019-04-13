using M.Base;
using M.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Behaviour {

	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class PostEffectMapperView : MonoBehaviour, IMapperView {
		protected Mapper mapper;

		#region interface
		#region IMapperView
		public Mapper Model {
			get { return mapper; }
			set { mapper = value; }
		}
		#endregion
		#endregion

		#region unity
		private void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (mapper == null) {
				Graphics.Blit(source, destination);
				return;
			}
			mapper.Update(source, destination, Camera.current.backgroundColor);
		}
		#endregion
	}
}
