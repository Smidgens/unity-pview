// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using System;

	/// <summary>
	/// Wrapper for selecting menu item
	/// </summary>
	[Serializable]
	internal class MenuItemRef
	{
		public string Path => _item;

		public void Invoke()
		{
			if (!CanExecute()) { return; }
			UnityUtility.ExecuteMenu(_item);
		}

		/// <summary>
		/// Can command be called
		/// </summary>
		/// <returns></returns>
		public bool CanExecute()
		{
			if (string.IsNullOrEmpty(_item)) { return false; }
			return UnityUtility.CanExecuteMenu(_item);
		}

		[EditorMenuItem("Assets")]
		[SerializeField] private string _item = default;
	}
}