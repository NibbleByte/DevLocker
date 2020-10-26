using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevLocker.StatesManagement.StatesCommon
{
	// NOTE: This will work properly after Unity 2020.1. StateSceneBind has generic field and is not serialized in previous versions.
	public abstract class StateScenesLoaderBase<TState> : MonoBehaviour, IStateSubscriber where TState : struct, IComparable
	{
		public interface IStateSceneBind
		{
			TState GetState();

			string GetMainScene();
			string[] GetAdditionalScenes();
		}


		// NOTE: StateSceneBind has generic field and is not serialized in Unity versions earlier than 2020.1.
		// This means that the user had to provide child class with specified generic parameters.
#if !UNITY_2020_1_OR_NEWER
		public abstract IStateSceneBind[] GetStateScenes();
#else
		public StateSceneBind[] StateScenes;
		public IStateSceneBind[] GetStateScenes() => StateScenes;

		[System.Serializable]
		public class StateSceneBind : IStateSceneBind
		{
			public TState State;

			public string MainScene;
			public string[] AdditionalScenes;

			public TState GetState() => State;
			public string GetMainScene() => MainScene;
			public string[] GetAdditionalScenes() => AdditionalScenes;
		}

#endif

		public bool IsLoading { get; private set; }

		public abstract StateManagerBase<TState> StateManager { get; }

		// IStateVisualTransition - Wait for any visual transitions like animations, tweens, etc.
		// IStateLoadingCurtainTransition - Wait for any loading curtain transitions like curtain fade-ins etc.
		// Override this to include any custom types to wait for.
		protected Type[] WaitForTransitionTypes = new Type[] { typeof(IStateVisualTransition), typeof(IStateLoadingCurtainTransition) };

		protected bool m_Subscribed { get; private set; }

		public virtual void SubscribeState()
		{
			if (m_Subscribed)
				return;

			m_Subscribed = true;

			StateManager.TransitionStarts += OnTransitionStarts;
		}

		public virtual void UnsubscribeState()
		{
			if (!m_Subscribed)
				return;

			m_Subscribed = false;

			if (StateManager != null) {
				StateManager.TransitionStarts -= OnTransitionStarts;
			}
		}

		protected virtual void OnTransitionStarts(StateEventArgs<TState> e) {
			foreach(var bind in GetStateScenes()) {
				if (e.NextState.CompareTo(bind.GetState()) == 0) {
					StartCoroutine(LoadScenes(e, bind.GetMainScene(), bind.GetAdditionalScenes()));
					return;
				}
			}
		}

		private IEnumerator LoadScenes(StateEventArgs<TState> args, string mainScene, string[] scenes) {

			using (args.GetTransitionScope(this)) {
				IsLoading = true;

				// Wait for other visual transitions to finish up first.
				yield return args.WaitForTransitions(this, WaitForTransitionTypes);

				// If game is starting up and scene is already loaded, don't reload it all over again.
				// Useful (for debug) when entering play mode from any scene that is not the boot up one.
				if (args.PrevState.CompareTo(default) == 0 || !SceneManager.GetSceneByName(mainScene).isLoaded) {
					yield return SceneManager.LoadSceneAsync(mainScene, LoadSceneMode.Single);
				}

				foreach (var scene in scenes) {
					yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
				}

				IsLoading = false;
			}
		}

	}

}