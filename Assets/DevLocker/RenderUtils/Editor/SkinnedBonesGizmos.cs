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
			public float AverageDistance;
		}

		private struct BoneSegment
		{
			public Transform Bone1;
			public Transform Bone2;

			public BoneSegment(Transform bone1, Transform bone2)
			{
				Bone1 = bone1;
				Bone2 = bone2;
			}
		}

		private static List<TargetBonesData> _targets = new List<TargetBonesData>();

		private static readonly Color BONE_LINE_COLOR = Color.red;
		private static readonly Color BONE_SPHERE_COLOR = Color.cyan;
		private static GUIStyle BONE_SPHERE_LABEL_STYLE;
		private static GUIStyle BONE_MISSING_LABEL_STYLE;

		private static readonly string PreferenceActive = $"{nameof(SkinnedBonesGizmos)}_Active";

		static SkinnedBonesGizmos()
		{
			RefreshPreferences();
		}

		public static void ToggleActive()
		{
			EditorPrefs.SetBool(PreferenceActive, !EditorPrefs.GetBool(PreferenceActive, false));
			RefreshPreferences();
			OnSelectionChanged();
			SceneView.RepaintAll();
		}

		private static void RefreshPreferences()
		{
			SceneView.duringSceneGui -= OnSceneGUI;
			Selection.selectionChanged -= OnSelectionChanged;

			if (EditorPrefs.GetBool(PreferenceActive, false)) {
				SceneView.duringSceneGui += OnSceneGUI;
				Selection.selectionChanged += OnSelectionChanged;
			}
		}

		private static void OnSelectionChanged()
		{
			_targets.Clear();

			foreach (var go in Selection.gameObjects) {

				if (PrefabUtility.IsPartOfPrefabAsset(go))
					continue;

				foreach (var renderer in go.GetComponents<SkinnedMeshRenderer>()) {

					if (renderer.bones.Length == 0)
						continue;

					_targets.Add(CreateTargetBoneData(renderer));
				}

			}
		}

		private static TargetBonesData CreateTargetBoneData(SkinnedMeshRenderer renderer)
		{
			var bones = renderer.bones;

			// Will be handled again in drawing.
			if (bones.Any(b => b == null)) {
				return new TargetBonesData()
				{
					Renderer = renderer,
					Bones = bones,
					BoneSegments = new BoneSegment[0],
					AverageDistance = 0.0f
				};
			}


			var boneSegments = new List<BoneSegment>();
			foreach (var bone in bones) {

				// The "#" check is specific for our Phoenix Point game, as we scatter (re-parent) bones around the rig.
				var parent = bone.name.StartsWith("#") ? bone.parent.parent : bone.parent;
				while (parent != null && !bones.Any(b => b == parent || (b.name.StartsWith("#") && b.parent == parent))) {
					parent = parent.parent;
				}

				if (parent != null) {
					boneSegments.Add(new BoneSegment(parent, bone));
				}

			}


			float averageDist = 0.0f;

			if (boneSegments.Count > 1) {
				int distCount = 0;
				foreach (var boneSegment in boneSegments) {

					var dist = Vector3.Distance(boneSegment.Bone1.position, boneSegment.Bone2.position);
					if (dist < 0.001f)
						continue;

					averageDist += Vector3.Distance(boneSegment.Bone1.position, boneSegment.Bone2.position);
					distCount++;
				}
				averageDist = averageDist / distCount;
			}


			return new TargetBonesData()
			{
				Renderer = renderer,
				Bones = bones,
				BoneSegments = boneSegments.ToArray(),
				// If animated, AverageDistance would remain static, but it is an optimization. Maybe not needed?
				AverageDistance = averageDist
			};
		}

		private static void InitStyles()
		{
			BONE_SPHERE_LABEL_STYLE = new GUIStyle();
			BONE_SPHERE_LABEL_STYLE.alignment = TextAnchor.MiddleCenter;
			BONE_SPHERE_LABEL_STYLE.border = new RectOffset();
			//BONE_SPHERE_LABEL_STYLE.contentOffset = new Vector2(-0f, -3f);
			BONE_SPHERE_LABEL_STYLE.margin = new RectOffset();
			BONE_SPHERE_LABEL_STYLE.padding = new RectOffset();
			BONE_SPHERE_LABEL_STYLE.normal.textColor = Color.black;
			BONE_SPHERE_LABEL_STYLE.fontSize = 10;

			BONE_MISSING_LABEL_STYLE = new GUIStyle(EditorStyles.helpBox);
			BONE_MISSING_LABEL_STYLE.normal.textColor = Color.red;
			BONE_MISSING_LABEL_STYLE.font = EditorStyles.boldFont;
			BONE_MISSING_LABEL_STYLE.fontSize = 14;
			BONE_MISSING_LABEL_STYLE.fontStyle = FontStyle.Bold;
		}

		private static void OnSceneGUI(SceneView sceneView)
		{
			if (BONE_SPHERE_LABEL_STYLE == null) {
				InitStyles();
			}

			for (int i = 0; i < _targets.Count; ++i) {

				// Destroyed in the mean time.
				if (_targets[i].Renderer == null) {
					_targets.RemoveAt(i);
					--i;
					continue;
				}

				DrawBoneHandles(_targets[i]);
			}

		}


		private static void DrawBoneHandles(TargetBonesData target)
		{

			Handles.zTest = CompareFunction.Always;

			var bones = target.Bones;

			// Bones could be destroyed in any moment (edit or play mode).
			if (bones.Any(b => b == null)) {
				DrawMissingBonesSign(target);
				return;
			}

			// Single bone is a special case.
			if (bones.Length <= 1) {
				if (bones.Length == 1) {
					DrawLoneBone(bones[0]);
				}
				return;
			}

			if (target.AverageDistance < 0.001f) {
				DrawLoneBone(bones[0]);
				return;
			}

			// TODO: Draw lines from and to only if parented?

			// Draw lines and cones along the bones.
			{
				Handles.color = BONE_LINE_COLOR;
				foreach (var boneSegment in target.BoneSegments) {

					var pos1 = boneSegment.Bone1.position;
					var pos2 = boneSegment.Bone2.position;
					var dist = pos2 - pos1;

					if (dist == Vector3.zero)
						continue;

					Handles.DrawLine(pos1, pos2);

					var conePos = pos1 + dist / 2f;
					Handles.ConeHandleCap(0, conePos, Quaternion.LookRotation(dist.normalized), HandleSize(conePos, target.AverageDistance, 0.5f), Event.current.type);

				}
			}

			Handles.color = BONE_SPHERE_COLOR;
			for (int i = 0; i < bones.Length; ++i) {
				var bone = bones[i];
				var handleSie = HandleSize(bone.position, target.AverageDistance);

				if (Handles.Button(bone.position, Quaternion.identity, handleSie, handleSie, Handles.SphereHandleCap)) {
					Selection.activeGameObject = bone.gameObject;
				}

				BONE_SPHERE_LABEL_STYLE.contentOffset = i < 10 ? new Vector2(-0.5f, -3f) : new Vector2(-3f, -3f);

				Handles.Label(bone.position, i.ToString(), BONE_SPHERE_LABEL_STYLE);
			}
		}


		private static void DrawMissingBonesSign(TargetBonesData target)
		{
			var bone = target.Bones.FirstOrDefault(b => b != null);
			if (bone == null) {
				bone = target.Renderer.transform;
			}

			Handles.Label(bone.position, "Missing bones!!!", BONE_MISSING_LABEL_STYLE);
		}

		private static void DrawLoneBone(Transform bone)
		{
			var handleSie = HandleSize(bone.position, 0.5f);

			Handles.color = BONE_SPHERE_COLOR;
			if (Handles.Button(bone.position, Quaternion.identity, handleSie, handleSie, Handles.SphereHandleCap)) {
				Selection.activeGameObject = bone.gameObject;
			}
		}

		private static float HandleSize(Vector3 position, float averageDist, float modifier = 1.0f)
		{
			return 0.3f * averageDist * modifier;
		}
	}

}
