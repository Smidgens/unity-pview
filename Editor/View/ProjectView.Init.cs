//// smidgens @ github


//namespace Smidgenomics.Unity.ProjectView.Editor
//{
//	using UnityEditor;
//	using UnityEngine;

//	[InitializeOnLoad]
//	internal static partial class ProjectView
//	{
//		static ProjectView()
//		{
//			Init();
//		}

//		private static void Init()
//		{
//			EditorApplication.projectWindowItemOnGUI -= OnProjectItemGUI;
//			EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;
//		}

//		private static void OnProjectItemGUI(string guid, Rect r)
//		{
//			OnGUI(guid, r);
//		}
//	}
//}
