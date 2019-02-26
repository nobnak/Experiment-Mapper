using nobnak.Gist;
using nobnak.Gist.DataUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.UIElement {

	public class TextVector2 : ReactiveValue<Vector2> {

		protected readonly TextFloat[] texts;

		public TextVector2(Vector2 data) : base(data) {
			texts = new TextFloat[2];
			for (var i = 0; i < texts.Length; i++) {
				texts[i] = new TextFloat(data[i]);
				texts[i].Changed += r => SetDataInternal();
			}
		}

		#region interface
		public string this[int index] {
			get {
				return texts[index].StrValue;
			}
			set {
				texts[index].StrValue = value;
			}
		}
		#endregion

		#region member
		protected void SetDataInternal() {
			base.SetData(new Vector2(texts[0].Value, texts[1].Value));
		}
		protected void SetStrDataInternal() {
			for (var i = 0; i < texts.Length; i++)
				texts[i].Value = data[i];
		}
		#endregion

		#region Reactive
		protected override void SetData(Vector2 value) {
			SetStrDataInternal();
			base.SetData(value);
		}
		#endregion
	}
}
