using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeardAscetic
{
    public class EnemyJumper : EnemyCharacter
    {
        [SerializeField] private float jumpInterval = 1f;
        [SerializeField] private float jumpDuration = 0.5f; // 점프 이동에 걸릴 시간

        private float stopSqr => moveLimit * moveLimit;

        private CancellationTokenSource jumpCts;
        private float jumpElapsedTime;

        protected override void OnSpawnChild()
        {
            CancelJump();

            jumpCts = new CancellationTokenSource();

            jumpElapsedTime = jumpInterval;

            _ = JumpingLoopAsync(jumpCts.Token);
        }

        private async UniTaskVoid JumpingLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.NextFrame(token);

                if (isPause || !isInitialized || targetPlayer == null)
                    continue;

                jumpElapsedTime += Time.deltaTime;

                var toPlayer = (Vector2)(targetPlayer.transform.position - transform.position);
                if (toPlayer.sqrMagnitude <= stopSqr
                    && jumpElapsedTime >= jumpInterval)
                {
                    var startPos = Rigidbody.position;
                    var endPos = targetPlayer.transform.position;

                    _ = DoJumpAsync(startPos, endPos, jumpDuration, token);

                    jumpElapsedTime = 0f;
                }
            }
        }

        private async UniTask DoJumpAsync(Vector2 start, Vector2 end, float duration, CancellationToken token)
        {
            float elapsed = 0f;

            var jumpDir = end - start;
            if (jumpDir.sqrMagnitude > 0.01f)
                flipSubject.OnNext(jumpDir.x < 0f);

            Visual.PlayJumpVisual(duration);

            while (elapsed < duration && !token.IsCancellationRequested)
            {
                if (!isPause)
                {
                    float t = elapsed / duration;
                    Vector2 nextPos = Vector2.Lerp(start, end, t);
                    Rigidbody.MovePosition(nextPos);
                    elapsed += Time.deltaTime;
                }

                await UniTask.NextFrame(token);
            }

            Rigidbody.MovePosition(end);
            ColorManager.Instance.OnSplat(false, SplatType.Dot_1, end, Quaternion.identity, 2f);
        }

        protected override void FixedUpdate()
        {
            if (!isInitialized || targetPlayer == null || isPause)
                return;

            var toPlayer = (Vector2)(targetPlayer.transform.position - transform.position);

            if (toPlayer.sqrMagnitude <= stopSqr)
            {
                flipSubject.OnNext(toPlayer.x < 0f);
                MoverData.StopMove(Rigidbody);
                return;
            }

            var dir = Vector2.zero;
            if (targetPlayer != null)
            {
                dir = (targetPlayer.transform.position - (Vector3)this.transform.position).normalized;
            }

            Rigidbody.linearVelocity = dir * moveSpeed;
            lastVelocity = Rigidbody.linearVelocity;

            if (dir.sqrMagnitude > 0.01f)
                flipSubject.OnNext(dir.x < 0f);
        }

        private void CancelJump()
        {
            if (jumpCts != null)
            {
                jumpCts.Cancel();
                jumpCts.Dispose();
                jumpCts = null;
            }
        }

        public override void OnDespawnChild()
        {
            CancelJump();
        }

        protected override void OnDestroyChild()
        {
            CancelJump();
        }
    }
}