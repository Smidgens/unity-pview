// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEditor;
	using UnityEngine;
	using ROList = UnityEditorInternal.ReorderableList;

	/// <summary>
	/// Wrapper around unity's ReorderableList with convenient defaults
	/// </summary>
	internal class DragList
	{
		public int Length => _list.serializedProperty.arraySize;

		public bool Foldout { get; set; } = true;

		public Func<string> LabelFn { get; set; }

		public DragList
		(
			SerializedProperty prop,
			bool header = true
		)
		{
			_list = new ROList(prop.serializedObject, prop);
			_list.drawElementCallback = OnDrawElement;
			_list.elementHeightCallback = OnGetItemHeight;

			if (header)
			{
				_list.drawHeaderCallback = OnDrawHeader;
			}
			else
			{
				_list.drawHeaderCallback = NoOp;
				_list.headerHeight = 5f;
			}
		}

		public void DoLayoutList()
		{
			var drawFn = GetDrawFn();
			drawFn.Invoke();
		}

		private ROList _list = null;

		private Action GetDrawFn()
		{
			if (Foldout) { return DrawLayoutFoldout; }
			return DrawLayout;
		}

		private void DrawLayout()
		{
			_list.DoLayoutList();
		}

		private void DrawLayoutFoldout()
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);

			var label = LabelFn != null ? LabelFn() : _list.serializedProperty.displayName;

			EditorGUI.indentLevel++;
			_list.serializedProperty.isExpanded =
				EditorGUILayout.Foldout(_list.serializedProperty.isExpanded, label, true);
			EditorGUI.indentLevel--;
			if (_list.serializedProperty.isExpanded)
			{
				_list.DoLayoutList();
			}
			EditorGUILayout.EndVertical();
		}

		private void OnDrawHeader(Rect pos)
		{
			EditorGUI.LabelField(pos, _list.serializedProperty.displayName);
		}

		private float OnGetItemHeight(int i)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		private void OnDrawElement(Rect pos, int i, bool focused, bool active)
		{
			var center = pos.center;
			pos.height = EditorGUIUtility.singleLineHeight; // prevent stretching of row
			pos.center = center;
			var item = _list.serializedProperty.GetArrayElementAtIndex(i);
			EditorGUI.PropertyField(pos, item);
		}

		private void NoOp<T>() { }
		private R NoOp<T,R>(T _) { return default; }
		private void NoOp<T>(T _) { }

	}
}