using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DevLocker.Audio.Editor
{
	[CustomEditor(typeof(AudioSourceIsolateAndFollow))]
	[CanEditMultipleObjects]
	public class AudioSourceIsolateAndFollowEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawDefaultInspector();

			EditorGUILayout.Space();

			if (Application.isPlaying) {
				EditorGUI.BeginDisabledGroup(true);
				foreach (var t in targets.OfType<AudioSourceIsolateAndFollow>()) {
					if (t != null) {
						EditorGUILayout.ObjectField(t.Link, typeof(AudioSourceIsolateAndFollow), true);
					}
				}
				EditorGUI.EndDisabledGroup();
			}
		}
	}
}
