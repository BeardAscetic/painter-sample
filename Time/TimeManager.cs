using System;
using UnityEngine;
using UniRx;

namespace BeardAscetic
{
    public class TimeManager : Singleton<TimeManager>
    {
        private ReactiveProperty<float> remainingTime = new ReactiveProperty<float>(0f);
        public IReadOnlyReactiveProperty<float> RemainingTime => remainingTime;

        private Subject<Unit> onTimerCompleted = new Subject<Unit>();
        public IObservable<Unit> OnTimerCompleted => onTimerCompleted;

        private float endTime;
        private bool isRunning;
        public static bool IsRunning => Instance.isRunning;

        public event Action OnPause;
        public event Action OnResume;
        private bool isPaused = false;
        private float pauseStartTime;

        public void StartTimer(float duration)
        {
            endTime = Time.time + duration;
            remainingTime.Value = duration;
            isRunning = true;
            isPaused = false;
        }
        
        void Update()
        {
            if (!isRunning || isPaused) return;

            float t = Mathf.Max(0f, endTime - Time.time);
            remainingTime.Value = t;

            if (t <= 0f)
            {
                isRunning = false;
                onTimerCompleted.OnNext(Unit.Default);
                onTimerCompleted.OnCompleted();
                PauseAll();
            }
        }

        public void Register(IPause pauseable)
        {
            OnPause += pauseable.PauseTime;
            OnResume += pauseable.ResumeTime;
        }

        public void Unregister(IPause pauseable)
        {
            OnPause -= pauseable.PauseTime;
            OnResume -= pauseable.ResumeTime;
        }

        public void PauseAll()
        {
            if (isPaused) return;
            isPaused = true;
            pauseStartTime = Time.time;
            OnPause?.Invoke();
        }

        public void ResumeAll()
        {
            if (!isRunning || !isPaused) return;
            // 일시정지 기간만큼 endTime 보정
            float pausedDuration = Time.time - pauseStartTime;
            endTime += pausedDuration;
            isPaused = false;
            OnResume?.Invoke();
        }
    }
}
