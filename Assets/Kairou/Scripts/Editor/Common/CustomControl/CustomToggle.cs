using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public class CustomToggle : Toggle
    {
        Action _unregisterAll;

        public void RegisterValueChangedCallbackAsUnregisterable(EventCallback<ChangeEvent<bool>> callback, bool skipFirst)
        {
            if (skipFirst == false)
            {
                this.RegisterValueChangedCallback(callback);
                _unregisterAll += () => this.UnregisterValueChangedCallback(callback);
            }
            else
            {
                bool first = true;
                EventCallback<ChangeEvent<bool>> wrapper = evt =>
                {
                    if (first)
                    {
                        first = false;
                        return;
                    }
                    callback?.Invoke(evt);
                };
                this.RegisterValueChangedCallback(wrapper);
                _unregisterAll += () => this.UnregisterValueChangedCallback(wrapper);
            }

        }

        public void UnregisterAll()
        {
            _unregisterAll?.Invoke();
            _unregisterAll = null;
        }
    }
}