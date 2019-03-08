using M.Base;
using M.Converters;
using M.Model;
using M.Model.Shape;
using nobnak.Gist;
using nobnak.Gist.DataUI;
using nobnak.Gist.IMGUI.Scope;
using nobnak.Gist.InputDevice;
using nobnak.Gist.Scoped;
using nobnak.Gist.StateMachine;
using System.Linq;
using UnityEngine;

namespace M.Behaviour {

	[ExecuteInEditMode]
	public class MapperController : MonoBehaviour {
		public Events events = new Events();

		[SerializeField]
		protected Data data = new Data();

		protected GLFigure fig;
		protected Mapper mapper;
		protected Validator validator = new Validator();
		protected FSM<ShapeSelectionState> fsmShapeSelection;

		protected MouseTracker mouse = new MouseTracker();
		protected Rect guirect = new Rect(10, 10, 200f, 300f);
		protected TextInt guiSelectedShape;
		protected TextInt guiSelectedVertices;

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
								? shape.VertexInput : shape.VertexOutputRaw;
							var width = (float)(dst != null ? dst.width : Screen.width);
							var height = (float)(dst != null ? dst.height : Screen.height);
							var aspect = width / height;
							var unit = 20f / width;
							var size = new Vector3(unit, unit * aspect, 1f);
							for (var i = 0; i < vertices.Count; i++) {
								var v = vertices[i];
								var selectedVertex = (data.selectedVertices & 1 << i) != 0;
								if (selectedVertex)
									fig.FillCircle(Matrix4x4.TRS(v, Quaternion.identity, size));
								else
									fig.DrawCircle(Matrix4x4.TRS(v, Quaternion.identity, size));
							}
						}
						GL.PopMatrix();
					}
				};
			}
		}
		protected void Window(int id) {
			var shapeSelected = 0 <= data.selectedShape && data.selectedShape < mapper.Count;

			using (new GUIChangedScope(() => { })) {
				using (new GUILayout.VerticalScope()) {
					guiSelectedShape.StrValue =
						GUILayout.TextField(guiSelectedShape.StrValue);
					guiSelectedVertices.StrValue =
						GUILayout.TextField(guiSelectedVertices.StrValue);
					var shape = GetSelectedShape();
					if (shape != null)
						shape.GUI();
				}
			}

			GUI.DragWindow();
		}

		private ITriangleComplex GetSelectedShape() {
			return (0 <= data.selectedShape && data.selectedShape < mapper.Count) ?
				mapper[data.selectedShape] : null;
		}
		#endregion

		#region unity
		private void OnEnable() {
			fsmShapeSelection = new FSM<ShapeSelectionState>(FSM.TransitionModeEnum.Immediate);
			fig = new GLFigure();
			guiSelectedShape = new TextInt(data.selectedShape);
			guiSelectedVertices = new TextInt(data.selectedVertices);

			SetMapper(new Mapper());

			fig.DefaultLineMat.ZTestMode = GLMaterial.ZTestEnum.ALWAYS;
			guiSelectedShape.Changed += r => {
				validator.Invalidate();
				data.selectedShape = r.Value;
			};
			guiSelectedVertices.Changed += r => {
				validator.Invalidate();
				data.selectedVertices = r.Value;
			};

			validator.Reset();
			validator.Validation += () => {
				if (mapper == null) {
					validator.Invalidate();
					return;
				}
				mapper.Clear();
				foreach (var t in data.shapes.triangles)
					mapper.Add(t);
				foreach (var q in data.shapes.quads)
					mapper.Add(q);
			};

			fsmShapeSelection.StateFor(ShapeSelectionState.None).Update(f => {
				if (0 <= data.selectedShape && data.selectedShape < data.shapes.Count) {
					f.Goto(ShapeSelectionState.Selected);
					return;
				}
			});
			fsmShapeSelection.StateFor(ShapeSelectionState.Selected).Update(f => {
				if (data.selectedShape < 0 || data.shapes.Count <= data.selectedShape) {
					f.Goto(ShapeSelectionState.None);
					return;
				}
			});
			fsmShapeSelection.Init(ShapeSelectionState.None);

			mouse.OnSelection += (m, arg) => {
				if ((arg & MouseTracker.ButtonFlag.Left) == 0)
					return;

				var s2n = CoordConverter.ScreenToNDC;
				var dx = (Vector2)s2n.MultiplyVector(m.Positiondiff);

				var shape = GetSelectedShape();
				if (shape != null) {
					for (var i = 0; i < shape.VertexOutput.Count; i++) {
						if ((data.selectedVertices & (1 << i)) == 0)
							continue;
						var v = shape.VertexOutputRaw[i];
						v.x += dx.x;
						v.y += dx.y;
						shape.VertexOutputRaw[i] = v;
					}
					shape.Invalidate();
				}
			};
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
			mouse.Update();
		}
		private void OnGUI() {
			guirect = GUILayout.Window(GetInstanceID(), guirect, Window, "Mapper");
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
			public int selectedVertices = 0;
			public Shapes shapes = new Shapes();
		}
		#endregion
	}
}
