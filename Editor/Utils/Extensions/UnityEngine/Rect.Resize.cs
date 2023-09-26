// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal static partial class Rect_
	{
		public static void Resize(this ref Rect r, in float s) => r.Resize(s, s, s, s);
		public static void Resize(this ref Rect r, float h, in float v) => r.Resize(h, h, v, v);

		public static void Resize
		(
			this ref Rect rect,
			in float l,
			in float r,
			in float t,
			in float b
		)
		{
			var c = rect.center;
			rect.width += l + r;
			rect.height += t + b;
			rect.center = c;
		}
	}
}