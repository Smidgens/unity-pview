// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	// magic constants, lt.dan
	internal static class Constants
	{
		public const string MENU_ROOT = "Project View/";

		public static class SettingsPath
		{
			public const string
			PROJECT = "ProjectSettings/SM_ProjectView.asset",
			USER = "UserSettings/SM_ProjectView.asset";
		}

		// resource paths
		public const string
		TEX_PATH_GRID = "smidgenomics.pview/{grid}";

		// 
		public const string
		SETTINGS_PATH_PROJECT = "ProjectSettings/SM_ProjectView.asset",
		SETTINGS_PATH_USER = "UserSettings/SM_ProjectView.asset";

		public static class MenuActions
		{
			public const string
			ROOT = "Help/" + MENU_ROOT;
		}

		// CreateAssetMenu paths
		public static class CreateAssetMenu
		{
			public const string
			ROOT = MENU_ROOT;
		}

	}
}