using UnityEditor;
using UnityEngine;

namespace DevLocker.Animations
{
	[CustomEditor(typeof(PlayAnimation))]
	public class PlayAnimationEditor : Editor
	{
		private bool previewActive = false;
		private float previewTime = 0.0f;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			var playAnimation = (PlayAnimation)target;

			EditorGUI.BeginDisabledGroup(playAnimation.Clip == null);
			{
				bool wasChanged = GUI.changed;
				previewTime = EditorGUILayout.Slider("Preview:", previewTime, 0.0f, 1.0f);

				if (GUI.changed && !wasChanged && playAnimation.Clip) {
					previewActive = true;

					playAnimation.Clip.SampleAnimation(playAnimation.gameObject, previewTime * playAnimation.Clip.length);
				}

				if (previewActive && GUIUtility.hotControl == 0) {
					previewActive = false;
					previewTime = 0.0f;

					playAnimation.Clip.SampleAnimation(playAnimation.gameObject, 0.0f);
				}
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}
