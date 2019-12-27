using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

namespace DevLocker.Animations.AnimatorExpositor
{
	[CustomPropertyDrawer(typeof(AnimatorState))]
	public class AnimStatePropertyDrawer : PropertyDrawer
	{
		// Cache
		private List<GUIContent> _options = new List<GUIContent>();

		private List<AnimatorState> _optionsStates = new List<AnimatorState>();

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

			var controller = animator.runtimeAnimatorController as AnimatorController;
			if (controller == null) {
				EditorGUI.LabelField(position, "Controller is AnimatorOverrideController. This is not supported yet.");
				return;
			}


			var selectedLayerProp = property.FindPropertyRelative("Layer");
			var selectedStateProp = property.FindPropertyRelative("State");

			int selectedIndex = -1;
			var layers = controller.layers;
			for (int layerIndex = 0; layerIndex < layers.Length; ++layerIndex) {
				var layer = layers[layerIndex];
				var states = layer.stateMachine.states;

				foreach (var state in states) {
					_options.Add(new GUIContent($"{state.state.name} [{layer.name}]"));

					var animState = new AnimatorState() {Layer = layerIndex, State = state.state.nameHash};
					_optionsStates.Add(animState);

					if (animState.State == selectedStateProp.intValue) {
						selectedIndex = _options.Count - 1;
					}
				}
			}


			selectedIndex = EditorGUI.Popup(position, label, selectedIndex, _options.ToArray());

			if (selectedIndex >= 0) {
				selectedLayerProp.intValue = _optionsStates[selectedIndex].Layer;
				selectedStateProp.intValue = _optionsStates[selectedIndex].State;
			}

			_options.Clear();
			_optionsStates.Clear();
		}
	}

}
