using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using System.Collections.Generic;

namespace DevLocker
{
	/// <summary>
	/// This WAS used to bypass the Unity limitation of not having nested prefabs support.
	/// Put this script on a proxy object, link a prefab and it will render that prefab in the scene as it was there.
	/// You can select and move the proxy object. When the scene runs, the script spawns that prefab onto the proxy object.
	/// When build prefab instances are baked into the scene directly, so there should be no performance overhead.
	/// Can be used recursively.
	/// 
	/// This is an improved version of the "Poor Mans Nested Prefabs" by Nicholas Francis
	/// http://framebunker.com/blog/poor-mans-nested-prefabs/
	/// Additionally, we have added a proper Editor.
	/// 
	/// This script is now obsolete as Unity finally unrolled nested prefabs support.
	/// </summary>

	[ExecuteInEditMode]
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class PrefabInstance : MonoBehaviour
	{
		public GameObject prefab;

#if UNITY_EDITOR
		// Struct of all components. Used for edit-time visualization and gizmo drawing
		public struct Thingy
		{
			public Mesh mesh;
			public Matrix4x4 matrix;
			public List<Material> materials;
		}

		[System.NonSerialized] public List<Thingy> things = new List<Thingy>();

		static PrefabInstance()
		{
			PrefabUtility.prefabInstanceUpdated += OnPrefabUpdated;
		}

		static void OnPrefabUpdated(GameObject prefab)
		{
			var instances = GameObject.FindObjectsOfType<PrefabInstance>();
			foreach (var instance in instances) {
				instance.things.Clear();
				instance.Rebuild(instance.gameObject, Matrix4x4.identity, new Stack<GameObject>());
			}
		}

		void OnValidate()
		{
			things.Clear();
			if (enabled)
				Rebuild(prefab, Matrix4x4.identity, new Stack<GameObject>());
		}

#endif


		void OnEnable()
		{
			if (Application.isPlaying) {
				if (prefab) {
					BakeInstance(this);
				} else {
					Debug.LogWarning($"Nested prefab: Missing link to prefab on {name}.", this);
				}
				return;
			}

#if UNITY_EDITOR
			things.Clear();
			if (enabled)
				Rebuild(prefab, Matrix4x4.identity, new Stack<GameObject>());
#endif
		}


#if UNITY_EDITOR

		void Rebuild(GameObject source, Matrix4x4 inMatrix, Stack<GameObject> processed)
		{
			if (!source)
				return;

			if (processed.Contains(source)) {
				Debug.LogError($"Nested prefabs circular dependency: {source.name}", source);
				return;
			}

			processed.Push(source);

			Matrix4x4 baseMat = inMatrix * Matrix4x4.TRS(-source.transform.position, Quaternion.identity, Vector3.one);

			foreach (Renderer mr in source.GetComponentsInChildren(typeof(Renderer), true)) {
				Mesh mesh;
				if (mr is SkinnedMeshRenderer) {
					mesh = ((SkinnedMeshRenderer)mr).sharedMesh;
				} else {
					var meshFilter = mr.GetComponent<MeshFilter>();

					// Example: Text Meshes.
					if (meshFilter == null)
						continue;
					mesh = meshFilter.GetComponent<MeshFilter>().sharedMesh;
				}

				things.Add(new Thingy() {
					mesh = mesh,
					matrix = baseMat * mr.transform.localToWorldMatrix,
					materials = new List<Material>(mr.sharedMaterials)
				});
			}

			foreach (PrefabInstance pi in source.GetComponentsInChildren(typeof(PrefabInstance), true)) {
				if (pi.enabled && pi.gameObject.activeSelf)
					Rebuild(pi.prefab, baseMat * pi.transform.localToWorldMatrix, processed);
			}

			processed.Pop();
		}

		// Editor-time-only update: Draw the meshes so we can see the objects in the scene view
		void Update()
		{
			if (EditorApplication.isPlaying)
				return;
			Matrix4x4 mat = transform.localToWorldMatrix;
			foreach (Thingy t in things)
				for (int i = 0; i < t.materials.Count; i++)
					Graphics.DrawMesh(t.mesh, mat * t.matrix, t.materials[i], gameObject.layer, null, i);
		}

		// Picking logic: Since we don't have gizmos.drawmesh, draw a bounding cube around each thingy
		void OnDrawGizmos() { DrawGizmos(new Color(0, 0, 0, 0)); }
		void OnDrawGizmosSelected() { DrawGizmos(new Color(0, 0, 1, .2f)); }
		void DrawGizmos(Color col)
		{
			if (EditorApplication.isPlaying)
				return;
			Gizmos.color = col;
			Matrix4x4 mat = transform.localToWorldMatrix;
			foreach (Thingy t in things) {
				Gizmos.matrix = mat * t.matrix;
				Gizmos.DrawCube(t.mesh.bounds.center, t.mesh.bounds.size);
			}
		}

		// Baking stuff: Copy in all the referenced objects into the scene on play or build
		[PostProcessScene(-2)]
		public static void OnPostprocessScene()
		{
			foreach (PrefabInstance pi in UnityEngine.Object.FindObjectsOfType(typeof(PrefabInstance)))
				BakeInstance(pi);
		}

#endif

		public static void BakeInstance(PrefabInstance pi)
		{
			if (!pi.prefab || !pi.enabled)
				return;
			pi.enabled = false;

#if UNITY_EDITOR
			GameObject go = PrefabUtility.InstantiatePrefab(pi.prefab) as GameObject;
#else
		GameObject go = GameObject.Instantiate(pi.prefab);
#endif
			Quaternion rot = go.transform.localRotation;
			Vector3 scale = go.transform.localScale;
			go.transform.parent = pi.transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localScale = scale;
			go.transform.localRotation = rot;
			pi.prefab = null;
			foreach (PrefabInstance childPi in go.GetComponentsInChildren<PrefabInstance>())
				BakeInstance(childPi);
		}

	}



#if UNITY_EDITOR
	[CanEditMultipleObjects, CustomEditor(typeof(PrefabInstance))]
	public class PrefabInstanceEditor : Editor
	{
		private Stack<PrefabInstance> _processed = new Stack<PrefabInstance>();

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			for (int i = 0; i < targets.Length; ++i) {
				var source = (PrefabInstance)targets[i];
				_processed.Clear();

				EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
				EditorGUI.indentLevel--;
				DrawPrefabInstances(source, false);
				EditorGUI.indentLevel++;
			}
		}

		private void DrawPrefabInstances(PrefabInstance source, bool drawSource)
		{
			if (drawSource) {
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField(source.name);
				source.prefab = (GameObject)EditorGUILayout.ObjectField(source.prefab, typeof(GameObject), false);

				EditorGUILayout.EndHorizontal();
			}

			if (!source.prefab)
				return;

			if (_processed.Contains(source)) {
				var style = new GUIStyle(EditorStyles.boldLabel);
				style.normal.textColor = Color.red;
				EditorGUILayout.LabelField("^ Circular dependency break ^", style);
				return;
			} else {
				_processed.Push(source);
			}

			EditorGUI.indentLevel++;

			var subInstances = source.prefab.GetComponentsInChildren<PrefabInstance>();
			foreach (var instance in subInstances) {
				DrawPrefabInstances(instance, true);
			}

			if (subInstances.Length > 0) {
				EditorGUILayout.Space();
			}

			EditorGUI.indentLevel--;

			_processed.Pop();
		}
	}
#endif

}
