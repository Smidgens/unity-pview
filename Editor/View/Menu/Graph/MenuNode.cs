// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	/// <summary>
	/// Base class for node type in menu graph
	/// </summary>
	internal abstract class MenuNode : ScriptableObject
	{
		public string Label => GetLabel();
		public float SortWeight => _position.y;
		public Vector2 Position => _position;
		public int Inputs => GetInputs();
		public int Outputs => GetOutputs();
		public Color Color => GetColor();

		public virtual System.Action GetInvokeHandler()
		{
			return null;
		}

		/// <summary>
		/// Save with undo
		/// </summary>
		public static class WithUndo
		{
			public static void SetPosition(MenuNode n, Vector2 v)
			{
				UnityEditor.Undo.RecordObject(n, "Set menu node position");
				n._position = v;
				UnityEditor.EditorUtility.SetDirty(n);
			}
		}

		protected virtual string GetLabel()
		{
			if (string.IsNullOrEmpty(_label))
			{
				return "(untitled)";
			}
			return _label;
		}

		protected virtual Color GetColor() => Color.white;
		protected virtual int GetOutputs() => 0;
		protected virtual int GetInputs() => 1;
		protected virtual Texture GetIcon() => null;

		[SerializeField] protected string _label = "";

		// position in graph
		[HideInInspector]
		[SerializeField] protected Vector2 _position = default;

	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;
	using SP = UnityEditor.SerializedProperty;

	[CustomEditor(typeof(MenuNode), true)]
	internal class _MenuNode : Editor
	{
		public override void OnInspectorGUI()
		{
			// no props
			if (_props == null) { return; }
			serializedObject.UpdateIfRequiredOrScript();
			GUILayout.Space(5f);
			// draw all props
			foreach (var p in _props) { EditorGUILayout.PropertyField(p); }
			serializedObject.ApplyModifiedProperties();
		}

		private SP[] _props = null;

		private void OnEnable()
		{
			if (target.GetType() == typeof(MenuSeparator))
			{
				return;
			}
			_props = new SP[]
			{
				serializedObject.FindProperty("_label"),
			};
		}
	}


}