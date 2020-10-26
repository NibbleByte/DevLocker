using UnityEngine;
using System;
using System.Linq;
using System.Collections;

namespace DevLocker.StatesManagement.StatesCommon
{

	/// <summary>
	/// Sets Animator int parameter to the current state int value.
	/// The Animator parameter must have the same name as the state enum type name.
	/// </summary>
	public abstract class StateAnimatorSetIntValueBase<TState> : MonoBehaviour, IStateSubscriber, IStateVisualTransition where TState : struct, IComparable
	{
		[Tooltip("How long the transition should last. Sync this with your animations, since the code can't know how much time the animator will take.")]
		public float TransitionDuration = 0f;

		[Tooltip("Ignored states will be set and transition skipped. States outside this list will be waited out.")]
		public TState[] IgnoreStates;

		[Tooltip("(Optional) Target Animator. If left null, Animator on this object is used.")]
		public Animator Animator;

		public abstract StateManagerBase<TState> StateManager { get; }

		protected bool m_Subscribed { get; private set; }

		public virtual void SubscribeState()
		{
			if (m_Subscribed)
				return;

			m_Subscribed = true;

			if (Animator == null) {
				Animator = GetComponent<Animator>();
			}

			OnValidate();

			if (Animator) {
				StateManager.TransitionStarts += OnTransitionStarts;
				ApplyState(StateManager.CurrentOrLastEventArgs);
			}
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

		protected virtual void OnTransitionStarts(StateEventArgs<TState> e)
		{
			StartCoroutine(StartAnimation(e));
		}

		private IEnumerator StartAnimation(StateEventArgs<TState> args)
		{
			ApplyState(args);

			if (TransitionDuration <= 0)
				yield break;

			if (Array.IndexOf(IgnoreStates, args.NextState) != -1)
				yield break;

			using (args.GetTransitionScope(this)) {
				yield return new WaitForSeconds(TransitionDuration);
			}
		}

		protected virtual void ApplyState(StateEventArgs<TState> args)
		{
			// NOTE: nameof(TState) doesn't work!
			Animator.SetInteger(typeof(TState).Name, (int)(object)args.NextState);
		}

		protected virtual void OnValidate()
		{
			var targetAnimator = Animator;
			if (targetAnimator == null) {
				targetAnimator = GetComponent<Animator>();
			}

			if (targetAnimator == null) {
				Debug.LogError($"{name} ({GetType().Name}) doesn't have any animator to work with!", this);
				return;
			}


			// NOTE: nameof(TState) doesn't work!
			if (targetAnimator.parameters.All(p => p.name != typeof(TState).Name || p.type != AnimatorControllerParameterType.Int)) {
				Debug.LogError($"{targetAnimator.name} Animator doesn't have int parameter of type int {typeof(TState).Name}. This is needed for {GetType().Name} can work!", this);
				return;
			}
		}
	}

}