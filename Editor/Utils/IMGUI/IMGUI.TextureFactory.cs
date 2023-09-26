// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	internal static partial class IMGUI
	{
		internal static class TextureFactory
		{
			public static Texture CreatePixel(Color c) => CreateTex(1, c);
			public static Texture CreatePixelWhite() => CreateTex(1, Color.white);

			private static Texture CreateTex(in int size, in Color c)
			{
				var t = new Texture2D(size, size);
				for (var x = 0; x < size; x++)
				{
					for (var y = 0; y < size; y++)
					{
						t.SetPixel(x, y, c);
					}
				}
				t.Apply();
				t.filterMode = FilterMode.Point;
				return t;
			}
		}
	}
}