using UnityEngine;
using UnityEditor;
using DevLocker.Tools;
using System.Linq;
using System.Globalization;

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
		private struct BoneInfo
		{
			public Transform Bone;
			public float Weight;
			public int References;
		}

		private BoneInfo[] _boneInfos = null;
		private int _invalidIndecesFound = 0;

		private bool _showEmptyWeightBones = false;

		public SkinnedMeshRendererBonesDrawer()
			: base("SkinnedMeshRendererEditor")
		{ }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();

			if (_boneInfos != null) {
				if (_invalidIndecesFound > 0) {
					EditorGUILayout.HelpBox($"{_invalidIndecesFound} invalid bone indices found!", MessageType.Error);
				}

				bool hasEmptyWeightBones = false;
				int displayedBones = 0;

				foreach (var boneInfo in _boneInfos) {
					if (boneInfo.References == 0) {
						hasEmptyWeightBones = true;
					}

					if (_showEmptyWeightBones || boneInfo.References > 0) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.ObjectField(boneInfo.Bone, typeof(Transform), true);
						EditorGUILayout.TextField(boneInfo.References.ToString(CultureInfo.InvariantCulture), GUILayout.MaxWidth(30f));
						EditorGUILayout.TextField(boneInfo.Weight.ToString("0.##"), GUILayout.MaxWidth(40f));
						EditorGUILayout.EndHorizontal();
						displayedBones++;
					}
				}

				GUILayout.BeginHorizontal();

				GUILayout.Label("Count: ", GUILayout.Width(50f));
				EditorGUILayout.TextField(displayedBones.ToString(), GUILayout.Width(32f));
				GUILayout.Space(15f);

				if (hasEmptyWeightBones) {
					_showEmptyWeightBones = EditorGUILayout.Toggle("Show no weight bones", _showEmptyWeightBones);
				}

				GUILayout.EndHorizontal();

				if (GUILayout.Button("Hide All Bones")) {
					_boneInfos = null;
					_invalidIndecesFound = 0;
					_showEmptyWeightBones = false;
				}
			} else {
				if (GUILayout.Button("Show All Bones")) {
					var renderer = (SkinnedMeshRenderer) target;
					var mesh = renderer.sharedMesh;

					if (mesh == null)
						return;

					_boneInfos = renderer.bones
						.Select(b => new BoneInfo() { Bone = b})
						.ToArray();

#if UNITY_2019
					var nativeWeights = mesh.GetAllBoneWeights();
					foreach (var boneWeight1 in nativeWeights) {
						_boneInfos[boneWeight1.boneIndex].References++;
						_boneInfos[boneWeight1.boneIndex].Weight += boneWeight1.weight;
					}
#else
					var boneWeights = mesh.boneWeights;
					foreach (var boneWeight in boneWeights) {

						if (boneWeight.boneIndex0 < _boneInfos.Length) {
							_boneInfos[boneWeight.boneIndex0].References++;
							_boneInfos[boneWeight.boneIndex0].Weight += boneWeight.weight0;
						} else {
							_invalidIndecesFound++;
						}

						if (boneWeight.boneIndex1 < _boneInfos.Length) {
							_boneInfos[boneWeight.boneIndex1].References++;
							_boneInfos[boneWeight.boneIndex1].Weight += boneWeight.weight1;
						} else {
							_invalidIndecesFound++;
						}

						if (boneWeight.boneIndex2 < _boneInfos.Length) {
							_boneInfos[boneWeight.boneIndex2].References++;
							_boneInfos[boneWeight.boneIndex2].Weight += boneWeight.weight2;
						} else {
							_invalidIndecesFound++;
						}

						if (boneWeight.boneIndex3 < _boneInfos.Length) {
							_boneInfos[boneWeight.boneIndex3].References++;
							_boneInfos[boneWeight.boneIndex3].Weight += boneWeight.weight3;
						} else {
							_invalidIndecesFound++;
						}
					}
#endif
				}
			}

			if (GUILayout.Button("Toggle Bone Gizmos")) {
				SkinnedBonesGizmos.ToggleActive();
			}

			if (SkinnedBonesGizmos.IsActive) {
				SkinnedBonesGizmos.Size = EditorGUILayout.Slider("Bones Size", SkinnedBonesGizmos.Size, 0f, 5f);
			}
		}

		public void OnSceneGUI()
		{
			CallInspectorMethod("OnSceneGUI", false);
		}
	}
}
