using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class SerializedArray
    {
        public static void Set(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
