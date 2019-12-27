using UnityEngine;
using UnityEditor;

namespace DevLocker.Animations.AnimatorExpositor
{
	[CustomPropertyDrawer(typeof(AnimatorParamAttribute))]
	public class AnimParamPropertyDrawer : PropertyDrawer
	{
		// Cache
		private GUIContent[] _options = new GUIContent[0];

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var target = (MonoBehaviour) property.serializedObject.targetObject;
			var animator = target.GetComponent<IAnimatorProvider>().GetAnimator();

			if (!animator) {
				EditorGUI.LabelField(position, "Cannot read animator parameters. Animator component is missing or disabled.");
				return;
			}

			if (animator.runtimeAnimatorController == null) {
				EditorGUI.LabelField(position, "No controller assigned to the Animator.");
				return;
			}

			if (animator.parameterCount == 0) {
				EditorGUI.LabelField(position, "No parameters available.");

				// HACK: while not playing, changing parameters & saving can cause the editor to unload the controller.
				//		 Fix it by re-enabling the Animator.
				animator.enabled = !animator.enabled;
				animator.enabled = !animator.enabled;

				return;
			}

			var animParams = animator.parameters;
			if (_options.Length != animParams.Length) {
				_options = new GUIContent[animParams.Length];
			}


			int selectedIndex = -1;
			for (int i = 0; i < animParams.Length; ++i) {
				var param = animParams[i];
				_options[i] = new GUIContent(param.name);
				if (param.nameHash == property.intValue) {
					selectedIndex = i;
				}
			}

			selectedIndex = EditorGUI.Popup(position, label, selectedIndex, _options);

			if (selectedIndex >= 0) {
				property.intValue = animParams[selectedIndex].nameHash;
			}
		}
	}

}
