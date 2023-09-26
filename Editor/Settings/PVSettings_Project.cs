// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	[FilePath(Constants.SETTINGS_PATH_PROJECT, FilePathAttribute.Location.ProjectFolder)]
	internal class PVSettings_Project : ScriptableSingleton<PVSettings_Project>
	{
		public PVMenu Menu => _menu;
		public PVIcons Icons => _icons;

		[Header("Default Profiles")]
		[SerializeField] internal PVMenu _menu = default;
		[SerializeField] internal PVIcons _icons = default;


		public void Save()
		{
			Save(true);
		}

		private void OnEnable()
		{
			// unity would draw props as readonly otherwise
			hideFlags &= ~HideFlags.NotEditable;
		}
	}
}


namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEditor;
	using System.Linq;
	using UnityEngine;

	[CustomEditor(typeof(PVSettings_Project))]
	internal class _PVSettings_Project : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			serializedObject.UpdateIfRequiredOrScript();
			//base.OnInspectorGUI();

			foreach(var p in _props)
			{
				EditorGUILayout.PropertyField(p);
			}

			if (EditorGUI.EndChangeCheck() || serializedObject.hasModifiedProperties)
			{
				serializedObject.ApplyModifiedProperties();
				((PVSettings_Project)target).Save();
			}
		}

		private SerializedProperty[] _props = { };

		private static string[] PROP_NAMES =
		{
			nameof(PVSettings_Project._menu),
			nameof(PVSettings_Project._icons),
			//nameof(PVSettings_Project._testMenu),
		};

		private void OnEnable()
		{
			_props = PROP_NAMES.Select(name => serializedObject.FindProperty(name)).ToArray();
		}

		private void OnDisable()
		{
			// this might cause sync issues with undo/redo, not sure though
			PVSettings_Project.instance.Save();
		}
	}
}