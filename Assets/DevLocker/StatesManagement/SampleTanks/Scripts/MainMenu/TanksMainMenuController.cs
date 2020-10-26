using DevLocker.StatesManagement.SampleTanks.GameManager;

namespace DevLocker.StatesManagement.SampleTanks.MainMenu
{
	public class TanksMainMenuController : Player.TanksPlayerDataProvider
	{
		public void StartGame()
		{
			TanksGameManager.Instance.States.SetState(TanksGameStates.Garage, PlayerData);
		}
	}
}