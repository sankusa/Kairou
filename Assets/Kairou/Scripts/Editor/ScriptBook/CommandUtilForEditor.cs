using UnityEditor;

namespace Kairou.Editor
{
    public class CommandUtilForEditor
    {
        public static Command GetContainingCommand(SerializedProperty property)
        {
            // 通常、CommandはmanagedReferenceとなるはずなので、managedReferenceValueを通してCommandインスタンスを取得すればいいのだが
            // 柔軟性のためmanagedReference以外のケースもカバーできるようにしておく。
            // パフォーマンス最適ではないが、managedReferenceValueの代わりにGetObject()を使用する。
            // パフォーマンスに問題ありなら、managedReferenceValueの使用を検討する。
            var prop = property.GetParent();
            while (prop != null)
            {
                if (prop.GetObject() is Command command)
                {
                    return command;
                }
                prop = prop.GetParent();
            }

            return null;
        }
    }
}