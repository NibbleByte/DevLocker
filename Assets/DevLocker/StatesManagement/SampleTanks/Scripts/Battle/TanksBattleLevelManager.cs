using DevLocker.StatesManagement.SampleTanks.GameManager;
using DevLocker.StatesManagement.SampleTanks.Player;
using UnityEngine;

namespace DevLocker.StatesManagement.SampleTanks.Battle
{
	public enum TanksBattleStates
	{
		None = 0,
		Battle = 1,
		BattleSummary = 2,

		Options = 20,
	}

	public class TanksBattleLevelManager : TanksLevelManager
	{
		public TanksBattleStates StartupState;

		public static TanksBattleLevelManager Instance => (TanksBattleLevelManager)LevelInstance;

		public StateManagerBase<TanksBattleStates> States { get; private set; }

		protected override void Awake()
		{
			base.Awake();

			if (IsActingManager) {
				States = new StateManagerBase<TanksBattleStates>();
			}
		}

		void Start()
		{
			var playerData = (TanksPlayerData)TanksGameManager.Instance.States.CurrentOrLastEventArgs.NextParam;
			Player = new TanksPlayer(playerData);

			Debug.LogWarning($"Starting Battle with player: {Player.Data}.");

			States.PushState(StartupState);
		}

		public override void PopCurrentState()
		{
			States.PopState();
		}

		public void Shoot()
		{
			Player.AddAmmo(-1);
		}

		public void GoToGarage()
		{
			TanksGameManager.Instance.States.SetState(TanksGameStates.Garage, Player.Data);
		}
	}
}