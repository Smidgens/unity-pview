// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;

	internal static partial class IMGUI
	{
		public static Texture WhiteTexture => _WHITE_TEX.Value;

		private static readonly Lazy<Texture>
		_WHITE_TEX = new Lazy<Texture>(TextureFactory.CreatePixelWhite);
	}
}
