

namespace DevLocker.StatesManagement.SampleTanks.GameManager
{
	public class TanksGameStateScenesLoader : StatesCommon.StateScenesLoaderBase<TanksGameStates>
	{

		// NOTE: base.StateSceneBind has generic field and is not serialized in Unity versions earlier than 2020.1.
		// This means that the user had to provide child class with specified generic parameters.
#if !UNITY_2020_1_OR_NEWER
		[System.Serializable]
		public class StateSceneBind : IStateSceneBind
		{
			public TanksGameStates State;

			public string MainScene;
			public string[] AdditionalScenes;

			public TanksGameStates GetState() => State;
			public string GetMainScene() => MainScene;
			public string[] GetAdditionalScenes() => AdditionalScenes;
		}

		public StateSceneBind[] StateScenes;
		public override IStateSceneBind[] GetStateScenes() => StateScenes;
#endif

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