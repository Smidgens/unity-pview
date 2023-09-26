// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEngine.Events;
	using System;
	using System.Linq;

	/// <summary>
	/// Menu option node
	/// </summary>
	internal class MenuAction : MenuNode
	{
		public static readonly Color NODE_COLOR = new Color(0.925f, 0.474f, 0.074f);

		public override Action GetInvokeHandler()
		{
			if (_type == ActionType.ExecuteMenuItem)
			{
				if (!_menuItem.CanExecute()) { return null; }
			}

			// todo: return null if action can't be invoked currently
			return Invoke;
		}

		public void Invoke()
		{
			switch (_type)
			{
				case ActionType.ExecuteMenuItem: _menuItem.Invoke(); break;
				case ActionType.InvokeUnityEvent: _onEvent.Invoke(); break;
			}
			_onAfterInvoke.Invoke();
		}
		protected override Color GetColor() => NODE_COLOR;

		protected override string GetLabel()
		{
			if (_type == ActionType.ExecuteMenuItem && string.IsNullOrEmpty(_label))
			{
				var p = _menuItem?.Path;
				if (!string.IsNullOrEmpty(p))
				{
					var lastItem = p.Split('/').LastOrDefault();
					if (lastItem != null)
					{
						return lastItem;
					}
				}
			}
			return base.GetLabel();
		}

		private enum ActionType
		{
			/// <summary>
			/// Menu Path
			/// </summary>
			ExecuteMenuItem,
			/// <summary>
			/// Exec event handlers
			/// </summary>
			InvokeUnityEvent,
		}

		[SerializeField] private ActionType _type = default;
		// menu option
		[Inlined.Fields("_item", HideLabel = true)]
		[SerializeField] private MenuItemRef _menuItem = default;

		[HideInInspector]
		// event handlers
		[SerializeField] private UnityEvent _onEvent = null;

		[HideInInspector]
		// executed regardless of type
		[SerializeField] private UnityEvent _onAfterInvoke = null;

		/// <summary>
		/// Editor helper
		/// </summary>
		public static class FNames
		{
			public const string
			LABEL = nameof(_label),
			TYPE = nameof(_type),
			MENU_PATH = nameof(_menuItem),
			ON_EVENT = nameof(_onEvent),
			ON_ALWAYS = nameof(_onAfterInvoke);
		}
	}
}


namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using SP = UnityEditor.SerializedProperty;

	[CustomEditor(typeof(MenuAction))]
	internal class _MenuAction : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();

			GUILayout.Space(5f);

			EditorGUILayout.PropertyField(_label);
			EditorGUILayout.PropertyField(_type);

			GUILayout.Space(10f);

			// select which handler to draw
			var tp = _type.enumValueIndex == 0 ? _menuItem : _onEvent;
			EditorGUILayout.PropertyField(tp);

			GUILayout.Space(10f);

			EditorGUILayout.PropertyField(_onAfterInvoke);
			serializedObject.ApplyModifiedProperties();
		}

		private SP
		_label = default,
		_type = default,
		_menuItem = default,
		_onEvent = default,
		_onAfterInvoke = default;

		private void OnEnable()
		{
			_label = serializedObject.FindProperty(MenuAction.FNames.LABEL);
			_type = serializedObject.FindProperty(MenuAction.FNames.TYPE);
			_menuItem = serializedObject.FindProperty(MenuAction.FNames.MENU_PATH);
			_onAfterInvoke = serializedObject.FindProperty(MenuAction.FNames.ON_ALWAYS);
			_onEvent = serializedObject.FindProperty(MenuAction.FNames.ON_EVENT);
		}

	}


}