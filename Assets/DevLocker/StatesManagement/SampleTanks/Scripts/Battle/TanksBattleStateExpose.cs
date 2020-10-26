using UnityEngine;

namespace DevLocker.StatesManagement.SampleTanks.Battle
{
	// Expose for uGUI Unity events.
	public class TanksBattleStateExpose : MonoBehaviour {

		public void SetState(TanksBattleStates state) {
			TanksBattleLevelManager.Instance.States.SetState(state);
		}

		public void SetStateBattle() {
			SetState(TanksBattleStates.Battle);
		}

		public void SetStateBattleSummary() {
			SetState(TanksBattleStates.BattleSummary);
		}


		public void PushStateOptions() {
			TanksBattleLevelManager.Instance.States.PushState(TanksBattleStates.Options);
		}
	}

}