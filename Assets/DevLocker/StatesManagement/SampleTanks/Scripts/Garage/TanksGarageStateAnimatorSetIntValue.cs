
namespace DevLocker.StatesManagement.SampleTanks.Garage
{
	public class TanksGarageStateAnimatorSetIntValue : StatesCommon.StateAnimatorSetIntValueBase<TanksGarageStates>
	{
		public override StateManagerBase<TanksGarageStates> StateManager => TanksGarageLevelManager.Instance?.States;

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