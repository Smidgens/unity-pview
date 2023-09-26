// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;
	using UnityEditor;

	internal abstract class PVIcons : ScriptableObject
	{
		public virtual void Draw(string guid, in Rect pos)
		{
			var size = IconGUI.GetAdjustedSize(pos);
			var ico = FindIcon(guid, size.x);

			if (!ico) { return; }
			IconGUI.DrawIcon(pos, ico);
		}

		protected virtual Texture FindIcon(string guid, in float width)
		{
			return null;
		}

		protected virtual bool MatchGUID(string guid, ref AtlasSprite sprite)
		{
			return false;
		}

	}
}