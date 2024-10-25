﻿
namespace DevLocker.StatesManagement.SampleTanks.GameManager
{
	public class TanksGameStateAnimatorSetIntValue : StatesCommon.StateAnimatorSetIntValueBase<TanksGameStates>
	{
		public override StateManagerBase<TanksGameStates> StateManager => TanksGameManager.Instance?.States;

		// Optional! It is up to your life-cycle policy.
		// Or you can use SubscribeStateListeners on the root game object.
		// Or Both! Subscribing twice is safe.
		//void Awake()
		//{
		//	SubscribeState();
		//}

		void OnDestroy()
		{
			UnsubscribeState();
		}
	}

}