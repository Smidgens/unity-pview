// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System.Linq;
	using System.Collections.Generic;

	internal static partial class ProjectView
	{
		public static PVIcons_Rules Icons { get; set; } = default;

		public static void OnGUI(string guid, Rect pos)
		{
			DrawIcons(pos, guid);
			HandleMenu();
		}

		private static void DrawIcons(Rect pos, string guid)
		{
			GetActiveIcons()?.Draw(guid, pos);
		}

		private static void HandleMenu()
		{
			var e = Event.current;

			// not a context event
			if (!e.IsContextClick()) { return; }

			// ctrl is held -> show default unity menu
			if (e.control) { return; }

			var m = GetActiveMenu();

			// shift held -> explicitly use project default
			if (e.shift) { m = PVSettings_Project.instance.Menu; }

			// menu is null -> show default unity
			if (!m) { return; }

			e.Use();
			m.GetMenu()?.ShowAsContext();
		}

		private static PVIcons GetActiveIcons()
		{
			if (PVSettings_User.instance.UseDefaultIcons)
			{
				return null;
			}
			var icons = PVSettings_User.instance.Icons;
			return icons ? icons : PVSettings_Project.instance.Icons;
		}

		private static PVMenu GetActiveMenu()
		{
			if (PVSettings_User.instance.UseDefaultMenu)
			{
				return null;
			}
			var menu = PVSettings_User.instance.Menu;
			return menu ? menu : PVSettings_Project.instance.Menu;
		}

	}
}