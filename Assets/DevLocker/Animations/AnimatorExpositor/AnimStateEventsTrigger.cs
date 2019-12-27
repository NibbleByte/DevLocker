using UnityEngine;

namespace DevLocker.Animations.AnimatorExpositor
{
	/// <summary>
	/// StateMachineBehaviour that triggers generic events.
	/// Use with AnimEventListener.
	/// </summary>
	public class AnimStateEventsTrigger : StateMachineBehaviour
	{
		public string StartedEventName;
		public string FinishedEventName;

		private AnimEventListener _listener;

		// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!_listener) {
				_listener = animator.GetComponent<AnimEventListener>();
			}

			if (!string.IsNullOrWhiteSpace(StartedEventName)) {
				_listener.TriggerEvent(StartedEventName);
			}
		}

		// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{

			if (!string.IsNullOrWhiteSpace(FinishedEventName)) {
				_listener.TriggerEvent(FinishedEventName);
			}
		}
	}

}
