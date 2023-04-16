using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DevLocker.RenderUtils
{
	/// <summary>
	/// Draws gizmos over the skinned bones and their links.
	/// Select any SkinnedMeshRenderer to use.
	/// Can click on the gizmos.
	/// </summary>
	[InitializeOnLoad]
	public static class SkinnedBonesGizmos
	{
		private struct TargetBonesData
		{
			public SkinnedMeshRenderer Renderer;
			public Transform[] Bones;
			public BoneSegment[] BoneSegments;

			public bool HasMissingBones;
		}

		private struct BoneSegment
		{
			public Transform Parent;
			public Transform Bone;

			public float Distance;

			public BoneSegment(Transform parent, Transform bone, float distance)
			{
				Parent = parent;
				Bone = bone;
				Distance = distance;
			}
		}

		private static List<TargetBonesData> s_Targets = new List<TargetBonesData>();

		private static readonly Color s_BonesColor = Color.red;
		private static GUIStyle s_BonesMissingLabelStyle;

		public static bool IsActive { get; private set; }
		public static float Size {
			get => s_SizeMultiplier;
			set {
				if (s_SizeMultiplier == value)
					return;

				s_SizeMultiplier = value;
				EditorPrefs.SetFloat(s_PreferenceSize, s_SizeMultiplier);
			}
		}

		private static float s_SizeMultiplier = 1f;
		private const float s_BaseSizeMultiplier = 0.2f;

		private static readonly string s_PreferenceActive = $"{nameof(SkinnedBonesGizmos)}_Active";
		private static readonly string s_PreferenceSize = $"{nameof(SkinnedBonesGizmos)}_Size";

		static SkinnedBonesGizmos()
		{
			RefreshPreferences();
			OnSelectionChanged();
		}

		public static void ToggleActive()
		{
			EditorPrefs.SetBool(s_PreferenceActive, !EditorPrefs.GetBool(s_PreferenceActive, false));
			RefreshPreferences();
			OnSelectionChanged();
			SceneView.RepaintAll();
		}

		private static void RefreshPreferences()
		{
			SceneView.duringSceneGui -= OnSceneGUI;
			Selection.selectionChanged -= OnSelectionChanged;

			IsActive = EditorPrefs.GetBool(s_PreferenceActive, false);
			s_SizeMultiplier = EditorPrefs.GetFloat(s_PreferenceSize, 1f);

			if (IsActive) {
				SceneView.duringSceneGui += OnSceneGUI;
				Selection.selectionChanged += OnSelectionChanged;
			}
		}

		private static void InitStyles()
		{
			s_BonesMissingLabelStyle = new GUIStyle(EditorStyles.helpBox);
			s_BonesMissingLabelStyle.normal.textColor = Color.red;
			s_BonesMissingLabelStyle.font = EditorStyles.boldFont;
			s_BonesMissingLabelStyle.fontSize = 14;
			s_BonesMissingLabelStyle.fontStyle = FontStyle.Bold;
		}


		private static void OnSelectionChanged()
		{
			var allBones = s_Targets.SelectMany(tb => tb.Bones).ToList();

			// Selected a child transform - don't clear the selection gizmos.
			if (Selection.gameObjects.Any(go => allBones.Contains(go.transform)))
				return;

			s_Targets.Clear();

			foreach (var go in Selection.gameObjects) {

				if (PrefabUtility.IsPartOfPrefabAsset(go))
					continue;

				foreach (var renderer in go.GetComponents<SkinnedMeshRenderer>()) {

					if (renderer.bones.Length == 0)
						continue;

					s_Targets.Add(CreateTargetBoneData(renderer));
				}

			}
		}

		private static TargetBonesData CreateTargetBoneData(SkinnedMeshRenderer renderer)
		{
			var bones = renderer.bones;

			bool hasMissingBones = false;
			var boneSegments = new List<BoneSegment>();
			for(int i = 0; i < bones.Length; ++i) {
				Transform bone = bones[i];
				if (bone == null) {
					hasMissingBones = true;
					continue;
				}

				var parent = bone.parent;
				while (parent != null && !bones.Any(b => b == parent)) {
					parent = parent.parent;
				}


				float minDistance = float.MaxValue;

				if (parent == null) {

					foreach (Transform otherBone in bones) {
						if (otherBone == null || bone == otherBone)
							continue;

						float distance = Vector3.Distance(bone.position, otherBone.position);
						if (distance < minDistance) {
							minDistance = distance;
						}
					}

				} else {
					minDistance = Vector3.Distance(parent.position, bone.position);
				}


				if (minDistance == float.MaxValue) {
					minDistance = 0.5f;
				}

				boneSegments.Add(new BoneSegment(parent, bone, minDistance));
			}

			// Create segments with no parent for all end bones so they can also be clicked.
			foreach(Transform bone in bones) {
				if (bone == null)
					continue;

				if (!boneSegments.Any(bs => bs.Parent == bone)) {
					float minDistance = boneSegments.FirstOrDefault(b => b.Bone == bone).Distance;
					boneSegments.Add(new BoneSegment(null, bone, minDistance));
				}
			}

			return new TargetBonesData()
			{
				Renderer = renderer,
				Bones = bones,
				BoneSegments = boneSegments.ToArray(),
				HasMissingBones = hasMissingBones,
			};
		}

		private static void OnSceneGUI(SceneView sceneView)
		{
			if (s_BonesMissingLabelStyle == null) {
				InitStyles();
			}

			for (int i = 0; i < s_Targets.Count; ++i) {

				// Destroyed in the mean time.
				if (s_Targets[i].Renderer == null) {
					s_Targets.RemoveAt(i);
					--i;
					continue;
				}

				DrawBoneHandles(s_Targets[i]);
			}

		}


		private static void DrawBoneHandles(TargetBonesData target)
		{
			Handles.zTest = CompareFunction.Always;

			var bones = target.Bones;

			Handles.color = s_BonesColor;

			bool hasMissingBones = target.HasMissingBones;

			foreach (var boneSegment in target.BoneSegments) {

				if (boneSegment.Bone == null) {
					hasMissingBones = true;
					continue;
				}

				if (boneSegment.Parent) {


					var pos1 = boneSegment.Parent.position;
					var pos2 = boneSegment.Bone.position;
					var dist = pos2 - pos1;

					if (dist == Vector3.zero)
						continue;

					Handles.DrawLine(pos1, pos2);

					var conePos = pos1 + dist / 2f;

					var handleSize = dist.magnitude * s_SizeMultiplier * s_BaseSizeMultiplier;

					if (Handles.Button(conePos, Quaternion.LookRotation(dist.normalized), handleSize, handleSize, Handles.ConeHandleCap)) {
						Selection.activeGameObject = boneSegment.Parent.gameObject;
					}

				} else {

					var handleSie = boneSegment.Distance * s_SizeMultiplier * s_BaseSizeMultiplier;

					if (Handles.Button(boneSegment.Bone.position, Quaternion.identity, handleSie, handleSie, Handles.SphereHandleCap)) {
						Selection.activeGameObject = boneSegment.Bone.gameObject;
					}
				}

			}

			// Bones could be destroyed in any moment (edit or play mode).
			if (hasMissingBones) {
				DrawMissingBonesSign(target);
			}
		}


		private static void DrawMissingBonesSign(TargetBonesData target)
		{
			var bone = target.Bones.FirstOrDefault(b => b != null);
			if (bone == null) {
				bone = target.Renderer.transform;
			}

			Handles.Label(bone.position + Vector3.down * 0.05f, "Missing bones!!!", s_BonesMissingLabelStyle);
		}
	}

}
