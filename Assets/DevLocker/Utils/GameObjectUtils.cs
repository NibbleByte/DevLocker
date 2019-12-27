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
		public static IEnumerable<T> GetComponents<T>(this Scene scene, bool inChildren)
		{
			Func<GameObject, IEnumerable<T>> getCmp;
			if (inChildren)
				getCmp = go => go.GetComponentsInChildren<T>();
			else
				getCmp = go => go.GetComponents<T>();
			return scene.GetRootGameObjects().SelectMany(getCmp);
		}

		public static T GetComponent<T>(this Scene scene, bool inChildren)
		{
			return scene.GetComponents<T>(inChildren).FirstOrDefault();
		}

		public static Transform FindObject(this Scene scene, string objName)
		{
			foreach (var obj in scene.GetRootGameObjects()) {
				Transform found = obj.transform.FindTransformInChildren(objName);
				if (found != null)
					return found;
			}
			return null;
		}


		public static IEnumerable<Transform> GetChildren(this Transform transform)
		{
			for (int i = 0; i < transform.childCount; ++i)
				yield return transform.GetChild(i);
		}

		public static IEnumerable<T> EnumerateComponentsInChildren<T>(this GameObject go)
		{
			return EnumerateComponentsInChildren<T>(go.transform);
		}
		
		public static IEnumerable<T> EnumerateComponentsInChildren<T>(this Transform transform)
		{
			var components = transform.GetComponents<T>();
			var childComponents = GetChildren(transform).SelectMany(EnumerateComponentsInChildren<T>);
			return components.Concat(childComponents);
		}
		
		public static IEnumerable<Transform> EnumerateTransformsInChildren(this GameObject go)
		{
			return EnumerateTransformsInChildren(go.transform);
		}
		
		public static IEnumerable<Transform> EnumerateTransformsInChildren(this Transform transform)
		{
			var childTransforms = GetChildren(transform).SelectMany(EnumerateTransformsInChildren);
			return Enumerable.Repeat(transform, 1).Concat(childTransforms);
		}

		// Can use object name or path.
		public static Transform FindTransformInChildren(this Transform parent, string pointName)
		{
			return FindTransformInChildren(parent, pointName, includeParent: false);
		}

		public static Transform FindTransformInChildren(this Transform parent, string pointName, bool includeParent)
		{
			if (string.IsNullOrEmpty(pointName))
				return null;
			return FindTransformInChildrenInternal(parent, pointName, includeParent);
		}

		private static Transform FindTransformInChildrenInternal(this Transform parent, string pointName, bool includeParent)
		{
			if (includeParent && parent.name == pointName) {
				return parent;
			}

			Transform transform = parent.Find(pointName);

			if (transform != null)
				return transform;

			for (int i = 0, n = parent.childCount; i < n; ++i) {
				transform = parent.GetChild(i).FindTransformInChildren(pointName);

				if (transform != null)
					return transform;
			}

			return null;
		}

		public static Transform FindTransformInChildren(this Transform parent, Predicate<Transform> predicate)
		{
			if (predicate(parent))
				return parent;

			for (int i = 0, n = parent.childCount; i < n; ++i) {
				var transform = FindTransformInChildren(parent.GetChild(i), predicate);

				if (transform != null)
					return transform;
			}

			return null;
		}

		public static Transform ResetTransform(this Transform transform)
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;

			return transform;
		}

		public static Transform CopyFrom(this Transform transform, Transform other)
		{
			transform.localPosition = other.localPosition;
			transform.localRotation = other.localRotation;
			transform.localScale = other.localScale;
			return transform;
		}

		public static Transform CopyGlobalFrom(this Transform transform, Transform other)
		{
			transform.position = other.position;
			transform.rotation = other.rotation;
			return transform;
		}

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

		// Finds best possible transform according to the path, even if not fully satisfied.
		public static Transform FindBestMatch(Transform root, string path, bool includeParent = true)
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

		// TODO: is obsolete by transform.IsChildOf()? Or maybe IsChildOf doesn't work with inactive objects? If so, change to transform.IsChildOf(true)
		public static bool IsUnder(Transform root, Transform node)
		{
			while (node != null) {

				if (node == root)
					return true;

				node = node.parent;
			}

			return false;
		}

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
		
		// Because Unity API misses this function. Lame.
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

		// Because Unity API misses this function. Lame.
		public static T GetComponentInParent<T>(this GameObject gameObject, bool includeInactive)
		{
			return GetComponentInParent<T>(gameObject.transform, includeInactive);
		}
    }
}
