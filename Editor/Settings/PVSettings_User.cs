// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;

	[System.Flags]
	internal enum PVDefaultFlags
	{
		None = 0,
		Menu = 1,
		Icons = 2,
		All = ~0
	}

	[FilePath(Constants.SETTINGS_PATH_USER, FilePathAttribute.Location.ProjectFolder)]
	internal class PVSettings_User : ScriptableSingleton<PVSettings_User>
	{
		public PVMenu Menu => _menu;
		public PVIcons Icons => _icons;
		public PVDefaultFlags Flags => _useDefaults;

		internal enum PVMenuBehaviour
		{
			Nothing,
			UseUnityMenu,
			UseProjectMenu
		}

		public bool UseDefaultMenu => _useDefaults.HasFlag(PVDefaultFlags.Menu);
		public bool UseDefaultIcons => _useDefaults.HasFlag(PVDefaultFlags.Icons);

		[Header("Profiles")]
		[SerializeField] internal PVMenu _menu = default;
		[SerializeField] internal PVIcons _icons = default;

		[Header("Settings")]
		[SerializeField] internal PVDefaultFlags _useDefaults = default;

		[Header("Shift/Ctrl")]
		[SerializeField] internal PVMenuBehaviour _ctrlBehaviour = PVMenuBehaviour.UseUnityMenu;
		[SerializeField] internal PVMenuBehaviour _shiftBehaviour = PVMenuBehaviour.UseProjectMenu;

		[Space]
		[EditorMenuItem("Assets")]
		[SerializeField] internal string _testMenu = default;

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
	using UnityEditor;
	using System.Linq;

	[CustomEditor(typeof(PVSettings_User))]
	internal class _PVSettings_User : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			serializedObject.UpdateIfRequiredOrScript();

			foreach (var p in _props)
			{
				EditorGUILayout.PropertyField(p);
			}

			if (EditorGUI.EndChangeCheck() || serializedObject.hasModifiedProperties)
			{
				serializedObject.ApplyModifiedProperties();
				((PVSettings_User)target).Save();
			}
		}

		private SerializedProperty[] _props = { };

		private static string[] PROP_NAMES =
		{
			nameof(PVSettings_User._menu),
			nameof(PVSettings_User._icons),
			nameof(PVSettings_User._useDefaults),
			nameof(PVSettings_User._ctrlBehaviour),
			nameof(PVSettings_User._shiftBehaviour),
		};

		private void OnEnable()
		{
			_props = PROP_NAMES.Select(name => serializedObject.FindProperty(name)).ToArray();
		}

		private void OnDisable()
		{
			// this might cause sync issues with undo/redo, not sure though
			PVSettings_User.instance.Save();
		}
	}
}