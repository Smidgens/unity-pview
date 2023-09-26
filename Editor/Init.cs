// smidgens @ github


namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEditor;
	using UnityEditor.Callbacks;

	/// <summary>
	/// Registers 
	/// </summary>
	[InitializeOnLoad]
	internal static partial class PVInit
	{
		static PVInit()
		{
			RegisterCallbacks();
		}

		/// <summary>
		/// Registers editor callbacks
		/// </summary>
		private static void RegisterCallbacks()
		{
			EditorApplication.projectWindowItemOnGUI -= ProjectView.OnGUI;
			EditorApplication.projectWindowItemOnGUI += ProjectView.OnGUI;
		}

		/// <summary>
		/// Callback for menu graph asset
		/// </summary>
		[OnOpenAsset]
		private static bool OnEditMenuGraph(int instanceID, int line)
		{
			var ob = EditorUtility.InstanceIDToObject(instanceID);
			if (ob.GetType() != typeof(PVMenu_Graph)) { return false; }

			EditMenuGraph.Open((PVMenu_Graph)ob);

			return true;
		}
	}
}
