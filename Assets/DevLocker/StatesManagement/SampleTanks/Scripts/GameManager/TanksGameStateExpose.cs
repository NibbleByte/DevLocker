using UnityEngine;

namespace DevLocker.StatesManagement.SampleTanks.GameManager
{
	// Expose for uGUI Unity events.
	public class TanksGameStateExpose : MonoBehaviour {

		public void SetState(TanksGameStates state) {
			TanksGameManager.Instance.States.SetState(state);
		}

		public void SetStateMainMenu() {
			SetState(TanksGameStates.MainMenu);
		}

		public void SetStateGarage() {
			SetState(TanksGameStates.Garage);
		}

		public void SetStateBattle() {
			SetState(TanksGameStates.Battle);
		}
	}

}