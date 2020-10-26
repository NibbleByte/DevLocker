using UnityEngine;
using System;
using System.Linq;
using System.Collections;

namespace DevLocker.StatesManagement.StatesCommon
{

	/// <summary>
	/// Trigger Animator for each state.
	/// If the Animator has a trigger with the same name as the changed state it will be triggered.
	/// Trigger can also be of type bool and int. It will set it to true or 1 if state matches. It will set it to false or 0 if the state is left.
	/// </summary>
	public abstract class StateAnimatorTriggerBase<TState> : MonoBehaviour, IStateSubscriber, IStateVisualTransition where TState : struct, IComparable
	{
		[Tooltip("How long the transition should last. Sync this with your animations, since the code can't know how much time the animator will take.")]
		public float TransitionDuration = 0f;

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

			using (args.GetTransitionScope(this)) {
				yield return new WaitForSeconds(TransitionDuration);
			}
		}

		protected virtual void ApplyState(StateEventArgs<TState> args)
		{
			var prevStateName = args.PrevState.ToString();
			var nextStateName = args.NextState.ToString();

			var parameters = Animator.parameters;
			foreach (var parameter in parameters) {
				var parName = parameter.name;

				if (parName == prevStateName) {
					SetFlag(parameter, false);

				} else if (parName == nextStateName) {
					SetFlag(parameter, true);
				}
			}
		}

		protected void SetFlag(AnimatorControllerParameter param, bool flag)
		{
			switch (param.type) {
				case AnimatorControllerParameterType.Trigger:
					if (flag) Animator.SetTrigger(param.name);
					else Animator.ResetTrigger(param.name);
					break;

				case AnimatorControllerParameterType.Bool:
					Animator.SetBool(param.name, flag);
					break;

				case AnimatorControllerParameterType.Int:
					Animator.SetInteger(param.name, flag ? 1 : 0);
					break;
			}
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
		}
	}

}