using UnityEngine;

namespace DevLocker.Utils
{
	/// <summary>
	/// Add this to the root of your nested layout groups if they don't manage to rebuild properly due to dynamically resized elements inside.
	/// When enabled it will wait for the current layout rebuild to finish, then do another final rebuild pass.
	/// </summary>
	public class UIRebuildLayoutOnEnable : MonoBehaviour
	{
		private bool m_RebuildDone = false;

		void OnEnable()
		{
			m_RebuildDone = false;
		}

		private void Update()
		{
			if (!m_RebuildDone && !UIUtils.IsLayoutRebuildPending()) {
				m_RebuildDone = true;
				UIUtils.ForceRecalclulateLayouts((RectTransform)transform);
			}
		}

		/// <summary>
		/// Call this to force layout rebuild (after the current rebuilds finish).
		/// </summary>
		[ContextMenu("Rebuild Layout")]
		public void RebuildLayout()
		{
			m_RebuildDone = false;
		}
	}
}