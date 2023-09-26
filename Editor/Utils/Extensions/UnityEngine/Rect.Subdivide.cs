// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	/// <summary>
	/// Extensions for UnityEngine.Rect
	/// </summary>
	internal static partial class Rect_
	{
		/// <summary>
		/// Split rect into vertical subrects
		/// </summary>
		public static Rect[] SubdivideHeight(this in Rect r, float padding, params float[] heights)
		{
			float absoluteHeight = padding * (heights.Length - 1);
			for (int i = 0; i < heights.Length; i++)
			{
				if (heights[i] <= 1f) { continue; }
				absoluteHeight += heights[i];
			}
			float remainder = r.height - absoluteHeight;
			if (remainder < 0) { remainder = 0f; }

			Rect[] rects = new Rect[heights.Length];
			float offset = 0f;
			for (int i = 0; i < heights.Length; i++)
			{
				rects[i] = new Rect(r.x, r.y + offset, r.width, heights[i] <= 1f ? heights[i] * remainder : heights[i]);
				offset += rects[i].height + padding;
			}
			return rects;
		}

		public static Rect[] SubdivideWidth(this Rect pos, double pad, params float[] widths)
		{
			var r = new Rect[widths.Length];
			if (widths.Length == 0) { return r; }
			var (poffset, ptotal) = GetSplitPadding(widths.Length, pos.width, pad);
			var totalSize = pos.width - ptotal;
			var w = totalSize.Subdivide(widths);
			var offset = 0f;
			for (var i = 0; i < w.Length; i++)
			{
				r[i] = pos;
				r[i].x += offset;
				r[i].width = w[i];
				offset += w[i] + poffset;
			}
			return r;
		}

		private static (float, float) GetSplitPadding(int n, float v, double p)
		{
			if (n < 2) { return default; }
			var o = System.Convert.ToSingle(p);
			// ratio
			if (o < 1) { o = o * v; }
			return (o, o * (n - 1));
		}
	}
}