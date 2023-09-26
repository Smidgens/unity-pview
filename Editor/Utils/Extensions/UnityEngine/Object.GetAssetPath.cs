// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
    using UnityEditor;
    using UnityAsset = UnityEngine.Object;

    internal static partial class Object_
    {
        /// <summary>
        /// Retrieves path to given project asset
        /// </summary>
        /// <param name="o">Asset</param>
        /// <returns>File path if valid asset, null otherwise</returns>
        public static string GetAssetPath(this UnityAsset o)
        {
            var path = AssetDatabase.GetAssetPath(o);
            return !string.IsNullOrEmpty(path)
            ? AssetDatabase.GUIDFromAssetPath(path).ToString()
            : null;
        }
    }

}