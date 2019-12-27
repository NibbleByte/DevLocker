using System;

namespace DevLocker.Animations.AnimatorExpositor
{
	/// <summary>
	/// Data to select Animator state. Has a custom drawer.
	/// </summary>
	[Serializable]
	public struct AnimatorState
	{
		public int Layer;
		public int State;
	}

}
