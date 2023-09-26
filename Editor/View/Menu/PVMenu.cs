// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;

	/// <summary>
	/// Base type for custom editor menu
	/// </summary>
	internal abstract class PVMenu : ScriptableObject
	{
		public virtual GenericMenu GetMenu()
		{
			return new GenericMenu();
		}

		[SerializeField] internal string _menuTitle = "";

		protected GenericMenu MenuFromItems(IEnumerable<MenuItem> items)
		{
			var m = new GenericMenu();

			m.allowDuplicateNames = true;

			if (!string.IsNullOrEmpty(_menuTitle))
			{
				m.AddDisabledItem(new GUIContent($"• {_menuTitle} •"));
				m.AddSeparator("");
			}

			foreach (var item in items)
			{
				if (item.separator) { continue; }
				// if it weren't for this check, the menu would be cachable
				if (UnityUtility.CanExecuteMenu(item.exePath))
				{
					m.AddItem(item.label, false, item.fn);
				}
				else
				{
					m.AddDisabledItem(item.label);
				}
			}
			return m;
		}

		protected struct MenuItem
		{
			public string path;
			public string exePath;
			public GUIContent label;
			public bool separator;
			public GenericMenu.MenuFunction fn;
		}
	}
}