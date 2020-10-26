using DevLocker.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace DevLocker.StatesManagement
{
	/// <summary>
	/// Searches for any child state subscribers and subscribes them.
	/// OnDestroy will unsubscribe them.
	/// The purpose of this component is to bypass Unity's limitation: Awake() & OnDestroy() methods don't get called for inactive objects.
	/// If an designer deactivates temporarily some object and forgets to activate it back, it will never subscribe runtime.
	/// </summary>
	public class SubscribeStateListeners : MonoBehaviour
	{
		private List<IStateSubscriber> m_Subscribers = new List<IStateSubscriber>();

		void Awake()
		{
			foreach(var subscriber in transform.EnumerateComponentsInChildren<IStateSubscriber>(true)) {
				subscriber.SubscribeState();

				m_Subscribers.Add(subscriber);
			}
		}

		private void OnDestroy()
		{
			foreach(var subscriber in m_Subscribers) {
				subscriber.UnsubscribeState();
			}
		}
	}

}