using UnityEditor;
using UnityEngine;

// This property drawer adds "C" and "P" button next to your custom class/struct in the inspector.
// "C" button will copy the content, "P" will paste it (override it with the previously copied content).
// There are 3 ways of using it:
//	- Insert [InspectorCopyable] attribute before to the field you want to be copyable. Custom classes/structs only.
//
//	- Make your custom drawer for your classes/structs by inheriting this one:
//		[CustomPropertyDrawer(typeof(MyCustomClass), true)]
//		MyCustomClassPropertyDrawer : InspectorCopyablePropertyDrawer {}
//
//	- Make you custom class/struct inherit from the utility class: InspectorCopyableBasePropertyDrawer. For example:
//		class MyCustomClass : UnityTools.InspectorCopyableBasePropertyDrawer { }
//
namespace DevLocker.Tools
{


	// Your class can inherit from InspectorCopyableBase and will receive copyable functionality.
	// If you don't like your runtime code mangling with this editor class, you can inherit your own property drawer like this one.
	[CustomPropertyDrawer(typeof(InspectorCopyableBase), true)]
	public class InspectorCopyableBasePropertyDrawer : InspectorCopyablePropertyDrawer { }


	// Per field copyable attribute.
	[CustomPropertyDrawer(typeof(InspectorCopyableAttribute))]
	public class InspectorCopyablePropertyDrawer : PropertyDrawer
	{
		private static Object[] _copyTargetObjects;
		private static string _copyTargetPath;
		private static string _copyTargetType;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position = EditorGUITools.FixIndent(position, property);

			const float BUTTON_PADDING = 4.0f;
			const float BUTTON_WIDTH = 21.0f;
			const float BUTTON_HEIGHT = 14.0f;

			var pasteRect = position;
			pasteRect.x += position.width - BUTTON_WIDTH - BUTTON_PADDING;
			pasteRect.width = BUTTON_WIDTH;
			pasteRect.height = BUTTON_HEIGHT;

			var copyRect = pasteRect;
			copyRect.x -= BUTTON_WIDTH + BUTTON_PADDING;

			if (GUI.Button(copyRect, new GUIContent("C", "Copy"))) {
				_copyTargetObjects = property.serializedObject.targetObjects;
				_copyTargetPath = property.propertyPath;
				_copyTargetType = property.type;
			}

			// Disable if generic type is different.
			// NOTE: No inheritance support.
			EditorGUI.BeginDisabledGroup(property.propertyType != SerializedPropertyType.Generic || _copyTargetType != property.type);

			if (GUI.Button(pasteRect, new GUIContent("P", "Paste")) && _copyTargetObjects != null) {

				var sourceProp = new SerializedObject(_copyTargetObjects).FindProperty(_copyTargetPath);
				var destProp = property.Copy();

				var destPropertyPath = destProp.propertyPath;

				while (destProp.Next(true) && sourceProp.Next(true)) {

					// Reached the end of this class/struct members.
					if (!destProp.propertyPath.StartsWith(destPropertyPath))
						break;

					if (destProp.name != sourceProp.name || destProp.propertyType != sourceProp.propertyType) {
						Debug.LogError($"Properties don't match: {destProp.name} != {sourceProp.name}");
						break;
					}

					switch (sourceProp.propertyType) {
						case SerializedPropertyType.Boolean: destProp.boolValue = sourceProp.boolValue; break;
						case SerializedPropertyType.Integer: destProp.intValue = sourceProp.intValue; break;
						case SerializedPropertyType.Float: destProp.floatValue = sourceProp.floatValue; break;
						case SerializedPropertyType.ObjectReference: destProp.objectReferenceValue = sourceProp.objectReferenceValue; break;
						case SerializedPropertyType.String: destProp.stringValue = sourceProp.stringValue; break;
						case SerializedPropertyType.Enum: destProp.enumValueIndex = sourceProp.enumValueIndex; break;
						case SerializedPropertyType.Character: destProp.intValue = sourceProp.intValue; break;

						case SerializedPropertyType.Color: destProp.colorValue = sourceProp.colorValue; break;
						case SerializedPropertyType.LayerMask: destProp.intValue = sourceProp.intValue; break;
						case SerializedPropertyType.AnimationCurve: destProp.animationCurveValue = sourceProp.animationCurveValue; break;

						case SerializedPropertyType.Vector2: destProp.vector2Value = sourceProp.vector2Value; break;
						case SerializedPropertyType.Vector2Int: destProp.vector2IntValue = sourceProp.vector2IntValue; break;
						case SerializedPropertyType.Vector3: destProp.vector3Value = sourceProp.vector3Value; break;
						case SerializedPropertyType.Vector3Int: destProp.vector3IntValue = sourceProp.vector3IntValue; break;
						case SerializedPropertyType.Vector4: destProp.vector4Value = sourceProp.vector4Value; break;
						case SerializedPropertyType.Quaternion: destProp.quaternionValue = sourceProp.quaternionValue; break;

						case SerializedPropertyType.Rect: destProp.rectValue = sourceProp.rectValue; break;
						case SerializedPropertyType.RectInt: destProp.rectIntValue = sourceProp.rectIntValue; break;
						case SerializedPropertyType.Bounds: destProp.boundsValue = sourceProp.boundsValue; break;
						case SerializedPropertyType.BoundsInt: destProp.boundsIntValue = sourceProp.boundsIntValue; break;


						case SerializedPropertyType.ExposedReference: destProp.exposedReferenceValue = sourceProp.exposedReferenceValue; break;

						case SerializedPropertyType.ArraySize: destProp.intValue = sourceProp.intValue; break;

						case SerializedPropertyType.Generic: /* Do nothing... children are the one processed. */ break;

						default: Debug.LogError($"Not supported type yet: {destProp.propertyType} for field {destProp.name}"); return;
					}


				}

				destProp.serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.EndDisabledGroup();

			EditorGUI.PropertyField(position, property, label, true);
		}
	}
}
