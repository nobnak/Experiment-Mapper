using M.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M.Converters {

	public static class CoordConverter {

		public static Vector2Int TextureSize(Texture tex = null) {
			return new Vector2Int(
				tex != null ? tex.width : Screen.width,
				tex != null ? tex.height : Screen.height);
		}

		public static Matrix4x4 ScreenToNDC {
			get {
				var s = TextureSize();
				return Matrix4x4.TRS(
					new Vector3(-1f, -1f, 0f),
					Quaternion.identity,
					new Vector3(2f / s.x, 2f / s.y, 1f)
					);
			}
		}
	}
}
