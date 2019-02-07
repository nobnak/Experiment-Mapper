using M.Base;
using M.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Behaviour {

	[ExecuteInEditMode]
	public class PostEffectMapperView : MonoBehaviour, IMapperView {
		public Settings settings = new Settings();

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
			mapper.Update(source, destination, settings.flags);
		}
		#endregion

		#region classes
		[System.Serializable]
		public class Settings {
			public Mapper.Flags flags = 0;
		}
		#endregion
	}
}
