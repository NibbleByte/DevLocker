
namespace DevLocker.StatesManagement.SampleTanks.Battle
{
	public class TanksBattleStateActivator : StatesCommon.StateActivatorBase<TanksBattleStates>
	{
		public override StateManagerBase<TanksBattleStates> StateManager => TanksBattleLevelManager.Instance?.States;

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