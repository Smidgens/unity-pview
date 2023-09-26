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

		/// <summary>
		/// Checks if object is asset (exists on disk)
		/// </summary>
		public static bool IsAsset(this UnityObject o) => o && EditorUtility.IsPersistent(o);

		/// <summary>
		/// Instantiates nested asset
		/// </summary>
		public static T CreateNested<T>(this UnityObject parent, string name = null, bool hide = true) where T : ScriptableObject
		{
			if (!parent.IsAsset()) { return null; }
			var ob = ScriptableObject.CreateInstance<T>();
			if (name == null) { name = $"New {typeof(T).Name}"; }
			ob.name = name;
			// hidden from user in project
			if (hide) { ob.hideFlags = HideFlags.HideInHierarchy; }
			Undo.RegisterCreatedObjectUndo(ob, $"Instantiate {typeof(T).Name}");
			AssetDatabase.AddObjectToAsset(ob, parent);
			return ob;
		}
	}
}
