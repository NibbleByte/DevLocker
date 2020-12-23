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

		private bool _showEmptyWeightBones = false;

		public SkinnedMeshRendererBonesDrawer()
			: base("SkinnedMeshRendererEditor")
		{ }

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (_boneInfos != null) {
				bool hasEmptyWeightBones = false;

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
					}
				}

				if (hasEmptyWeightBones) {
					_showEmptyWeightBones = EditorGUILayout.Toggle("Show no weight bones", _showEmptyWeightBones);
				}

				if (GUILayout.Button("Hide All Bones")) {
					_boneInfos = null;
					_showEmptyWeightBones = false;
				}
			} else {
				if (GUILayout.Button("Show All Bones")) {
					var renderer = (SkinnedMeshRenderer)target;
					var mesh = renderer.sharedMesh;

					_boneInfos = renderer.bones
						.Select(b => new BoneInfo() { Bone = b })
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
						_boneInfos[boneWeight.boneIndex0].References++;
						_boneInfos[boneWeight.boneIndex0].Weight += boneWeight.weight0;

						_boneInfos[boneWeight.boneIndex1].References++;
						_boneInfos[boneWeight.boneIndex1].Weight += boneWeight.weight1;

						_boneInfos[boneWeight.boneIndex2].References++;
						_boneInfos[boneWeight.boneIndex2].Weight += boneWeight.weight2;

						_boneInfos[boneWeight.boneIndex3].References++;
						_boneInfos[boneWeight.boneIndex3].Weight += boneWeight.weight3;
					}
#endif
				}
			}

		}

		public void OnSceneGUI()
		{
			CallInspectorMethod("OnSceneGUI", false);
		}
	}
}
