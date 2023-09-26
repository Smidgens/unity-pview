// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;
	using System.Linq;
	using UnityEngine.Serialization;
	using System.Collections.Generic;
	using UnityEditor;

	/// <summary>
	/// Custom menu using pattern matching
	/// </summary>
	[CreateAssetMenu(menuName = Constants.CreateAssetMenu.ROOT + "Menu/Pattern")]
	internal class PVMenu_Rules : PVMenu
	{
		[Inlined.Fields(typeof(MatchRule))]
		[FormerlySerializedAs("_hide")]
		[FormerlySerializedAs("_match")]
		[SerializeField] internal MatchRule[] _rules = { };
		[Inlined.Fields(typeof(MoveRule))]
		[SerializeField] internal MoveRule[] _move = { };
		[SerializeField] internal Settings _settings = default;

		public override GenericMenu GetMenu()
		{
			return MenuFromItems(GetItems());
		}

		private IEnumerable<MenuItem> GetItems()
		{
			var items =
			UnityUtility.GetSubmenus("Assets")
			.Where(x =>
			{
				var cmpPath = x.Substring(7);

				foreach (var r in ALWAYS_SKIPPED_MENUS)
				{
					if (cmpPath.StartsWith(r)) { return false; }
				}
				if (!Match(cmpPath)) { return false; }
				return true;
			})
			.ToArray();

			var options = new List<MenuItem>();

			for (var i = 0; i < items.Length; i++)
			{
				var path = items[i];
				var cmpPath = path.Substring(7);

				var label =
				MatchMove(cmpPath, out var newLabel)
				? newLabel
				: path.Substring(7);

				var nextPath = i < items.Length - 1 ? items[i + 1] : null;
				var exePath = $"{path}";

				options.Add(new MenuItem
				{
					exePath = exePath,
					label = new GUIContent(label),
					//separator = separator,
					path = path,
					fn = () => UnityUtility.ExecuteMenu(exePath)
				});
			}
			return options;
		}

		private static readonly string[] ALWAYS_SKIPPED_MENUS =
		{
			"Create/Playables" // these bug unity out for some reason
		};

		internal enum RuleMode
		{
			Exclude,
			Include
		}

		[Serializable]
		internal struct Settings
		{
			public RuleMode mode;
		}

		private bool MatchMove(in string p, out string newPath)
		{
			newPath = null;
			if (string.IsNullOrEmpty(p)) { return false; }
			foreach(var r in _move)
			{
				if(Wildcard.IsMatch(p, r.pattern))
				{
					newPath = Move(p, r.output);
					return true;
				}
			}
			return false;
		}

		private static string Move(in string path, in string newPrefix)
		{
			if (newPrefix.Length == 0)
			{
				return path.Split('/').LastOrDefault();
			}
			return newPrefix + "/" + path.Split('/').LastOrDefault();
		}

		private bool Match(string p)
		{
			if (_settings.mode == RuleMode.Exclude)
			{
				return MatchOrDefault(p, false, true);
			}
			return MatchOrDefault(p, true, false);
		}

		private bool MatchOrDefault(string p, bool matchResult, bool def)
		{
			foreach (var r in _rules)
			{
				if (Wildcard.IsMatch(p, r.pattern))
				{
					return matchResult;
				}
			}
			return def;
		}

		[Serializable]
		internal class MatchRule
		{
			public string pattern;
		}

		[Serializable]
		internal class MoveRule
		{
			public string pattern;
			public string output;
		}
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Reflection;

	[CustomEditor(typeof(PVMenu_Rules))]
	internal class _PVMenu_PatternMatched : Editor
	{
		public override void OnInspectorGUI()
		{

			EditorGUILayout.Space();
			serializedObject.UpdateIfRequiredOrScript();

			foreach(var p in _defaultProperties)
			{
				EditorGUILayout.PropertyField(p);
			}

			GUILayout.Space(5f);

			//if(_drawFn)
			_drawFn?.Invoke();

			//DrawTabs();

			//_tabs[_activeTab].handler();

			//_list1.DoLayoutList();
			//_list2.DoLayoutList();
			serializedObject.ApplyModifiedProperties();

			GUILayout.Space(5f);

			EditorGUILayout.LabelField("Debug", EditorStyles.largeLabel);

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Clear Cache"))
			{
				ProjectView.ClearCache();
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Preview"))
			{
				Preview();
			}

			EditorGUILayout.EndHorizontal();
		}

		[SerializeField]
		private int _activeTab = 0;

		private class Tab
		{
			public string Label
			{
				get
				{
					return labelFn != null ? labelFn() : label;
				}
			}
			public string label;
			public Func<string> labelFn;
			public Action handler;

			public void Deconstruct(out string tabName, out Action handler)
			{
				tabName = label;
				handler = this.handler;
			}
		}

		private List<Tab> _tabs = new List<Tab>();

		private Action _drawFn = null;

		private IEnumerable<SerializedProperty> _defaultProperties = null;

		private static string[] _defaultPropertyNames =
		{
			nameof(PVMenu._menuTitle)
		};


		private void OnEnable()
		{
			_defaultProperties = _defaultPropertyNames.Select(pn => serializedObject.FindProperty(pn));


			// init
			var matchRules = CreateListForProperty(nameof(PVMenu_Rules._rules));
			var moveRules = CreateListForProperty(nameof(PVMenu_Rules._move));
			var settingsProps = GetNestedProperties<PVMenu_Rules.Settings>(nameof(PVMenu_Rules._settings));


			matchRules.LabelFn = () =>
			{
				var include = ((PVMenu_Rules)target)._settings.mode == PVMenu_Rules.RuleMode.Include;
				var prefix = include ? "Include" : "Exclude";
				return $"{prefix} ({matchRules.Length})";
			};

			moveRules.LabelFn = () => $"Move ({matchRules.Length})";

			_drawFn = () =>
			{


				DrawLayout(settingsProps);
				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Rules", EditorStyles.whiteLargeLabel);
				matchRules.DoLayoutList();
				moveRules.DoLayoutList();
			};

			_tabs.Add(new Tab
			{
				label = "Settings",
				handler = () =>
				{
					DrawLayout(settingsProps);
					//EditorGUILayout.PropertyField(settings);
				}
			});

			_tabs.Add(new Tab
			{
				labelFn = () =>
				{
					var include = ((PVMenu_Rules)target)._settings.mode == PVMenu_Rules.RuleMode.Include;
					var prefix = include ? "Include" : "Exclude";
					return $"{prefix} ({matchRules.Length})";
				},
				handler = () =>
				{
					matchRules.DoLayoutList();
				}
			});

			_tabs.Add(new Tab
			{
				labelFn = () => $"Move ({matchRules.Length})",
				handler = () =>
				{
					moveRules.DoLayoutList();
				}
			});

		}

		private void OnDisable()
		{
			// cleanup
		}

		private void Preview()
		{
			Debug.Log("Preview N/I");
		}

		private void DrawTabs()
		{
			EditorGUILayout.BeginHorizontal(GUI.skin.box);

			int i = 0;
			foreach(var tab in _tabs)
			{
				using (new EditorGUI.DisabledScope(_activeTab == i))
				{
					if (GUILayout.Button(tab.Label, EditorStyles.toolbarButton))
					{
						SetTab(i);
					}
				}
				i++;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void SetTab(int i)
		{
			_activeTab = i;
			// delaying this to allow GUI to finish drawing before attempting stuff
			//EditorApplication.delayCall += () => _activeTab = i;
		}

		private SerializedProperty[] GetNestedProperties<T>(string name)
		{
			var settings = serializedObject.FindProperty(nameof(PVMenu_Rules._settings));

			var bindings = BindingFlags.Public | BindingFlags.Instance;
			var type = typeof(T);
			var fields = type.GetFields(bindings);

			return fields.Select(field =>
			{
				return settings.FindPropertyRelative(field.Name);
			}).ToArray();
		}

		private void DrawLayout(params SerializedProperty[] props)
		{
			foreach(var p in props) { EditorGUILayout.PropertyField(p); }
		}

		private DragList CreateListForProperty(string name)
		{
			var p = serializedObject.FindProperty(name);
			if (!p.isArray)
			{
				throw new ArgumentException($"Cannot create reorderable list for property {name}");
			}
			return new DragList(p, header:false);
		}
	}
}


// TODO: move to separate file
namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;

	internal static class Enumerable_
	{
		public static void ForEach<T>(this IEnumerable<T> c, Action<T, int> fn)
		{
			int i = 0;
			foreach (var item in c)
			{
				fn(item, i);
				i++;
			}
		}

		public static void ForEach<T>(this IEnumerable<T> c, Action<T> fn)
		{
			foreach (var item in c)
			{
				fn(item);
			}
		}
	}

}