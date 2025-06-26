using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public class CustomPropertyField : PropertyField
    {
        Action _unregisterAll = null;

        public void RegisterValueChangedCallbackAsUnRegistable(EventCallback<SerializedPropertyChangeEvent> callback)
        {
            RegisterValueChangeCallback(callback);
            _unregisterAll += () => UnregisterCallback(callback);
        }

        public void UnregisterValueChangedCallback()
        {
            _unregisterAll?.Invoke();
            _unregisterAll = null;
        }
    }
}