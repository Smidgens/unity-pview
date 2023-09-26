// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;
	using UnityObject = UnityEngine.Object;

	/// <summary>
	/// Extensions for UnityEngine.Object
	/// </summary>
	internal static partial class UnityObject_
	{
		private static readonly UnityObject[] EMPTY_ASSET_ARR = new UnityObject[0];

		/// <summary>
		/// Loads all nested assets of given type
		/// </summary>
		public static UnityObject[] LoadNestedAssets(this UnityObject target, Type type)
		{
			if (!target.IsAsset()) { return EMPTY_ASSET_ARR; }
			var path = AssetDatabase.GetAssetPath(target);
			return AssetDatabase.LoadAllAssetsAtPath(path)
			.Where(x => x.GetType() == type)
			.OrderBy(a => a.name)
			.ToArray();
		}

		/// <summary>
		/// Loads all nested assets of given type
		/// </summary>
		public static T[] LoadNestedAssets<T>(this UnityObject target) where T : UnityObject
		{
			if (!target.IsAsset()) { return new T[0]; }
			var path = AssetDatabase.GetAssetPath(target);
			return AssetDatabase.LoadAllAssetsAtPath(path)
			.Select(x => x as T)
			.Where(x => x)
			.OrderBy(a => a.name)
			.ToArray();
		}
	}
}
