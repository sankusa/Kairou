using UnityEngine;

namespace Kairou
{
    public class Vector2Vector3Converter : TypeConverter<Vector2, Vector3>
    {
        public override Vector3 Convert(Vector2 fromValue)
        {
            return fromValue;
        }
    }
}