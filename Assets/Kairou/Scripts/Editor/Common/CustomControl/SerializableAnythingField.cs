using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public class LightManagedReferenceField : VisualElement
    {
        PropertyField _propertyField;
        TmpObject _tmpObj;
        SerializedObject _serializedObject;
        SerializedProperty _managedReferenceProperty;

        Action _onValueChanged;

        public LightManagedReferenceField(Action onValueChanged)
        {
            _tmpObj = ScriptableObject.CreateInstance<TmpObject>();
            _serializedObject = new SerializedObject(_tmpObj);

            _propertyField = new PropertyField();
            _propertyField.bindingPath = "_object";
            _propertyField.RegisterValueChangeCallback(_ =>
            {
                _managedReferenceProperty.serializedObject.ApplyModifiedProperties();
                _onValueChanged?.Invoke();
            });

            _onValueChanged = onValueChanged;

            Add(_propertyField);
        }

        public void Attach(SerializedProperty managedReferenceProperty)
        {
            _propertyField.Unbind();

            _tmpObj.Object = managedReferenceProperty.managedReferenceValue;
            _serializedObject = new SerializedObject(_tmpObj);
            _managedReferenceProperty = managedReferenceProperty;

            _propertyField.Bind(_serializedObject);
        }

        public void Refresh()
        {
            var nameProp = _serializedObject.FindProperty("m_Name");
            nameProp.stringValue = string.IsNullOrEmpty(nameProp.stringValue) ? " " : "";
        }
    }

    public class SerializableAnythingField<T> : VisualElement where T : class
    {
        PropertyField _propertyField;
        TmpObject _tmpObj;
        SerializedObject _serializedObject;
        Action<T> _onValueChanged;

        public SerializableAnythingField(Action<T> onValueChanged)
        {
            _tmpObj = ScriptableObject.CreateInstance<TmpObject>();
            _serializedObject = new SerializedObject(_tmpObj);

            _propertyField = new PropertyField();
            _propertyField.bindingPath = "_object";
            _propertyField.RegisterValueChangeCallback(_ =>
            {
                _onValueChanged?.Invoke(_tmpObj.Object as T);
            });

            _onValueChanged = onValueChanged;

            Add(_propertyField);
        }

        public void Attach(T target)
        {
            _propertyField.Unbind();

            _tmpObj.Object = target;
            _serializedObject = new SerializedObject(_tmpObj);

            _propertyField.Bind(_serializedObject);
        }
    }

    public class TmpObject : ScriptableObject
    {
        [SerializeReference] object _object;
        public object Object
        {
            get => _object;
            set => _object = value;
        }
    }
}