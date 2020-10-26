using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevLocker.Utils
{
	/// <summary>
	/// Basic operations for searching, iterating and processing GameObjects and Transforms in scenes.
	/// </summary>
	public static class GameObjectUtils
	{
		/// <summary>
		/// Enumerate children.
		/// Breaking enumerator early will prevent further calculations.
		/// </summary>
		public static IEnumerable<Transform> EnumerateChildren(this Transform transform)
		{
			for (int i = 0; i < transform.childCount; ++i)
				yield return transform.GetChild(i);
		}

		/// <summary>
		/// Enumerate all specified components of the game objects of the loaded scene.
		/// Breaking enumerator early will prevent further calculations.
		/// Special case for Transform type.
		/// </summary>
		public static IEnumerable<T> EnumerateComponentsInChildren<T>(this Scene scene, bool includeInactive)
		{
			foreach(var go in scene.GetRootGameObjects()) {
				foreach(var component in go.EnumerateComponentsInChildren<T>(includeInactive)) {
					yield return component;
				}
			}
		}

		/// <summary>
		/// Enumerate all specified components of the game object itself and it's children recursively.
		/// Breaking enumerator early will prevent further calculations.
		/// Special case for Transform type.
		/// </summary>
		public static IEnumerable<T> EnumerateComponentsInChildren<T>(this GameObject go, bool includeInactive)
		{
			return EnumerateComponentsInChildren<T>(go.transform, includeInactive);
		}

		/// <summary>
		/// Enumerate all specified components of the transform itself and it's children recursively.
		/// Breaking enumerator early will prevent further calculations.
		/// Special case for Transform type.
		/// </summary>
		public static IEnumerable<T> EnumerateComponentsInChildren<T>(this Transform transform, bool includeInactive)
		{
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(transform);
			List<T> components = new List<T>();

			bool typeIsTransform = typeof(T) == typeof(Transform);

			while (queue.Count > 0) {
				var nextTransform = queue.Dequeue();

				if (typeIsTransform) {
					yield return (T)(object)nextTransform;

				} else {
					nextTransform.GetComponents(components);

					foreach (var component in components) {
						yield return component;
					}
				}

				foreach(var child in EnumerateChildren(nextTransform)) {
					if (!includeInactive && child.gameObject.activeSelf == false)
						continue;

					queue.Enqueue(child);
				}
			}
		}

		/// <summary>
		/// Reset local position, rotation and scale.
		/// </summary>
		public static Transform ResetTransform(this Transform transform)
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;

			return transform;
		}

		/// <summary>
		/// Copy local position, rotation and scale from another transform.
		/// </summary>
		public static Transform CopyFrom(this Transform transform, Transform other)
		{
			transform.localPosition = other.localPosition;
			transform.localRotation = other.localRotation;
			transform.localScale = other.localScale;
			return transform;
		}

		/// <summary>
		/// Copy local position, rotation and scale to another transform.
		/// </summary>
		public static Transform CopyTo(this Transform transform, Transform other)
		{
			other.localPosition = transform.localPosition;
			other.localRotation = transform.localRotation;
			other.localScale = transform.localScale;
			return transform;
		}

		/// <summary>
		/// Copy global position, rotation from another transform.
		/// </summary>
		public static Transform CopyGlobalFrom(this Transform transform, Transform other)
		{
			transform.position = other.position;
			transform.rotation = other.rotation;
			return transform;
		}

		/// <summary>
		/// Copy global position, rotation to another transform.
		/// </summary>
		public static Transform CopyGlobalTo(this Transform transform, Transform other)
		{
			other.position = transform.position;
			other.rotation = transform.rotation;
			return transform;
		}

		/// <summary>
		/// Generate path suitable for use with Transform.Find() between root and node.
		/// Root can be null.
		/// </summary>
		public static string GetFindPath(Transform root, Transform node)
		{
			string findPath = string.Empty;

			while (node != root && node != null) {
				findPath = node.name + (string.IsNullOrEmpty(findPath) ? "" : ("/" + findPath));
				node = node.parent;
			}

			if (root == null)
				findPath = "/" + findPath;

			return findPath;
		}

		/// <summary>
		/// Find transform by name with specified StringComparison method.
		/// Breaking enumerator early will prevent further calculations.
		/// </summary>
		public static IEnumerable<Transform> FindTransform(this Transform transform, string name, StringComparison comparison, bool includeInactive = false)
		{
			foreach (var t in transform.EnumerateComponentsInChildren<Transform>(includeInactive)) {
				if (t.name.Equals(name, comparison))
					yield return t;
			}
		}

		/// <summary>
		/// Finds best possible transform according to the path, even if not fully satisfied.
		/// </summary>
		public static Transform FindBestMatch(this Transform root, string path, bool includeParent = true)
		{
			Transform result = root;

			int currentStartIndex = 0;
			int currentEndIndex = path.IndexOf("/");
			currentEndIndex = (currentEndIndex == -1) ? path.Length : currentEndIndex;

			while (currentStartIndex < path.Length) {

				if (currentStartIndex != currentEndIndex) {
					var nextName = path.Substring(currentStartIndex, currentEndIndex - currentStartIndex);

					var current = result.Find(nextName);
					if (current == null)
						break;

					result = current;
				}

				currentStartIndex = currentEndIndex + 1;

				if (currentStartIndex < path.Length) {
					currentEndIndex = path.IndexOf("/", currentStartIndex);
					currentEndIndex = (currentEndIndex == -1) ? path.Length : currentEndIndex;
				}
			}

			if (result == root && !includeParent) {
				result = null;
			}

			return result;
		}

		/// <summary>
		/// Destroy all children and detach them immediately.
		/// This way they won't be iterated by further requests in this frame.
		/// </summary>
		public static void DestroyChildren(this Transform transform, bool immediate = false)
		{
			for (int i = transform.childCount - 1; i >= 0; --i) {
				var child = transform.GetChild(i).gameObject;
				if (immediate)
					GameObject.DestroyImmediate(child);
				else
					GameObject.Destroy(child);
			}
			transform.DetachChildren();
		}


		/// <summary>
		/// Because Unity API misses this function. Lame.
		/// </summary>
		public static T GetComponentInParent<T>(this Component component, bool includeInactive)
		{
			if (!includeInactive) {
				return component.GetComponentInParent<T>();
			}

			var transform = component.transform;

			while (transform) {
				T target = transform.GetComponent<T>();
				if (target != null && !target.Equals(null))
					return target;

				transform = transform.parent;
			}

			return default(T);
		}

		/// <summary>
		/// Because Unity API misses this function. Lame.
		/// </summary>
		public static T GetComponentInParent<T>(this GameObject gameObject, bool includeInactive)
		{
			return GetComponentInParent<T>(gameObject.transform, includeInactive);
		}
    }
}
