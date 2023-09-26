// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal static partial class IMGUI
	{
		// draw area of 
		public static void DrawSprite
		(
			in Rect area,
			Texture atlas,
			in Vector2 offset,
			in Vector2 size
		)
		{
			if (size.x == 0 || size.y == 0 || !atlas) { return; }
			using (new GUIScope.Clip(area))
			{
				var sx = 1f / size.x;
				var sy = 1f / size.y;
				var ir = area;
				ir.size = new Vector2
				(
					sx * area.width,
					sy * area.width
				);
				ir.position = new Vector2
				(
					-offset.x * area.width * sx,
					-offset.y * area.height * sy
				);
				GUI.DrawTexture(ir, atlas, ScaleMode.StretchToFill);
			}
		}
	}

}