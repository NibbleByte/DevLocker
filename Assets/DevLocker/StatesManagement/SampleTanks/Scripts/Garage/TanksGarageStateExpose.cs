using UnityEngine;

namespace DevLocker.StatesManagement.SampleTanks.Garage
{
	// Expose for uGUI Unity events.
	public class TanksGarageStateExpose : MonoBehaviour {

		public void SetState(TanksGarageStates state) {
			TanksGarageLevelManager.Instance.States.SetState(state);
		}

		public void SetStateLobby() {
			SetState(TanksGarageStates.Lobby);
		}

		public void SetStateShop() {
			SetState(TanksGarageStates.Shop);
		}

		public void SetStateInventory() {
			SetState(TanksGarageStates.Inventory);
		}

		public void SetStateStats() {
			SetState(TanksGarageStates.Stats);
		}

		public void SetStateChat() {
			SetState(TanksGarageStates.Chat);
		}


		public void PushStateOptions() {
			TanksGarageLevelManager.Instance.States.PushState(TanksGarageStates.Options);
		}
	}

}