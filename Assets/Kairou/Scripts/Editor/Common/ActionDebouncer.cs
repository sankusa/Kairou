using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kairou.Editor
{
    public class ActionDebouncer
    {
        VisualElement _scheduler;
        float _delaySeconds;
        public float DelaySeconds
        {
            get => _delaySeconds;
            set => _delaySeconds = value;
        }
        long _delayFrame;
        public long DelayFrame
        {
            get => _delayFrame;
            set => _delayFrame = value;
        }
        long _callId;
        Action _action;
        public Action Action
        {
            get => _action;
            set => _action = value;
        }

        public ActionDebouncer(VisualElement scheduler, float delaySeconds = 0, long delayFrame = 0, Action action = null)
        {
            _scheduler = scheduler;
            _delaySeconds = delaySeconds;
            _delayFrame = delayFrame;
            _action = action;
        }

        public void Schedule()
        {
            _callId++;
            var currentCallId = _callId;
            var frameOnCalled = TickCounter.Count;
            _scheduler.schedule.Execute(() =>
            {
                UpdateEvent(currentCallId, frameOnCalled);
            })
            .ExecuteLater((long)(_delaySeconds * 1000));
        }

        void UpdateEvent(long currentCallId, long frameOnCalled)
        {
            if (currentCallId != _callId) return;
            if (TickCounter.Count - frameOnCalled >= _delayFrame)
            {
                _action?.Invoke();
            }
            // 規定のフレーム数を超えていなければ引き伸ばし
            else
            {
                _scheduler.schedule.Execute(() =>
                {
                    UpdateEvent(currentCallId, frameOnCalled);
                })
                .ExecuteLater((long)(_delaySeconds * 1000));
            }
        }
    }
}