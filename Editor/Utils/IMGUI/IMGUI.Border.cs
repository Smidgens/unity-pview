// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal static partial class IMGUI
	{
		public static void Border(Rect pos, in Color c, in float width = 1f)
		{
			DrawRect(pos.SliceTop(width), c);
			DrawRect(pos.SliceBottom(width), c);
			DrawRect(pos.SliceLeft(width), c);
			DrawRect(pos.SliceRight(width), c);
		}

		public static void BorderLeft(Rect pos, in Color c, in float w = 1f) => DrawRect(pos.SliceLeft(w), c);
		public static void BorderRight(Rect pos, in Color c, in float w = 1f) => DrawRect(pos.SliceRight(w), c);
		public static void BorderTop(Rect pos, in Color c, in float w = 1f) => DrawRect(pos.SliceTop(w), c);
		public static void BorderBottom(Rect pos, in Color c, in float w = 1f) => DrawRect(pos.SliceBottom(w), c);
	}
}