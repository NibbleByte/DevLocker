
using UnityEngine;

namespace DevLocker.StatesManagement.SampleTanks.GameManager
{
	public enum TanksGameStates
	{
		None = 0,
		MainMenu = 1,
		Garage = 2,
		Battle = 3,
	}

	public class TanksGameManager : MonoBehaviour
	{
		public TanksGameStates StartupState;

		public static TanksGameManager Instance { get; private set; }

		public StateManagerBase<TanksGameStates> States { get; private set; }

		void Awake()
		{
			if (Instance != null) {
				gameObject.SetActive(false);
				return;
			}

			Instance = this;

			DontDestroyOnLoad(gameObject);

			States = new StateManagerBase<TanksGameStates>();
		}

		void Start()
		{
			var playerDataProvider = GetComponent<Player.TanksPlayerDataProvider>();
			if (playerDataProvider) {
				States.SetState(StartupState, playerDataProvider.PlayerData);
			} else {
				States.SetState(StartupState);
			}
		}

		void OnDestroy()
		{
			if (Instance == this) {
				States.Dispose();
				Instance = null;
			}
		}
	}

}