using System;

namespace Kairou.Editor
{
    public static class ScriptBookOwnerExtension
    {
        public static void ChangeWithUndo(this IScriptBookOwner owner, string undoName, Action<IScriptBookOwner> changeAction)
        {
            owner.AsObject()
                .ChangeWithUndo(undoName, obj => changeAction((IScriptBookOwner)obj));
        }
    }
}