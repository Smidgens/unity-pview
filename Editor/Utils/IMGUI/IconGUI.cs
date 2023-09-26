// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;

	/// <summary>
	/// Methods for drawing custom icons in project view
	/// </summary>
	internal static class IconGUI
	{
		// unity
		public const string UNITY_BG_DARK= "#333333";
		public const string UNITY_BG_LIGHT = "#bebebe";

		// constants used to draw over default project view gui
		public static class UnityDefaults
		{
			// background color
			public static readonly Color
			PV_COLOR_BG = EditorGUIUtility.isProSkin
			? new Color(0.2f, 0.2f, 0.2f) // gray blob
			: new Color(0.745f, 0.745f, 0.745f); // grayish blob?
			// largest drawn icon
			public const float PV_ICON_LG = 88f;
		}

		public static void DrawIcon(Rect rect, Texture icon, in Vector2 padding = default)
		{
			var isSmall = IsSmall(rect);
			TweakLayoutSize(ref rect, isSmall);
			DrawBG(rect);
			AddPadding(ref rect, padding);
			Draw(rect, icon, default, Vector2.one);
		}

		public static void DrawIcon
		(
			Rect rect, Texture icon,
			in Vector2 offset,
			in Vector2 size,
			in Vector2 padding = default
		)
		{
			var isSmall = IsSmall(rect);
			TweakLayoutSize(ref rect, isSmall);
			DrawBG(rect);
			AddPadding(ref rect, padding);
			Draw(rect, icon, offset, size);
		}

		public static Vector2 GetAdjustedSize(Rect rect)
		{
			var isSmall = IsSmall(rect);
			TweakLayoutSize(ref rect, isSmall);
			return rect.size;
		}

		private static void TweakLayoutSize(ref Rect rect, in bool small)
		{
			Equalize(ref rect, small);

			var lgsize = UnityDefaults.PV_ICON_LG;
			if (rect.width > lgsize)
			{
				var offset = (rect.width - lgsize) / 2f;
				rect = new Rect(rect.x + offset, rect.y + offset, lgsize, lgsize);
				return;
			}

			if (small && !IsTree(rect))
			{
				rect = new Rect(rect.x + 3f, rect.y, rect.width, rect.height);
			}
		}


		// should rect be considered small
		private static bool IsSmall(in Rect rect) => rect.width > rect.height;

		private static void AddPadding(ref Rect r, in Vector2 padding)
		{
			var c = r.center;
			r.width -= padding.x;
			r.height -= padding.y;
			r.center = c;
		}

		// scaling
		private static void Equalize(ref Rect rect, in bool small)
		{
			if (small) { rect.width = rect.height; }
			else { rect.height = rect.width; }
		}

		private static void Draw(in Rect r, Texture tex, in Vector2 offset, in Vector2 size)
		{
			if (!tex) { return; }
			//GUI.DrawTexture(r, tex);
			IMGUI.DrawSprite(r, tex, offset, size);
		}

		// draw project view background color
		private static void DrawBG(in Rect r) => EditorGUI.DrawRect(r, UnityDefaults.PV_COLOR_BG);

		// borrowed hack:
		//https://github.com/PhannGor/unity3d-rainbow-folders/
		private static bool IsTree(in Rect rect) => (rect.x - 16) % 14 == 0;
	}
}