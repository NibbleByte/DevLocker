using System;
using UnityEngine;

namespace DevLocker.Animations.AnimatorExpositor
{
	/// <summary>
	/// Exposes public "Execute()" method that will set the configured value to the specified Animator parameter.
	/// Useful to link in UI.
	/// </summary>
	public class AnimSetParams : MonoBehaviour, IAnimatorProvider
	{
		public enum ActionType
		{
			Set,
			Add,
			Multiply,
			Toggle,
			Clear,
		}

		public Animator Animator;
		public bool ExecuteOnAwake = false;
		public ActionType Action = ActionType.Set;

		[Header("Parameter:")]
		[AnimatorParam]
		public int AnimParam;

		public bool BoolValue;
		public float FloatValue;
		public int IntValue;

		private Animator _animator;

		void Awake()
		{
			if (ExecuteOnAwake) {
				Execute();
			}
		}

		[ContextMenu("Execute")]
		public void Execute()
		{
			if (!_animator) {
				_animator = GetAnimator();
			}

			if (!_animator) {
				Debug.LogError("Cannot read animator parameters. Animator component is missing or disabled.", this);
				return;
			}

			if (_animator.runtimeAnimatorController == null) {
				Debug.LogError("No controller assigned to the Animator.", this);
				return;
			}

			if (_animator.parameterCount == 0) {
				Debug.LogError("No parameters available.", this);
				return;
			}

			var foundParam = Array.Find(_animator.parameters, p => p.nameHash == AnimParam);

			if (foundParam == null) {
				Debug.LogError("Parameter not found. Maybe it was renamed or removed.", this);
				return;
			}

			switch (foundParam.type) {

				case AnimatorControllerParameterType.Bool:

					switch (Action) {

						case ActionType.Set:
							_animator.SetBool(AnimParam, BoolValue);
							break;
						case ActionType.Toggle:
							_animator.SetBool(AnimParam, !_animator.GetBool(AnimParam));
							break;
						default:
							Debug.LogError($"Invalid {Action} action for animator parameter {foundParam.name} of type {foundParam.type}.",
								this);
							break;
					}
					break;



				case AnimatorControllerParameterType.Float:

					switch (Action) {
						case ActionType.Set:
							_animator.SetFloat(AnimParam, FloatValue);
							break;
						case ActionType.Add:
							_animator.SetFloat(AnimParam, FloatValue + _animator.GetFloat(AnimParam));
							break;
						case ActionType.Multiply:
							_animator.SetFloat(AnimParam, FloatValue * _animator.GetFloat(AnimParam));
							break;
						default:
							Debug.LogError($"Invalid {Action} action for animator parameter {foundParam.name} of type {foundParam.type}.",
								this);
							break;
					}
					break;



				case AnimatorControllerParameterType.Int:

					switch (Action) {
						case ActionType.Set:
							_animator.SetInteger(AnimParam, IntValue);
							break;
						case ActionType.Add:
							_animator.SetInteger(AnimParam, IntValue + _animator.GetInteger(AnimParam));
							break;
						case ActionType.Multiply:
							_animator.SetInteger(AnimParam, IntValue * _animator.GetInteger(AnimParam));
							break;
						default:
							Debug.LogError($"Invalid {Action} action for animator parameter {foundParam.name} of type {foundParam.type}.",
								this);
							break;
					}
					break;



				case AnimatorControllerParameterType.Trigger:

					switch (Action) {
						case ActionType.Set:
							_animator.SetTrigger(AnimParam);
							break;
						case ActionType.Clear:
							_animator.ResetTrigger(AnimParam);
							break;
						default:
							Debug.LogError($"Invalid {Action} action for animator parameter {foundParam.name} of type {foundParam.type}.",
								this);
							break;
					}
					break;
			}
		}

		public Animator GetAnimator()
		{
			return Animator ? Animator : GetComponent<Animator>();
		}
	}

}
