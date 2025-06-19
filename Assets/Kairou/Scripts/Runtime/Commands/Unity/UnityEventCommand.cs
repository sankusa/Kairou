using UnityEngine;
using UnityEngine.Events;

namespace Kairou
{
    public partial class UnityEventCommand : Command
    {
        [SerializeField] UnityEvent _unityEvent;

        [CommandExecute]
        void Execute()
        {
            _unityEvent.Invoke();
        }

        override public string GetSummary() => _unityEvent.GetPersistentEventCount().ToString() + " Events";
    }
}