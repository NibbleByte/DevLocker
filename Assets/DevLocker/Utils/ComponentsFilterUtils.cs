using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DevLocker.Utils
{
	/// <summary>
	/// Procedures to remove undesired components.
	/// </summary>
	public static class ComponentsFilterUtils
	{
		/// <summary>
		/// Removes all components but the ones provided. Tries to take care of dependencies.
		/// WARNING: keepComponents should include dependent components as well (RequreComponent attributes).
		/// </summary>
		public static GameObject FilterInComponents(this GameObject go, params Type[] keepComponents)
		{
			return FilterInComponents(go, true, keepComponents);
		}

		/// <summary>
		/// Removes all components but the ones provided. Tries to take care of dependencies.
		/// WARNING: keepComponents should include dependent components as well (RequreComponent attributes).
		/// </summary>
		public static GameObject FilterInComponents(this GameObject go, bool recursive, params Type[] keepComponents)
		{
			var components = go.GetComponents<Component>();

			int sanityCounter = 0;
			bool componentsPending = true;
			while (componentsPending) {

				componentsPending = false;

				foreach (var component in components) {
					if (component == null)
						continue;

					if (component is Transform)
						continue;

					if (!keepComponents.Any(kc => kc.IsInstanceOfType(component))) {

						if (CheckDependenciesOnComponent(components, component)) {
							componentsPending = true;
						} else {
							// DestroyImmediate so the reference in the array becomes null.
							GameObject.DestroyImmediate(component, true);
						}
					}
				}

				sanityCounter++;
				if (sanityCounter >= 6) {
					Debug.LogError($"Could not filter in components because of complex/circular dependencies for object '{go.name}'!");
					break;
				}
			}

			if (recursive) {
				foreach (Transform child in go.transform) {
					FilterInComponents(child.gameObject, recursive, keepComponents);
				}
			}

			return go;
		}


		/// <summary>
		/// Removes all components that are of the provided type. Tries to take care of dependencies.
		/// WARNING: removeComponents should include dependent components as well (RequreComponent attributes).
		/// </summary>
		public static GameObject FilterOutComponents(this GameObject go, params Type[] removeComponents)
		{
			return FilterOutComponents(go, true, removeComponents);
		}

		/// <summary>
		/// Removes all components that are of the provided type. Tries to take care of dependencies.
		/// WARNING: removeComponents should include dependent components as well (RequreComponent attributes).
		/// </summary>
		public static GameObject FilterOutComponents(this GameObject go, bool recursive, params Type[] removeComponents)
		{
			var components = go.GetComponents<Component>();

			int sanityCounter = 0;
			bool componentsPending = true;
			while (componentsPending) {

				componentsPending = false;

				foreach (var component in components) {
					if (component == null)
						continue;

					if (component is Transform)
						continue;

					if (removeComponents.Any(kc => kc.IsInstanceOfType(component))) {

						if (CheckDependenciesOnComponent(components, component)) {
							componentsPending = true;
						} else {
							// DestroyImmediate so the reference in the array becomes null.
							GameObject.DestroyImmediate(component, true);
						}
					}
				}

				sanityCounter++;
				if (sanityCounter >= 6) {
					Debug.LogError($"Could not filter in components because of complex/circular dependencies for object '{go.name}'!");
					break;
				}
			}

			if (recursive) {
				foreach (Transform child in go.transform) {
					FilterOutComponents(child.gameObject, recursive, removeComponents);
				}
			}

			return go;
		}


		private static readonly Dictionary<KeyValuePair<Type, Type>, bool> _requireBindsDatabase = new Dictionary<KeyValuePair<Type, Type>, bool>();
		private static bool CheckDependenciesOnComponent(Component[] components, Component component)
		{
			// Transform components are always there so ignore them.
			if (component is Transform)
				return false;

			var type = component.GetType();

			foreach (var otherComp in components) {
				if (otherComp == null || ReferenceEquals(otherComp, component))
					continue;

				// Transform components are always there so ignore them.
				if (otherComp is Transform)
					continue;


				var otherType = otherComp.GetType();
				bool isRequired;
				if (!_requireBindsDatabase.TryGetValue(new KeyValuePair<Type, Type>(otherType, type), out isRequired)) {

					var query = otherType.GetCustomAttributes(typeof(RequireComponent)).Select(attr => (RequireComponent) attr);
					isRequired = false;

					foreach (var requirement in query) {
						var t0 = requirement.m_Type0;
						var t1 = requirement.m_Type1;
						var t2 = requirement.m_Type2;

						if (t0 != null && t0.IsInstanceOfType(component)) {
							isRequired = true;
							break;
						}
						if (t1 != null && t1.IsInstanceOfType(component)) {
							isRequired = true;
							break;
						}
						if (t2 != null && t2.IsInstanceOfType(component)) {
							isRequired = true;
							break;
						}
					}

					_requireBindsDatabase.Add(new KeyValuePair<Type, Type>(otherType, type), isRequired);
				}

				if (isRequired)
					return true;
			}

			return false;
		}
    }
}
