// Reverse engineered UnityEditor.TransformInspector

using UnityEngine;
using UnityEditor;

namespace DevLocker.Tools
{
	/// <summary>
	/// Adds "P", "R", "S" buttons that reset respectfully position, rotation, scale in the Transform component.
	/// Also adds world position.
	/// 
	/// NOTE: DecoratorEditor is a custom class that uses reflection to do black magic!
	/// </summary>
	[CanEditMultipleObjects, CustomEditor(typeof(Transform))]
	public class TransformResetEditor : DecoratorEditor
	{
		private const float RESET_BUTTON_WIDTH = 18.0f;

		private SerializedProperty positionProperty;
		private SerializedProperty rotationProperty;
		private SerializedProperty scaleProperty;

		public TransformResetEditor()
			: base("TransformInspector")
		{ }

		public void OnEnable()
		{
			positionProperty = serializedObject.FindProperty("m_LocalPosition");
			rotationProperty = serializedObject.FindProperty("m_LocalRotation");
			scaleProperty = serializedObject.FindProperty("m_LocalScale");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.BeginHorizontal();
			{

				EditorGUILayout.BeginVertical(GUILayout.Width(RESET_BUTTON_WIDTH));
				{
					if (GUILayout.Button("P", GUILayout.Width(RESET_BUTTON_WIDTH), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
						positionProperty.vector3Value = Vector3.zero;

					if (GUILayout.Button("R", GUILayout.Width(RESET_BUTTON_WIDTH), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
						rotationProperty.quaternionValue = Quaternion.identity;

					if (GUILayout.Button("S", GUILayout.Width(RESET_BUTTON_WIDTH), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
						scaleProperty.vector3Value = Vector3.one;

					serializedObject.ApplyModifiedProperties();
				}
				EditorGUILayout.EndVertical();


				EditorGUILayout.BeginVertical();
				{
					base.OnInspectorGUI();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();

			var transformTarget = (Transform)target;
			var position = transformTarget.position;
			GUILayout.BeginHorizontal();
			EditorGUILayout.HelpBox("World Pos:", MessageType.None);
			EditorGUILayout.HelpBox($"X: {position.x:0.###}", MessageType.None);
			EditorGUILayout.HelpBox($"Y: {position.y:0.###}", MessageType.None);
			EditorGUILayout.HelpBox($"Z: {position.z:0.###}", MessageType.None);
			GUILayout.EndHorizontal();
		}
	}

}
