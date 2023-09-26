// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
    using UnityEditor;
    using UnityAsset = UnityEngine.Object;

    internal static partial class Object_
    {
        /// <summary>
        /// Retrieves GUID for given project asset
        /// </summary>
        /// <param name="o">Project asset</param>
        /// <returns>String GUID if valid asset, null otherwise</returns>
        public static string GetAssetGUID(this UnityAsset o)
        {
            var path = AssetDatabase.GetAssetPath(o);
            return !string.IsNullOrEmpty(path)
            ? AssetDatabase.GUIDFromAssetPath(path).ToString()
            : null;
        }
    }

}