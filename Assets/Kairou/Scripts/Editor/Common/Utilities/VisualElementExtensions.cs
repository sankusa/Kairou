using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public static class VisualElementExtensions
    {
        public static void SetBorderColor(this VisualElement element, Color color)
        {
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
        }

        public static void SetBorderWidth(this VisualElement element, float width)
        {
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
        }


        public static void RegisterValueChangedCallbackWithoutOnRegister(this PropertyField propertyField, EventCallback<SerializedPropertyChangeEvent> callback)
        {
            bool first = true;
            propertyField.RegisterValueChangeCallback(evt =>
            {
                if (first)
                {
                    first = false;
                    return;
                }
                callback?.Invoke(evt);
            });
        }
    }
}