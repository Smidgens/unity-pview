// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal static partial class IMGUI
	{
		public static void DrawRect(in Rect r, in Color c)
		{
			// TODO: replace with call to GL later instead of drawing texture
			Texture wt = WhiteTexture;
			if (!wt) { return; }
			Color tc = GUI.color;
			GUI.color = c;
			GUI.DrawTexture(r, wt, ScaleMode.StretchToFill);
			GUI.color = tc;
		}
	}
}