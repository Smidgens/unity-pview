// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;

	/// <summary>
	/// Submenu/group node
	/// </summary>
	internal class MenuGroup : MenuNode
	{
		protected override int GetOutputs() => 1;

		protected override Color GetColor()
		{
			return new Color(0.349f, 0.074f, 0.925f);
		}
	}
}
