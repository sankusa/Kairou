using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Kairou
{
    internal static class ReflectionUtil
    {
        public static object GetObject(object rootObject, string path) {
            foreach(string pathElement in path.Split('.')) {
                if(pathElement.Contains("[")) {
                    string arrayName = pathElement.Substring(0, pathElement.IndexOf("["));
                    int index = int.Parse(pathElement.Substring(pathElement.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    rootObject = GetValue(rootObject, arrayName, index);
                }
                else {
                    rootObject = GetValue(rootObject, pathElement);
                }
            }
            return rootObject;
        }

        private static object GetValue(object source, string name) {
            if(source == null) return null;

            Type type = source.GetType();

            while(type != null) {
                FieldInfo fieldInfo = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if(fieldInfo != null) return fieldInfo.GetValue(source);

                PropertyInfo propertyInfo = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(propertyInfo != null) return propertyInfo.GetValue(source, null);

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue(object source, string name, int index) {
            var enumerable = GetValue(source, name) as IEnumerable;
            var enumerator = enumerable.GetEnumerator();
            
            for(int i = 0; i <= index; i++) {
                enumerator.MoveNext();
            }

            return enumerator.Current;
        }
    }
}