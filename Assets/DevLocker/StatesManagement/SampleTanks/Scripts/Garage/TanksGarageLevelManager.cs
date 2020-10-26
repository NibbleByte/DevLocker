
using DevLocker.StatesManagement.SampleTanks.GameManager;
using DevLocker.StatesManagement.SampleTanks.Player;
using UnityEngine;

namespace DevLocker.StatesManagement.SampleTanks.Garage
{
	public enum TanksGarageStates
	{
		None = 0,
		Lobby = 1,
		Shop = 2,
		Inventory = 3,
		Stats = 4,
		Chat = 5,

		Options = 20,
	}

	public class TanksGarageLevelManager : TanksLevelManager
	{
		public TanksGarageStates StartupState;

		public static TanksGarageLevelManager Instance => (TanksGarageLevelManager)LevelInstance;

		public StateManagerBase<TanksGarageStates> States { get; private set; }

		protected override void Awake()
		{
			base.Awake();

			if (IsActingManager) {
				States = new StateManagerBase<TanksGarageStates>();
			}
		}

		void Start()
		{
			var playerData = (TanksPlayerData)TanksGameManager.Instance.States.CurrentOrLastEventArgs.NextParam;
			Player = new TanksPlayer(playerData);

			Debug.LogWarning($"Starting Garage with player: {Player.Data}.");

			States.PushState(StartupState);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (IsActingManager) {
				States.Dispose();
			}
		}

		public override void PopCurrentState()
		{
			States.PopState();
		}

		public void BuyAmmo()
		{
			Player.AddAmmo(50);
			Player.AddMoney(-100);
		}

		public void GoToBattle()
		{
			TanksGameManager.Instance.States.SetState(TanksGameStates.Battle, Player.Data);
		}
	}

}