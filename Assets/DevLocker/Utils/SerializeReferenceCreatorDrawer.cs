#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
#endif

using UnityEngine;


namespace DevLocker.Utils
{
#if UNITY_EDITOR
	/// <summary>
	/// Use this class with [SerializeReference] attribute to have a "Create" button next to such fields.
	/// By default [SerializeReference] fields display empty data in the inspector - there is no UI to create data instance.
	/// Pressing the "Create" button will ask you to select the type to be instantiated - any class that inherits or implements the target field type.
	///
	/// Create custom drawer of your class that inherits from this one by specifying the target class type itself as <typeparamref name="T"/>.
	/// It works properly if [SerializeReference] is not present.
	///
	/// If you want to have custom drawing of the data, override the <see cref="GetPropertyHeight_Custom(SerializedProperty, GUIContent)"/> and
	/// <see cref="OnGUI_Custom(Rect, SerializedProperty, GUIContent, bool)"/>.
	///
	/// Example:
	/// [CustomPropertyDrawer(typeof(MyClass))]
	/// public class MyClassDrawer : SerializeReferenceCreatorDrawer<MyClass>
	/// {
	/// }
	///
	/// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	/// IMPORTANT: if you use [SerializeReference] with lists and you want to avoid duplicated references to the same instance,
	///			   use the <see cref="SerializeReferenceValidation.ClearDuplicateReferences(UnityEngine.Object)"/> from this file!
	///	!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	///
	/// </summary>
	/// <typeparam name="T">Class type to be drawn.</typeparam>
	public class SerializeReferenceCreatorDrawer<T> : PropertyDrawer
	{
		protected const float s_ClearReferenceButtonWidth = 60f;

		protected GUIStyle m_TypeLabelStyle;

		private double m_TypeLabelClickTime;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.ManagedReference && string.IsNullOrEmpty(property.managedReferenceFullTypename)) {
				return EditorGUIUtility.singleLineHeight;
			} else {
				return GetPropertyHeight_Custom(property, label);
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (m_TypeLabelStyle == null) {
				m_TypeLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
				m_TypeLabelStyle.wordWrap = false;
				m_TypeLabelStyle.alignment = TextAnchor.MiddleRight;
			}

			// This property doesn't have the [SerializeReference] attribute.
			if (property.propertyType != SerializedPropertyType.ManagedReference) {
				OnGUI_Custom(position, property, label, false);
				return;
			}

			var buttonRect = position;
			buttonRect.x += buttonRect.width - s_ClearReferenceButtonWidth;
			buttonRect.width = s_ClearReferenceButtonWidth;
			buttonRect.height = EditorGUIUtility.singleLineHeight;

			bool isReferenceEmpty = string.IsNullOrEmpty(property.managedReferenceFullTypename);

			if (isReferenceEmpty) {
				label = EditorGUI.BeginProperty(position, label, property);

				EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

				EditorGUI.EndProperty();

				if (QuickButton(buttonRect, Color.green, "Create")) {

					//List<Type> availableTypes = AppDomain.CurrentDomain.GetAssemblies()
					//	.SelectMany(assembly => assembly.GetTypes())
					//	.Where(type => type.IsClass && !type.IsAbstract && !type.IsValueType)
					//	//.Where(type => type.GetCustomAttribute<SerializableAttribute>(true) != null)	// TODO: Doesn't find parent attributes.
					//	.Where(type => typeof(T).IsAssignableFrom(type))
					//	.Where(type => type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null)
					//	.ToList()
					//	;
					var availableTypes = TypeCache.GetTypesDerivedFrom<T>().ToList();


					availableTypes.Sort((a, b) => a.Name.CompareTo(b.Name));

					var menu = new GenericMenu();
					foreach(Type type in availableTypes) {
						if (type.IsAbstract || type.ContainsGenericParameters)
							continue;

						menu.AddItem(new GUIContent(type.Name), false, OnTypeSelected, new KeyValuePair<SerializedProperty, Type>(property, type));
					}

					menu.ShowAsContext();
				}

			} else {
				OnGUI_Custom(position, property, label, true);
			}
		}

		private void OnTypeSelected(object obj)
		{
			var pair = (KeyValuePair<SerializedProperty, Type>)obj;

			pair.Key.managedReferenceValue = Activator.CreateInstance(pair.Value);
			pair.Key.serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Override this to change the height of the displayed data.
		/// </summary>
		protected virtual float GetPropertyHeight_Custom(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property);
		}

		/// <summary>
		/// Override to customize how data is displayed.
		/// NOTE: To have a clear button, make sure you call <see cref="DrawClearButton(SerializedProperty, Rect, Color, string)"/>!
		/// </summary>
		protected virtual void OnGUI_Custom(Rect position, SerializedProperty property, GUIContent label, bool isManagedReference)
		{
			if (isManagedReference) {
				var barRect = position;
				barRect.height = EditorGUIUtility.singleLineHeight;

				OnGUI_CustomReferenceBar(barRect, property);
			}

			OnGUI_CustomData(position, property, label);
		}

		/// <summary>
		/// Override this to change how the header bar with reference controls looks like.
		/// NOTE: To have a clear button, make sure you call <see cref="DrawClearButton(SerializedProperty, Rect, Color, string)"/>!
		/// </summary>
		protected virtual void OnGUI_CustomReferenceBar(Rect barRect, SerializedProperty property)
		{
			var clearButtonRect = barRect;
			clearButtonRect.x += clearButtonRect.width - s_ClearReferenceButtonWidth;
			clearButtonRect.width = s_ClearReferenceButtonWidth;

			var typeLabelRect = barRect;
			typeLabelRect.width -= s_ClearReferenceButtonWidth;

			DrawManagedTypeLabel(typeLabelRect, property, Color.white * 0.8f);

			DrawClearButton(clearButtonRect, property, Color.red, "Clear");
		}

		/// <summary>
		/// Override this to change how the referenced object looks like.
		/// </summary>
		protected virtual void OnGUI_CustomData(Rect position, SerializedProperty property, GUIContent label)
		{
			//label = EditorGUI.BeginProperty(position, label, property);
			//EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			EditorGUI.PropertyField(position, property, label, true);

			//EditorGUI.EndProperty();
		}

		/// <summary>
		/// Draw label with the name of the managed type.
		/// If type name contains underscores "_", it will be truncated to the first underscore.
		/// </summary>
		protected void DrawManagedTypeLabel(Rect typeLabelRect, SerializedProperty property, Color color, GUIStyle style = null)
		{
			if (style == null) {
				style = m_TypeLabelStyle;
			}

			// If missing, start index is 0, so it's ok.
			int typeIndex = property.managedReferenceFullTypename.LastIndexOf(".") + 1;

			// End should be the first underscore or string length.
			int typeEndIndex = property.managedReferenceFullTypename.IndexOf("_", typeIndex + 1) - 1;
			if (typeEndIndex < 0) {
				typeEndIndex = property.managedReferenceFullTypename.Length - 1;
			}

			int assemblyIndex = property.managedReferenceFullTypename.IndexOf(" ") + 1;	// Again, missing will produce 0.

			GUIContent typeName = new GUIContent(property.managedReferenceFullTypename.Substring(typeIndex, typeEndIndex - typeIndex + 1));
			typeName.tooltip = $"Full managed type name:\n{property.managedReferenceFullTypename.Substring(assemblyIndex)}";

			Vector2 labelSize = style.CalcSize(typeName);
			typeLabelRect.x += typeLabelRect.width - labelSize.x;
			typeLabelRect.width = labelSize.x;

			var prevColor = GUI.color;
			GUI.color = color;


			if (GUI.Button(typeLabelRect, typeName, style)) {
				MonoScript asset = AssetDatabase.FindAssets($"t:script {property.managedReferenceFullTypename.Substring(typeIndex)}")
							.Select(AssetDatabase.GUIDToAssetPath)
							.Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
							.FirstOrDefault();

				if (asset) {
					// Detect double-click as Event.current.clickCount is weird.
					if (EditorApplication.timeSinceStartup - m_TypeLabelClickTime < 0.3) {
						AssetDatabase.OpenAsset(asset);
					} else {
						EditorGUIUtility.PingObject(asset);
					}

					m_TypeLabelClickTime = EditorApplication.timeSinceStartup;
					GUIUtility.ExitGUI();
				}
			}
			EditorGUIUtility.AddCursorRect(typeLabelRect, MouseCursor.Link);

			GUI.color = prevColor;
		}

		/// <summary>
		/// Draw the "Clear" button the way you want.
		/// NOTE: if button is pressed, <see cref="GUIUtility.ExitGUI"/> is called, preventing the code from resuming with empty reference.
		/// </summary>
		protected void DrawClearButton(Rect buttonRect, SerializedProperty property, Color color, string text)
		{
			if (QuickButton(buttonRect, color, text)) {
				property.managedReferenceValue = null;
				property.serializedObject.ApplyModifiedProperties();
				GUIUtility.ExitGUI();
			}
		}

		private bool QuickButton(Rect buttonRect, Color color, string text)
		{
			var prevBackground = GUI.backgroundColor;
			bool clicked = false;

			GUI.backgroundColor = color;
			if (GUI.Button(buttonRect, text)) {
				clicked = true;
			}

			GUI.backgroundColor = prevBackground;

			return clicked;
		}
	}
#endif

	/// <summary>
	/// When you use [SerializeReference] on a list, if user adds new elements or duplicates existing ones, the result will be duplicating the reference, not the instance.
	/// This tool will properly duplicate all serialized references inside a unity object.
	/// Just call it in your OnValidate() method.
	///
	/// https://discussions.unity.com/t/duplicating-a-serializereference-property/250097/3
	/// By Moe_Baker
	/// </summary>
	public static class SerializeReferenceValidation
	{
		/// <summary>
		/// Call this in your OnValidate() method to ensure no [SerializeReference] references are duplicated.
		/// </summary>
		public static void ClearDuplicateReferences(UnityEngine.Object target)
		{
#if UNITY_EDITOR
			var managedObject = new SerializedObject(target);

			var iterator = managedObject.GetIterator();

			while (iterator.NextVisible(true)) {
				if (iterator.propertyType is not SerializedPropertyType.ManagedReference)
					continue;

				if (string.IsNullOrEmpty(iterator.managedReferenceFullTypename) || iterator.managedReferenceValue == null)
					continue;

				if (s_References.Add(iterator.managedReferenceValue))
					continue;

				iterator.managedReferenceValue = DuplicateReference(iterator.managedReferenceValue);
			}

			s_References.Clear();

			managedObject.ApplyModifiedProperties();
#endif
		}

#if UNITY_EDITOR
		private static object DuplicateReference(object original)
		{
			// Yeah, not the most optimal solution, but not many options that Unity allows us

			var type = original.GetType();

			//Json serialization uses the same serialization engine that the inspector uses
			//Ie, we will get all the values we are expecting
			var json = JsonUtility.ToJson(original);

			var clone = JsonUtility.FromJson(json, type);

			return clone;
		}

		private static HashSet<object> s_References = new(200, ReferenceEqualityComparer.Default);

		private class ReferenceEqualityComparer : IEqualityComparer<object>
		{
			// Ensure we always do a reference check
			public new bool Equals(object x, object y) => ReferenceEquals(x, y);

			// Default hashcode implementation of the type is good enough
			// I wanted to use the CLRs' internal hashcode mechanism, but I couldn't find a public API for it
			public int GetHashCode(object obj) => obj.GetHashCode();

			public static ReferenceEqualityComparer Default { get; } = new ReferenceEqualityComparer();
		}
#endif

	}

}
