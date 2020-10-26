using System;

namespace DevLocker.StatesManagement.SampleTanks.Player
{
	public class TanksPlayer
	{
		public string Username => m_PlayerData.Username;
		public int Money => m_PlayerData.Money;
		public int Ammo => m_PlayerData.Ammo;

		public TanksPlayerData Data => m_PlayerData;

		public Action AmmoChanging;
		public Action AmmoChanged;

		public Action MoneyChanging;
		public Action MoneyChanged;

		private TanksPlayerData m_PlayerData;

		public TanksPlayer(TanksPlayerData initialData)
		{
			m_PlayerData = initialData;
		}

		public void AddAmmo(int amount)
		{
			AmmoChanging?.Invoke();
			m_PlayerData.Ammo += amount;
			AmmoChanged?.Invoke();
		}

		public void AddMoney(int amount)
		{
			MoneyChanging?.Invoke();
			m_PlayerData.Money += amount;
			MoneyChanged?.Invoke();
		}
	}
}