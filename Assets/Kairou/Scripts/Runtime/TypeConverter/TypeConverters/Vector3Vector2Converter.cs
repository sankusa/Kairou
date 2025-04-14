using UnityEngine;

namespace Kairou
{
    public class Vector3Vector2Converter : TypeConverter<Vector3, Vector2>
    {
        public override Vector2 Convert(Vector3 fromValue)
        {
            return fromValue;
        }
    }
}