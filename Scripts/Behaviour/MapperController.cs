using M.Model;
using M.Model.Shape;
using nobnak.Gist;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Behaviour {

	[ExecuteInEditMode]
	public class MapperController : MonoBehaviour {
		public Events events = new Events();
		public Shapes shapes = new Shapes();

		protected Mapper mapper;
		protected Validator validator = new Validator();

		#region member
		protected void SetMapper(Mapper mapper) {
			this.mapper = mapper;
			events.MapperOnChanged.Invoke(mapper);
		}
		#endregion

		#region unity
		private void OnEnable() {
			SetMapper(new Mapper());

			validator.Reset();
			validator.Validation += () => {
				if (mapper == null) {
					validator.Invalidate();
					return;
				}
				mapper.Clear();
				foreach (var t in shapes.triangles)
					mapper.Add(t);
				foreach (var q in shapes.quads)
					mapper.Add(q);

			};
		}
		private void OnDisable() {
			if (mapper != null) {
				mapper.Dispose();
				SetMapper(null);
			}
		}
		private void OnValidate() {
			validator.Invalidate();
		}
		private void Update() {
			validator.Validate();
		}
		#endregion

		#region classes
		[System.Serializable]
		public class MapperEvent : UnityEngine.Events.UnityEvent<Mapper> { }
		[System.Serializable]
		public class Events {
			public MapperEvent MapperOnChanged = new MapperEvent();
		}
		[System.Serializable]
		public class Shapes {
			public Triangle[] triangles = new Triangle[0];
			public Quad[] quads = new Quad[0];
		}
		#endregion
	}
}
