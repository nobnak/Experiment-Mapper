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
		[SerializeField]
		protected GUIData gui = new GUIData();

		protected GLFigure fig;
		protected Mapper mapper;
		protected Validator validator = new Validator();
		protected FSM<ShapeSelectionState> fsmShapeSelection;

		protected MouseTracker mouse = new MouseTracker();
		protected Rect guirect = new Rect(10, 10, 300f, 300f);
		protected TextInt guiSelectedShape;
		protected TextInt guiSelectedVertices;
		protected Reactive<VisTargetEnum> rctVisTarget;

		#region member
		protected void SetMapper(Mapper mapper) {
			this.mapper = mapper;
			events.MapperOnChanged.Invoke(mapper);
			if (mapper != null) {
				mapper.AfterOnUpdate += (src, dst, flags) => {
					if (gui.toggle && 0 <= gui.selectedShape && gui.selectedShape < mapper.Count) {
						GL.PushMatrix();
						GL.LoadIdentity();
						GL.LoadProjectionMatrix(Matrix4x4.identity);
						fig.CurrentColor = Color.cyan;
						using (new RenderTextureActivator(dst)) {
							var shape = mapper[gui.selectedShape];
							var vertices = GetVertices(shape);
							var width = (float)(dst != null ? dst.width : Screen.width);
							var height = (float)(dst != null ? dst.height : Screen.height);
							var aspect = width / height;
							var unit = 20f / width;
							var size = new Vector3(unit, unit * aspect, 1f);
							for (var i = 0; i < vertices.Count; i++) {
								var v = vertices[i];
								var selectedVertex = (gui.selectedVertices & 1 << i) != 0;
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

		protected System.Collections.Generic.IList<Vector2> GetVertices(ITriangleComplex shape) {
			return rctVisTarget.Value == VisTargetEnum.Input ? shape.VertexInput : shape.VertexOutputRaw;
		}

		protected void Window(int id) {
			var shapeSelected = 0 <= gui.selectedShape && gui.selectedShape < mapper.Count;

			using (new GUIChangedScope(() => { })) {
				var shape = GetSelectedShape();

				using (new GUILayout.VerticalScope()) {
					using (new GUILayout.HorizontalScope()) {
						GUILayout.Label("Edit target");
						rctVisTarget.Value = VIS_TARGET_ENUM_VALUES[GUILayout.SelectionGrid(
							(int)rctVisTarget.Value,
							VIS_TARGET_ENUM_NAMES, VIS_TARGET_ENUM_NAMES.Length)];
					}

					using (new GUILayout.HorizontalScope()) {
						GUILayout.Label("Visual");
						var flags = mapper.CurrFeature;
						var fuv = (flags & MapperMaterial.FeatureEnum.UV) != 0;
						var fgrid = (flags & MapperMaterial.FeatureEnum.WIREFRAME) != 0;
						fuv = GUILayout.Toggle(fuv, "UV");
						fgrid = GUILayout.Toggle(fgrid, "Grid");
						flags = (flags & ~(
							MapperMaterial.FeatureEnum.IMAGE
							| MapperMaterial.FeatureEnum.UV
							| MapperMaterial.FeatureEnum.WIREFRAME))
							| (fuv ? MapperMaterial.FeatureEnum.UV : MapperMaterial.FeatureEnum.IMAGE)
							| (fgrid ? MapperMaterial.FeatureEnum.WIREFRAME : 0);
						Debug.LogFormat("Feature : {0}", flags);
						mapper.CurrFeature = flags; // (MapperMaterial.FeatureEnum)9;
					}

					using (new GUILayout.HorizontalScope()) {
						GUILayout.Label("Selected shape");
						if (GUILayout.Button("<"))
							guiSelectedShape.Value--;
						if (GUILayout.Button(">"))
							guiSelectedShape.Value++;
					}
					guiSelectedShape.StrValue = GUILayout.TextField(guiSelectedShape.StrValue);

					using (new GUILayout.HorizontalScope()) {
						GUILayout.Label("Selected vertices");
						if (GUILayout.Button("Clear"))
							guiSelectedVertices.Value = 0;
						if (GUILayout.Button("All"))
							guiSelectedVertices.Value = -1;
					}
					using (new GUILayout.HorizontalScope()) {
						var flags = guiSelectedVertices.Value;
						if (shape != null) {
							for (var i = 0; i < shape.VertexOutputRaw.Count; i++) {
								var bit = 1 << i;
								var enabled = GUILayout.Toggle((flags & bit) != 0, string.Format("v{0}", i), GUILayout.ExpandWidth(false));
								flags = (flags & ~bit) | (enabled ? bit : 0);
							}
						}
						guiSelectedVertices.Value = flags;
						guiSelectedVertices.StrValue = GUILayout.TextField(guiSelectedVertices.StrValue);
					}
					if (shape != null)
						shape.GUI();
				}
			}

			GUI.DragWindow();
		}

		private ITriangleComplex GetSelectedShape() {
			return (0 <= gui.selectedShape && gui.selectedShape < mapper.Count) ?
				mapper[gui.selectedShape] : null;
		}
		#endregion

		#region unity
		private void OnEnable() {
			fsmShapeSelection = new FSM<ShapeSelectionState>(FSM.TransitionModeEnum.Immediate);
			fig = new GLFigure();
			guiSelectedShape = new TextInt(gui.selectedShape);
			guiSelectedVertices = new TextInt(gui.selectedVertices);
			rctVisTarget = new Reactive<VisTargetEnum>();

			SetMapper(new Mapper());

			fig.DefaultLineMat.ZTestMode = GLMaterial.ZTestEnum.ALWAYS;
			guiSelectedShape.Changed += r => {
				validator.Invalidate();
				gui.selectedShape = r.Value;
			};
			guiSelectedVertices.Changed += r => {
				validator.Invalidate();
				gui.selectedVertices = r.Value;
			};
			rctVisTarget.Changed += r => {
				mapper.CurrFlags = r.Value == VisTargetEnum.Input ?
					Mapper.FlagOutputVertex.Output_InputVertex | Mapper.FlagOutputVertex.Output_SrcImage :
					Mapper.FlagOutputVertex.None;
			};

			validator.Reset();
			validator.Validation += () => {
				if (mapper == null) {
					validator.Invalidate();
					return;
				}
				mapper.Clear();
				foreach (var q in data.shapes.quads)
					mapper.Add(q);
			};

			fsmShapeSelection.StateFor(ShapeSelectionState.None).Update(f => {
				if (0 <= gui.selectedShape && gui.selectedShape < data.shapes.Count) {
					f.Goto(ShapeSelectionState.Selected);
					return;
				}
			});
			fsmShapeSelection.StateFor(ShapeSelectionState.Selected).Update(f => {
				if (gui.selectedShape < 0 || data.shapes.Count <= gui.selectedShape) {
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
					var vertices = GetVertices(shape);
					for (var i = 0; i < vertices.Count; i++) {
						if ((gui.selectedVertices & (1 << i)) == 0)
							continue;
						var v = vertices[i];
						v.x += dx.x;
						v.y += dx.y;
						vertices[i] = v;
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
			gui.toggle.Update();
		}
		private void OnGUI() {
			if (gui.toggle)
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
			public Quad[] quads = new Quad[0];

			public int Count { get { return quads.Length; } }
		}
		public enum ShapeSelectionState { None = 0, Selected }
		public enum VisTargetEnum { Output = 0, Input }
		#region VisTargetEnum consts
		public static readonly string[] VIS_TARGET_ENUM_NAMES = System.Enum.GetNames(typeof(VisTargetEnum));
		public static readonly VisTargetEnum[] VIS_TARGET_ENUM_VALUES = (VisTargetEnum[])System.Enum.GetValues(typeof(VisTargetEnum));
		#endregion
		[System.Serializable]
		public class Data {
			public MapperMaterial.FeatureEnum feature = MapperMaterial.FeatureEnum.IMAGE;
			public Shapes shapes = new Shapes();
		}
		[System.Serializable]
		public class GUIData {
			public KeycodeToggle toggle = new KeycodeToggle(KeyCode.M);
			public int selectedShape = -1;
			public int selectedVertices = 0;

		}
		#endregion
	}
}
