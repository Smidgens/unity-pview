// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityAsset = UnityEngine.Object;

	internal static partial class Object_
	{
		public static void TrySelect<T>(this T o) where T : UnityAsset
		{
			if (!o) { return; }
			Selection.activeObject = o;
		}
	}

}