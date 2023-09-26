// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;

	/// <summary>
	/// Wrapper for selecting available menu items
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal class EditorMenuItemAttribute : PropertyAttribute
	{
		public string Root { get; }

		public string[] Items { get; }
		public EditorMenuItemAttribute(string root)
		{
			Root = root;
			Items = UnityUtility.GetSubmenus(root);
		}

	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;

	using SP = UnityEditor.SerializedProperty;

	/// <summary>
	/// Editor drawer for menu item ref
	/// </summary>
	[CustomPropertyDrawer(typeof(EditorMenuItemAttribute))]
	internal class _EditorMenuItemAttribute : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SP prop, GUIContent l)
		{
			if (l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				pos = EditorGUI.PrefixLabel(pos, l);
			}

			if(prop.propertyType != SerializedPropertyType.String)
			{
				// only support string
				return;
			}
			
			var attr = attribute as EditorMenuItemAttribute;


			if (GUI.Button(pos, GetItemLabel(prop.stringValue), EditorStyles.popup))
			{
				var m = GetDropdownMenu(prop.stringValue, attr.Items, newValue =>
				{
					prop.stringValue = newValue;
					prop.serializedObject.ApplyModifiedProperties();
				});
				m.ShowAsContext();

			}
		}

		private static string GetItemLabel(string value)
		{
			if (string.IsNullOrEmpty(value)) { return "-"; }
			return value.Substring(7);

		}

		// dropdown menu populated with possible menu items
		private static GenericMenu GetDropdownMenu(string v, string[] options, Action<string> onValue)
		{
			var m = new GenericMenu();
			foreach (var o in options)
			{
				var text = o.Substring(7); // cut "Assets/" prefix
				var value = o;
				var l = new GUIContent(text);
				m.AddItem(l, v == o, () => onValue.Invoke(value));
			}
			return m;
		}
	}
}