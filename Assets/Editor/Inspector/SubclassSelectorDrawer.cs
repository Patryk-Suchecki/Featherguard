using System;
using System.Linq;
using Featherguard.Core;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        const string NoneLabel = "(none)";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.propertyType != SerializedPropertyType.ManagedReference || !property.isExpanded) return height;
            foreach (var child in Children(property))
                height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "Use [SubclassSelector] with [SerializeReference].");
                return;
            }

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(new Rect(line.x, line.y, EditorGUIUtility.labelWidth, line.height), property.isExpanded, label, true);
            var button = new Rect(line.x + EditorGUIUtility.labelWidth, line.y, line.width - EditorGUIUtility.labelWidth, line.height);
            var current = property.managedReferenceValue?.GetType();
            if (EditorGUI.DropdownButton(button, new GUIContent(current != null ? ObjectNames.NicifyVariableName(current.Name) : NoneLabel), FocusType.Keyboard))
                ShowMenu(property, current, button);

            if (!property.isExpanded) return;
            EditorGUI.indentLevel++;
            float y = line.yMax + EditorGUIUtility.standardVerticalSpacing;
            foreach (var child in Children(property))
            {
                float height = EditorGUI.GetPropertyHeight(child, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child, true);
                y += height + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel--;
        }

        static void ShowMenu(SerializedProperty property, Type current, Rect anchor)
        {
            var baseType = ResolveType(property.managedReferenceFieldTypename);
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(NoneLabel), current == null, () => Assign(property, null));
            foreach (var type in TypeCache.GetTypesDerivedFrom(baseType)
                         .Where(t => !t.IsAbstract && !t.IsGenericType && t.GetConstructor(Type.EmptyTypes) != null)
                         .OrderBy(t => t.Name))
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), type == current, () => Assign(property, Activator.CreateInstance(type)));
            menu.DropDown(anchor);
        }

        static void Assign(SerializedProperty property, object value)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = value;
            property.isExpanded = value != null;
            property.serializedObject.ApplyModifiedProperties();
        }

        static Type ResolveType(string typename)
        {
            int split = typename.IndexOf(' ');
            return split < 0 ? typeof(object) : Type.GetType($"{typename.Substring(split + 1)}, {typename.Substring(0, split)}") ?? typeof(object);
        }

        static System.Collections.Generic.IEnumerable<SerializedProperty> Children(SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            if (!iterator.NextVisible(true)) yield break;
            while (!SerializedProperty.EqualContents(iterator, end))
            {
                yield return iterator.Copy();
                if (!iterator.NextVisible(false)) yield break;
            }
        }
    }
}
