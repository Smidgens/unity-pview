// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;
	using System.Reflection;
	using System.Linq;
	using System.Collections.Generic;

	internal static class Inlined
	{
		/// <summary>
		/// Set options for specific field
		/// </summary>
		internal class FieldOptionAttribute : PropertyAttribute
		{
			public float Width { get; } = 0f;

			public FieldOptionAttribute(float width = 0f)
			{
				Width = width;
			}
		}

		/// <summary>
		/// Render nested fields on a single line
		/// </summary>
		internal class FieldsAttribute : PropertyAttribute
		{
			public const float DEFAULT_PADDING = 2f;

			/// <summary>
			/// Space between fields
			/// </summary>
			public float Padding { get; } = DEFAULT_PADDING;

			public float FixedWidthTotal { get; } = 0f;

			public bool HideLabel { get; set; } = false;

			/// <summary>
			/// Field names
			/// </summary>
			public string[] Fields { get; } = { };
			public float[] Widths { get; } = { };

			public FieldsAttribute(params string[] fields)
			{
				Fields = fields;
				Widths = 1f.Subdivide(fields.Length);
			}

			public FieldsAttribute(Type t)
			{
				var autoSized = 0;
				var totalFixed = 0f;
				var reservedRatio = 0f;

				var flexIndices = new List<int>();
				var widths = new List<float>();

				var fields =
				t.GetFields(_FIELD_FLAGS)
				.Where(x =>
				{
					return !x.IsNotSerialized && x.GetCustomAttribute<HideInInspector>() == null;
				})
				.Select((x, i) =>
				{
					var o = x.GetCustomAttribute<FieldOptionAttribute>();
					var w = o != null ? o.Width : 0f;

					if (w == 1f) { autoSized++; }
					else if (w > 1f) { totalFixed += w; }
					else if (w > 0f)
					{
						reservedRatio += w;
					}
					else { autoSized++; }

					widths.Add(w);
					return x.Name;
				})
				.ToArray();

				var ratioRemainder = 1f - reservedRatio;
				var autoRatio = autoSized > 0 ? (1f - reservedRatio) / autoSized : 0;
				if (autoRatio < 0f) { autoRatio = 0f; }

				for (var i = 0; i < widths.Count; i++)
				{
					if (widths[i] == 0f)
					{
						widths[i] = autoRatio;
					}
				}
				Fields = fields;
				Widths = widths.ToArray();
			}


			private static readonly
			BindingFlags _FIELD_FLAGS =
			BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.Instance;

		}
	}
}


namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;

	[CustomPropertyDrawer(typeof(Inlined.FieldsAttribute))]
	internal class FieldRowAttribute_PD : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent l)
		{
			var a = attribute as Inlined.FieldsAttribute;

			if (!a.HideLabel && l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				pos = EditorGUI.PrefixLabel(pos, l);
			}

			var fields = a.Fields;
			if (fields.Length == 0) { return; }

			pos.height = EditorGUIUtility.singleLineHeight;
			var cols = pos.SubdivideWidth(2.0, a.Widths);

			for (var i = 0; i < fields.Length; i++)
			{
				var fieldProp = prop.FindPropertyRelative(fields[i]);
				if (fieldProp != null) { EditorGUI.PropertyField(cols[i], fieldProp, GUIContent.none); }
				else { EditorGUI.DrawRect(cols[i], Color.red * 0.3f); } // field not found
			}
		}
	}
}
