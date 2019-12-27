using System;
using UnityEngine;
using UnityEngine.Events;

namespace DevLocker.Animations.AnimatorExpositor
{
	/// <summary>
	/// Generic event listener. The response is an UnityEvent that can be used in the UI to link action directly.
	/// Use with AnimStateEventsTrigger.
	/// </summary>
	public class AnimEventListener : MonoBehaviour
	{
		[Serializable]
		public struct EventListenerBind
		{
			public string Name;
			public UnityEvent Target;
		}

		public EventListenerBind[] Listeners;

		public void TriggerEvent(string eventName)
		{

			int foundIndex = Array.FindIndex(Listeners, f => f.Name == eventName);

			if (foundIndex == -1) {
				Debug.LogWarning($"Trying to trigger animation event \"{eventName}\", but there are no handlers.", this);
				return;
			}

			Listeners[foundIndex].Target.Invoke();
		}

	}

}
