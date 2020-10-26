
using UnityEngine;
using UnityEngine.UI;

namespace DevLocker.StatesManagement.SampleTanks.Player.UI
{
	public class TanksUIPlayerAmmo : MonoBehaviour
	{
		public Text AmmoText;

		private void Start()
		{
			if (AmmoText == null) {
				AmmoText = GetComponent<Text>();
			}

			// No need to subscribe as Level will get destroyed on unload.
			TanksLevelManager.LevelInstance.Player.AmmoChanged += RefreshAmmoText;

			RefreshAmmoText();
		}

		private void RefreshAmmoText()
		{
			AmmoText.text = $"Ammo: {TanksLevelManager.LevelInstance.Player.Ammo}";
		}
	}
}