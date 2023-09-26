namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine;
	using System.Linq;


	//internal class PVSettingsProvider : SettingsProvider
	internal class PVSettingsProvider
	{

		[SettingsProvider]
		public static SettingsProvider GetProject()
		{
			var provider = AssetSettingsProvider.CreateProviderFromObject("Project/Editor/Project View", PVSettings_Project.instance);
			return provider;
		}

		/// <summary>
		/// Add 
		/// </summary>
		/// <returns></returns>
		[SettingsProvider]
		public static SettingsProvider GetUser()
		{
			var provider = AssetSettingsProvider.CreateProviderFromObject("Preferences/Editor/Project View", PVSettings_User.instance);
			SetScope(provider, SettingsScope.User);
			return provider;
		}


		/// <summary>
		/// Hideous workaround to set scope of AssetSettingsProvider
		/// </summary>
		/// <param name="provider">Provider</param>
		/// <param name="scope">Provider</param>
		private static void SetScope(SettingsProvider provider, SettingsScope scope)
		{
			Type ptype = typeof(SettingsProvider);
			// y u do dis, unity
			var bf = ptype.GetField("<scope>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.NonPublic);
			bf.SetValue(provider, scope);
		}

		//private IDisposable CreateSettingsWindowGUIScope()
		//{
		//	//return new EditorGUI.DisabledScope(false);
		//	//var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
		//	//var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
		//	//return Activator.CreateInstance(type) as IDisposable;
		//	var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
		//	var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
		//	return Activator.CreateInstance(type) as IDisposable;

		//}
	}
}