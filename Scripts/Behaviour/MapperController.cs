using M.Model;
using M.Model.Shape;
using nobnak.Gist;
using nobnak.Gist.Scoped;
using nobnak.Gist.StateMachine;
using System.Linq;
using UnityEngine;

namespace M.Behaviour {

	[ExecuteInEditMode]
	public class MapperController : MonoBehaviour {
		public Events events = new Events();
		public Shapes shapes = new Shapes();

		[SerializeField]
		protected Data data = new Data();

		protected GLFigure fig;
		protected Mapper mapper;
		protected Validator validator = new Validator();
		protected FSM<ShapeSelectionState> fsmShapeSelection;

		#region member
		protected void SetMapper(Mapper mapper) {
			this.mapper = mapper;
			events.MapperOnChanged.Invoke(mapper);
			if (mapper != null) {
				mapper.AfterOnUpdate += (src, dst, flags) => {
					if (0 <= data.selectedShape && data.selectedShape < mapper.Count) {
						GL.PushMatrix();
						GL.LoadIdentity();
						GL.LoadProjectionMatrix(Matrix4x4.identity);
						fig.CurrentColor = Color.cyan;
						using (new RenderTextureActivator(dst)) {
							var shape = mapper[data.selectedShape];
							var vertices = ((flags & Mapper.Flags.Output_InputVertex) != 0)
								? shape.VertexInput.Select(v => (Vector3)v) 
								: shape.VertexOutput;
							foreach (var v in vertices) {
								fig.DrawCircle(
									Matrix4x4.TRS(v, Quaternion.identity, 0.1f * Vector3.one));
							}
						}
						GL.PopMatrix();
					}
				};
			}
		}
		#endregion

		#region unity
		private void OnEnable() {
			fsmShapeSelection = new FSM<ShapeSelectionState>(FSM.TransitionModeEnum.Immediate);
			fig = new GLFigure();

			SetMapper(new Mapper());

			fig.DefaultLineMat.ZTestMode = GLMaterial.ZTestEnum.ALWAYS;

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

			fsmShapeSelection.StateFor(ShapeSelectionState.None).Update(f => {
				if (0 <= data.selectedShape && data.selectedShape < shapes.Count) {
					f.Goto(ShapeSelectionState.Selected);
					return;
				}
			});
			fsmShapeSelection.StateFor(ShapeSelectionState.Selected).Update(f => {
				if (data.selectedShape < 0 || shapes.Count <= data.selectedShape) {
					f.Goto(ShapeSelectionState.None);
					return;
				}
			});
			fsmShapeSelection.Init(ShapeSelectionState.None);
		}
		private void OnDisable() {
			if (mapper != null) {
				mapper.Dispose();
				SetMapper(null);
			}
			if (fig != null) {
				fig.Dispose();
				fig = null;
			}
		}
		private void OnValidate() {
			validator.Invalidate();
		}
		private void Update() {
			validator.Validate();
			fsmShapeSelection.Update();
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

			public int Count { get { return triangles.Length + quads.Length; } }
		}
		public enum ShapeSelectionState { None = 0, Selected }
		[System.Serializable]
		public class Data {
			public int selectedShape = -1;
		}
		#endregion
	}
}
