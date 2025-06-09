using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public class EasyDropdownField<T> : DropdownField
    {
        T[] _objects;

        public void SetOptions(T[] objects, List<string> choices)
        {
            _objects = objects;
            this.choices = choices;
        }

        public void SetValue(T value)
        {
            index = Array.IndexOf(_objects, value);
        }

        public bool RegisterValueChanged(Action<T> callBack, T defaultValue)
        {
            return this.RegisterValueChangedCallback(evt =>
            {
                int index = choices.IndexOf(evt.newValue);
                if (index == -1)
                {
                    callBack(defaultValue);
                }
                else
                {
                    callBack(_objects[index]);
                }
            });
        }
    }
}