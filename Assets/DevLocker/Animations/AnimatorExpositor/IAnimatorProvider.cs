using UnityEngine;

namespace DevLocker.Animations.AnimatorExpositor
{
	/// <summary>
	/// Provides the used Animator.
	/// </summary>
	public interface IAnimatorProvider
	{
		Animator GetAnimator();
	}

}
