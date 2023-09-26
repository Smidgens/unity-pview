// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;
	using UnityEditor;

	[CreateAssetMenu(menuName = Constants.CreateAssetMenu.ROOT + "Icons")]
	internal class PVIcons_Rules : PVIcons
	{
		public override void Draw(string guid, in Rect pos)
		{
			AtlasSprite icon = default;

			if(MatchIcon(guid, ref icon))
			{
				IconGUI.DrawIcon(pos, icon.Texture, icon.Offset, icon.Size);
			}
		}

		[SerializeField] internal IcoRule[] _rules = { };

		private bool MatchIcon(string guid, ref AtlasSprite icon)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			var ctx = new MatchContext
			{
				guid = guid,
				assetPath = path,
				isFolder = AssetDatabase.IsValidFolder(path),
			};

			foreach (var r in _rules)
			{
				if (MatchOne(r, ctx))
				{
					icon = r.icon;
					return true;
				}
			}

			return false;
		}

		internal AtlasSprite SpriteAt(int i)
		{
			return i > -1 && i < _rules.Length ? _rules[i].icon : default;
		}

		internal enum RuleTarget { Folder, Asset, }

		private struct MatchContext
		{
			public string guid;
			public string assetPath;
			public bool isFolder;
		}

		private bool MatchOne(in IcoRule rule, in MatchContext ctx)
		{
			if(rule.type == RuleType.Asset)
			{
				return rule.asset == ctx.guid;
			}

			// path/type
			if (string.IsNullOrEmpty(rule.pattern))
			{
				return false;
			}

			if(rule.type == RuleType.Path)
			{
				if(ctx.isFolder != rule.folder) { return false; }

				return Wildcard.IsMatch(ctx.assetPath, rule.pattern);
			}

			return false;
		}

		internal enum RuleType
		{
			Asset,
			Path,
			Type
		}

		[Serializable]
		internal struct IcoRule
		{
			public string label;
			public RuleType type;
			public string pattern;
			public AtlasSprite icon;
			public bool mute;
			[AssetGUID]
			public string asset;
			public bool folder;
		}

	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using ROList = UnityEditorInternal.ReorderableList;

	[CustomEditor(typeof(PVIcons_Rules))]
	internal class _PVProfile_Icons : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();

			if(_list.count > 0 && _list.index < 0 || _list.index > _list.count)
			{
				_list.index = 0;
			}

			EditorGUILayout.Space(2f);
			_list.DoLayoutList();
			GUILayout.Space(4f);
			DrawSelected();
			serializedObject.ApplyModifiedProperties();
		}

		private ROList _list = null;

		private void OnEnable()
		{
			var rulesProp = serializedObject.FindProperty(nameof(PVIcons_Rules._rules));

			_list = new ROList(serializedObject, rulesProp);

			_list.drawElementCallback = DrawListElement;

			_list.elementHeight = EditorGUIUtility.singleLineHeight;

			_list.drawHeaderCallback = r =>
			{
				EditorGUI.LabelField(r, _list.serializedProperty.displayName);
			};

			_list.onAddCallback = l =>
			{
				var ni = _list.serializedProperty.arraySize;
				_list.serializedProperty.arraySize++;
				var newElement = _list.serializedProperty.GetArrayElementAtIndex(ni);
				var size = newElement.FindPropertyRelative("icon._size");
				size.vector2Value = Vector2.one;
			};
		}

		private static string[][] _typeProps =
		{
			new string[]
			{
				"asset"
			},
			new string[]
			{
				"pattern",
				"folder"
			},
			new string[]
			{
				"pattern",
			}
		};

		private void DrawListElement(Rect pos, int i, bool f, bool a)
		{
			/*
			- type
			- icon
			*/

			var prop = _list.serializedProperty.GetArrayElementAtIndex(i);

			var icon = ((PVIcons_Rules)target).SpriteAt(i);

			//var icoTex = prop.FindPropertyRelative("icon._texture")?.objectReferenceValue as Texture;
			if (icon.Texture)
			{
				var icoPos = pos.SliceRight(pos.height);
				icoPos.Resize(-1f);
				icon.Draw(icoPos);
				//GUI.DrawTexture(icoPos, icoTex);
				pos.SliceRight(2f);
			}

			var type = prop.FindPropertyRelative("type");

			string label = (prop.FindPropertyRelative("label")).stringValue;

			if (string.IsNullOrEmpty(label))
			{
				var sb = new System.Text.StringBuilder("");

				//var typeName = ((PVProfile_Icons.IconRuleType)type.enumValueIndex).ToString();
				var typeName = type.enumNames[type.enumValueIndex];

				sb.Append(typeName);
				sb.Append(": ");

				if (type.enumValueIndex == 0)
				{
					var guid = prop.FindPropertyRelative("asset").stringValue;
					sb.Append(string.IsNullOrEmpty(guid) ? "<unset>" : guid);
				}

				if (type.enumValueIndex == 1)
				{
					var pattern = prop.FindPropertyRelative("pattern").stringValue;
					sb.Append(string.IsNullOrEmpty(pattern) ? "<unset>" : pattern);
				}
				label = sb.ToString();
			}

			EditorGUI.LabelField(pos, label, EditorStyles.miniLabel);
		}

		private void DrawSelected()
		{
			var i = _list.index;
			if(i < 0 || i >= _list.count) { return; }
			var prop = _list.serializedProperty.GetArrayElementAtIndex(i);
			var icon = prop.FindPropertyRelative("icon");

			var type = prop.FindPropertyRelative("type");
			var label = prop.FindPropertyRelative("label");

			EditorGUILayout.PropertyField(label);
			EditorGUILayout.Space(5f);


			EditorGUILayout.PropertyField(type);

			if (!_typeProps.IsOutOfBounds(type.enumValueIndex))
			{
				PropertyAll(prop, _typeProps[type.enumValueIndex]);
			}
			EditorGUILayout.Space(5f);
			EditorGUILayout.PropertyField(icon);

		}

		private void PropertyAll(SerializedProperty prop, string[] childProps)
		{
			foreach (var pn in childProps)
			{
				EditorGUILayout.PropertyField(prop.FindPropertyRelative(pn));
			}
		}


	}
}