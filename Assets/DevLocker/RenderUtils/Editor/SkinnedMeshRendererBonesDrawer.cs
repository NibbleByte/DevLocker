using UnityEngine;
using UnityEditor;
using DevLocker.Tools;

namespace DevLocker.RenderUtils
{
	/// <summary>
	/// Draws "Show All Bones" button at the end of SkinnedMeshRenderer component in the inspector.
	/// Clicking it will show the list of bones the mesh is skinned to.
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SkinnedMeshRenderer))]
	public class SkinnedMeshRendererBonesDrawer : DecoratorEditor
	{
		private Transform[] _bones = null;

		public SkinnedMeshRendererBonesDrawer()
			: base("SkinnedMeshRendererEditor")
		{ }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (_bones != null) {
				foreach (var bone in _bones) {
					EditorGUILayout.ObjectField(bone, typeof(Transform), true);
				}
				if (GUILayout.Button("Hide All Bones")) {
					_bones = null;
				}
			} else {
				if (GUILayout.Button("Show All Bones")) {
					_bones = ((SkinnedMeshRenderer)target).bones;
				}
			}

		}

		public void OnSceneGUI()
		{
			CallInspectorMethod("OnSceneGUI", false);
		}
	}
}
