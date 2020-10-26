
using UnityEngine;
using UnityEngine.UI;

namespace DevLocker.StatesManagement.SampleTanks.Player.UI
{
	public class TanksUIPlayerMoney : MonoBehaviour
	{
		public Text MoneyText;

		private void Start()
		{
			if (MoneyText == null) {
				MoneyText = GetComponent<Text>();
			}

			// No need to subscribe as Level will get destroyed on unload.
			TanksLevelManager.LevelInstance.Player.MoneyChanged += RefreshMoneyText;

			RefreshMoneyText();
		}

		private void RefreshMoneyText()
		{
			MoneyText.text = $"Money: {TanksLevelManager.LevelInstance.Player.Money}";
		}
	}
}