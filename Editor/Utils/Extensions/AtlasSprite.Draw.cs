// smidgens @ github

using UnityEngine;

namespace Smidgenomics.Unity.ProjectView.Editor
{
	internal static class AtlasSprite_
	{
		/// <summary>
		/// Draw sprite to IMGUI
		/// </summary>
		/// <param name="sprite">Sprite</param>
		/// <param name="pos">Rect on screen</param>
		public static void Draw(this in AtlasSprite sprite, in Rect pos)
		{
			IMGUI.DrawSprite(pos, sprite.Texture, sprite.Offset, sprite.Size);
		}
	}
}