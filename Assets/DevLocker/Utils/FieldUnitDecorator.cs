using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DevLocker.Utils
{
	/// <summary>
	/// Display enum as a bit mask drop-down menu in the editor.
	/// </summary>
	public class FieldUnitDecorator : PropertyAttribute
	{
		public string UnitSuffix { get; }

		public string Tooltip;

		public float MinValue = float.MinValue;
		public float MaxValue = float.MaxValue;

		public FieldUnitDecorator(string unitSuffix)
		{
			UnitSuffix = unitSuffix;
		}

		public FieldUnitDecorator(string unitSuffix, string tooltip)
		{
			UnitSuffix = unitSuffix;
			Tooltip = tooltip;
		}
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(FieldUnitDecorator))]
	internal class FieldUnitDecoratorPropertyDrawer : PropertyDrawer
	{
		private GUIStyle m_UnitStyle;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(position, property, label, true);

			var fieldUnitDecorator = (FieldUnitDecorator) attribute;

			switch (property.propertyType) {
				case SerializedPropertyType.Float:
					if (property.floatValue < fieldUnitDecorator.MinValue) {
						property.floatValue = fieldUnitDecorator.MinValue;
					}
					if (property.floatValue > fieldUnitDecorator.MaxValue) {
						property.floatValue = fieldUnitDecorator.MaxValue;
					}
					break;

				case SerializedPropertyType.Integer:
					if (property.intValue < fieldUnitDecorator.MinValue) {
						property.intValue = (int) fieldUnitDecorator.MinValue;
					}
					if (property.intValue > fieldUnitDecorator.MaxValue) {
						property.intValue = (int) fieldUnitDecorator.MaxValue;
					}
					break;
			}

			if (m_UnitStyle == null) {
				m_UnitStyle = new GUIStyle(EditorStyles.label);
				m_UnitStyle.alignment = TextAnchor.MiddleRight;
				m_UnitStyle.padding.right = 4;
				m_UnitStyle.padding.bottom = 1;
			}

			GUI.Label(position, new GUIContent(fieldUnitDecorator.UnitSuffix, fieldUnitDecorator.Tooltip), m_UnitStyle);
		}
	}
#endif

}
