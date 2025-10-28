using UnityEngine;
using System.Reflection;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		var methods = target.GetType()
			.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		foreach (var method in methods)
		{
			var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();
			if (buttonAttribute != null)
			{
				string label = string.IsNullOrEmpty(buttonAttribute.Label) ? method.Name : buttonAttribute.Label;

				if (GUILayout.Button(label))
				{
					method.Invoke(target, null);
				}
			}

		}

		var fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		foreach (var field in fields)
		{
			var previewAttr = field.GetCustomAttribute<TexturePreviewAttribute>();
			if (previewAttr != null && typeof(Texture).IsAssignableFrom(field.FieldType))
			{
				// Ensure field is serialized
				bool isSerialized = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
				if (!isSerialized) continue;

				Texture texture = field.GetValue(target) as Texture;
				if (texture != null)
				{
					GUILayout.Space(10);
					GUILayout.Label($"{field.Name} Preview", EditorStyles.boldLabel);

					float aspect = (float)texture.width / texture.height;
					float width = Mathf.Min(previewAttr.MaxWidth, EditorGUIUtility.currentViewWidth - 40);
					float height = width / aspect;

					Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false));
					EditorGUI.DrawPreviewTexture(rect, texture);
				}
			}
		}
	}
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		GUI.enabled = false; // Disable editing
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true; // Re-enable editing
	}
}

[CustomPropertyDrawer(typeof(ConditionalBaseAttribute), true)]
    public class ConditionalDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldShow(property)
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property)) return;
            EditorGUI.PropertyField(position, property, label, true);
        }

        private bool ShouldShow(SerializedProperty property)
        {
            var attr = (ConditionalBaseAttribute)attribute;
            var conditionProp = FindConditionProperty(property, attr.ConditionField);

            if (conditionProp == null)
                return true; // If condition not found, default to showing

            bool conditionMet = false;

            if (!attr.UseEnumComparison)
            {
                if (conditionProp.propertyType == SerializedPropertyType.Boolean)
                    conditionMet = conditionProp.boolValue == attr.ExpectedBool;
            }
            else
            {
                if (conditionProp.propertyType == SerializedPropertyType.Enum)
                    conditionMet = conditionProp.enumValueIndex == attr.ExpectedEnumValue;
                else if (conditionProp.propertyType == SerializedPropertyType.Integer)
                    conditionMet = conditionProp.intValue == attr.ExpectedEnumValue;
            }

            // If Inverse = true (ShowIf), we show when conditionMet is true
            // If Inverse = false (HideIf), we show when conditionMet is false
            return attr.Inverse ? conditionMet : !conditionMet;
        }

        private SerializedProperty FindConditionProperty(SerializedProperty property, string conditionName)
        {
            var parentPath = GetParentPath(property.propertyPath);
            var relativePath = string.IsNullOrEmpty(parentPath) ? conditionName : parentPath + "." + conditionName;

            var relative = property.serializedObject.FindProperty(relativePath);
            if (relative != null) return relative;

            return property.serializedObject.FindProperty(conditionName);
        }

        private string GetParentPath(string propertyPath)
        {
            int lastDot = propertyPath.LastIndexOf('.');
            return lastDot < 0 ? string.Empty : propertyPath.Substring(0, lastDot);
        }
    }

#endif

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : PropertyAttribute
{
	public string Label { get; }

	public ButtonAttribute(string label = null)
	{
		Label = label;
	}
}

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class TexturePreviewAttribute : Attribute
{
	public float MaxWidth { get; }

	public TexturePreviewAttribute(float maxWidth = 256f)
	{
		MaxWidth = maxWidth;
	}
}
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ReadOnlyAttribute : PropertyAttribute { }

public abstract class ConditionalBaseAttribute : PropertyAttribute
    {
        public readonly string ConditionField;
        public readonly bool UseEnumComparison;
        public readonly bool ExpectedBool;
        public readonly int ExpectedEnumValue;
        public readonly bool Inverse; // true = ShowIf, false = HideIf

        protected ConditionalBaseAttribute(string conditionBoolField, bool expectedBool, bool inverse)
        {
            ConditionField = conditionBoolField;
            ExpectedBool = expectedBool;
            UseEnumComparison = false;
            Inverse = inverse;
        }

        protected ConditionalBaseAttribute(string conditionEnumOrIntField, int expectedEnumValue, bool inverse)
        {
            ConditionField = conditionEnumOrIntField;
            ExpectedEnumValue = expectedEnumValue;
            UseEnumComparison = true;
            Inverse = inverse;
        }
    }
[AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class HideIfAttribute : ConditionalBaseAttribute
    {
        public HideIfAttribute(string conditionBoolField, bool expectedBool = true)
            : base(conditionBoolField, expectedBool, inverse: false) { }

        public HideIfAttribute(string conditionEnumOrIntField, int expectedEnumValue)
            : base(conditionEnumOrIntField, expectedEnumValue, inverse: false) { }
    }

[AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ShowIfAttribute : ConditionalBaseAttribute
{
	public ShowIfAttribute(string conditionBoolField, bool expectedBool = true)
		: base(conditionBoolField, expectedBool, inverse: true) { }

	public ShowIfAttribute(string conditionEnumOrIntField, int expectedEnumValue)
		: base(conditionEnumOrIntField, expectedEnumValue, inverse: true) { }
}
