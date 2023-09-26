// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;

	/// <summary>
	/// Helpers for various nifty utilities in unity
	/// </summary>
	internal static class UnityUtility
	{
		/// <summary>
		/// Retrieve System.Type for main asset at given path
		/// </summary>
		/// <param name="path">Project path</param>
		/// <returns></returns>
		public static Type GetTypeAtPath(in string path) => AssetDatabase.GetMainAssetTypeAtPath(path);

		/// <summary>
		/// Executes menu item path
		/// </summary>
		/// <param name="item">Path string</param>
		public static void ExecuteMenu(string item) => EditorApplication.ExecuteMenuItem(item);

		/// <summary>
		/// Check if given menu path can be executed in current context
		/// For example: can an asset be created in the current location
		/// </summary>
		public static bool CanExecuteMenu(string path) => Menu.GetEnabled(path);

		/// <summary>
		/// Returns list of menu items at given root path (context menu, window etc.)
		/// </summary>
		/// <param name="path"></param>
		/// <param name="separators"></param>
		/// <returns></returns>
		public static string[] GetSubmenus(string path, bool separators = false)
		{
			return separators
			? Unsupported.GetSubmenusIncludingSeparators(path)
			: Unsupported.GetSubmenus(path);
		}

		/// <summary>
		/// Is the editor in dark mode?
		/// </summary>
		public static readonly bool IsDark = EditorGUIUtility.isProSkin;

		/// <summary>
		/// Background color for current editor theme
		/// </summary>
		public static readonly Color
		BackgroundColor = IsDark
		? new Color(0.2f, 0.2f, 0.2f) // gray blob
		: new Color(0.745f, 0.745f, 0.745f); // grayish blob?
	}
}