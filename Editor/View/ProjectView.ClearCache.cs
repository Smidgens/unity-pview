// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;

	internal static partial class ProjectView
	{
		/// <summary>
		/// If cache exists, alert handlers
		/// </summary>
		public static event System.Action onClearCache = null;

		/// <summary>
		/// Trigger cache clearing, if something is currently cached
		/// </summary>
		[MenuItem(Constants.MenuActions.ROOT + "Clear Cache", false, priority = -20)]
		public static void ClearCache()
		{
			onClearCache?.Invoke();
		}
	}
}