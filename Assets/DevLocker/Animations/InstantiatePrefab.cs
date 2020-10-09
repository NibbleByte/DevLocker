using DevLocker.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DevLocker.Animations
{
	/// <summary>
	/// Useful with animation events.
	/// Attach next to an Animator and use animation events to instantiate prefabs.
	/// Event parameters:
	/// - objectReferenceParameter - Prefab to instantiate
	/// - stringParameter - name of the parent to attach the instance to.
	/// - intParameter - 0 - destroy previous instance, 1 - multiple instances.
	/// </summary>
	public class InstantiatePrefab : MonoBehaviour
	{
		private readonly Dictionary<string, List<GameObject>> _instances = new Dictionary<string, List<GameObject>>();

		public void Instantiate(AnimationEvent ev)
		{
			var prefab = ev.objectReferenceParameter as GameObject;
			if (prefab == null) {
				Debug.LogError($"Prefab is null or incorrect type for {ev.stringParameter} destination. Time: {ev.time}", this);
				return;
			}

			var destination = !string.IsNullOrWhiteSpace(ev.stringParameter)
				? transform.FindTransform(ev.stringParameter, System.StringComparison.Ordinal).FirstOrDefault()
				: transform;

			if (destination == null) {
				Debug.LogError($"Destination \"{ev.stringParameter}\" not found! Prefab \"{prefab.name}\" not instantiated. Time: {ev.time}", this);
				return;
			}

			var instance = GameObject.Instantiate(prefab, destination);
			instance.transform.ResetTransform();

			string bindName = GetBindName(ev);

			List<GameObject> instances;
			if (!_instances.TryGetValue(bindName, out instances)) {
				instances = new List<GameObject>();
				_instances.Add(bindName, instances);
			}

			if (ev.intParameter == 0) {
				Destroy(ev);
			}

			instances.Add(instance);
		}

		public void Destroy(AnimationEvent ev)
		{
			List<GameObject> instances;
			if (!_instances.TryGetValue(GetBindName(ev), out instances)) {
				return;
			}

			foreach (var instance in instances) {
				if (instance) {
					GameObject.Destroy(instance);
				}
			}
		}

		private string GetBindName(AnimationEvent ev)
		{
			return ev.objectReferenceParameter.name + ev.stringParameter;
		}
	}

}
