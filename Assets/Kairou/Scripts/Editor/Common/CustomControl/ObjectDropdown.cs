using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Kairou.Editor
{
    [UxmlElement]
    public partial class ObjectDropdown : VisualElement
    {
        readonly Label _label;
        public Label Label => _label;

        readonly DropdownField _dropdownField;
        public DropdownField DropdownField => _dropdownField;

        readonly ObjectField _objectField;
        public ObjectField ObjectField => _objectField;

        Func<Object, string> _makeChoiceFunc;
        Action<Object> _onSelect;

        IReadOnlyList<Object> _objects;

        public ObjectDropdown()
        {
            _label = new Label("Select");
            _label.style.unityTextAlign = TextAnchor.MiddleCenter;

            _dropdownField = new DropdownField();
            _dropdownField.style.marginRight = 0;
            _dropdownField.RegisterValueChangedCallback(_ =>
            {
                var obj = _objects[_dropdownField.index];
                _objectField.value = obj;
                if (_dropdownField.index == -1)
                {
                    _onSelect.Invoke(null);
                }
                else
                {
                    _onSelect.Invoke(obj);
                }
            });

            _objectField = new ObjectField();
            _objectField.enabledSelf = false;
            _objectField.style.marginLeft = 0;

            style.flexDirection = FlexDirection.Row;
            
            Add(_label);
            Add(_dropdownField);
            Add(_objectField);
        }

        public void SetUp<T>(string label, Func<T, string> makeChoiceFunc, Action<T> onSelect) where T : Object
        {
            _label.text = label;
            _makeChoiceFunc = obj => makeChoiceFunc((T)obj);
            _onSelect = obj => onSelect((T)obj);
        }

        public void SetObjects<T>(IReadOnlyList<T> objects) where T : Object
        {
            _objects = objects;
            _dropdownField.choices = objects.Select(_makeChoiceFunc).ToList();
            _dropdownField.value = _objectField.value == null ? null : _makeChoiceFunc(_objectField.value);
            if (_dropdownField.value == null)
            {
                var first = objects.FirstOrDefault();
                if (first != null)
                {
                    _dropdownField.value = _makeChoiceFunc(first);
                    _objectField.value = first;
                }
            }
        }
    }
}