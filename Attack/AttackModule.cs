using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BeardAscetic
{
    [RequireComponent(typeof(MonoBehaviour))]
    public abstract class AttackModule : MonoBehaviour, IPause
    {
        [Header("공격 주기 (초)")]
        [SerializeField] private float attackInterval = 1f;

        public PlayerCharacter TargetCharacter;
        public bool IsTargetEnemy = true;

        protected virtual float AttackInterval => attackInterval;
        protected Vector2 attackDirection => IsTargetEnemy ? TargetCharacter.LastAttackDir : TargetCharacter.LastMoveDir;
        
        private bool isPaused;
        private CancellationTokenSource loopCts;
        private CancellationToken destroyToken;

        public bool IsRunning { get; private set; }

        protected virtual void Awake()
        {
            destroyToken = this.GetCancellationTokenOnDestroy();
            TimeManager.Instance.Register(this);
        }

        public void StartLoop()
        {
            if (IsRunning)
            {
                RestartLoop();
                return;
            }

            loopCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);
            IsRunning = true;
            RunLoopAsync(loopCts.Token).Forget();
        }

        public void StopLoop()
        {
            if (!IsRunning) return;

            loopCts.Cancel();
            loopCts.Dispose();
            loopCts = null;
            IsRunning = false;
        }

        public void RestartLoop()
        {
            StopLoop();
            StartLoop();
        }

        async UniTaskVoid RunLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await UniTask.WaitWhile(() => isPaused, cancellationToken: token);

                    // 실제 공격
                    PerformAttack();

                    float elapsed = 0f;
                    while (elapsed < AttackInterval && !token.IsCancellationRequested)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        if (!isPaused)
                            elapsed += Time.deltaTime;
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                // 안전하게 상태 리셋
                IsRunning = false;
            }
        }

        protected abstract void PerformAttack();

        public void RegisterPause()
        {
            TimeManager.Instance.Register(this);
        }

        public void PauseTime()   => isPaused = true;
        public void ResumeTime()  => isPaused = false;
    }
}
