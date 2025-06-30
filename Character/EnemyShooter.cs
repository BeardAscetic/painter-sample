using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BeardAscetic
{
    public class EnemyShooter : EnemyCharacter
    {
        [SerializeField] private float shootInterval = 1f;

        private float stopSqr => moveLimit * moveLimit;
        private CancellationTokenSource shootCts;
        private float shootElapsedTime;

        protected override void OnSpawnChild()
        {
            shootCts?.Cancel();
            shootCts?.Dispose();

            shootCts = new CancellationTokenSource();

            shootElapsedTime = shootInterval;

            _ = ShootingLoopAsync(shootCts.Token);

            Footprint?.StartLoop();
        }

        private async UniTaskVoid ShootingLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.NextFrame(token);

                if (isPause || !isInitialized || targetPlayer == null)
                    continue;

                shootElapsedTime += Time.deltaTime;

                var toPlayer = (Vector2)(targetPlayer.transform.position - transform.position);
                if (toPlayer.sqrMagnitude <= stopSqr
                    && shootElapsedTime >= shootInterval)
                {
                    Shoot();
                    shootElapsedTime = 0f;
                }
            }
        }

        private void Shoot()
        {
            Vector2 dir = targetPlayer.transform.position - transform.position;
            dir.Normalize();
            BulletManager.TrySpawnAndLaunch(this.transform.position, dir, moveSpeed * 1.5f, 10, 0.1f, false);
        }

        protected override void FixedUpdate()
        {
            if (!isInitialized || targetPlayer == null || isPause)
                return;

            var toPlayer = (Vector2)(targetPlayer.transform.position - transform.position);
            flipSubject.OnNext(toPlayer.x < 0f);

            if (toPlayer.sqrMagnitude <= stopSqr)
            {
                MoverData.StopMove(Rigidbody);
                return;
            }

            var dir = Vector2.zero;
            if (targetPlayer != null)
            {
                dir = toPlayer.normalized;
            }

            Rigidbody.linearVelocity = dir * moveSpeed;
            lastVelocity = Rigidbody.linearVelocity;

            if (dir.sqrMagnitude > 0.01f)
                flipSubject.OnNext(dir.x < 0f);
        }

        private void CancelShooting()
        {
            shootCts?.Cancel();
            shootCts?.Dispose();
            shootCts = null;
        }

        protected override void OnDestroyChild()
        {
            CancelShooting();
        }

        public override void OnDespawnChild()
        {
            CancelShooting();
        }
    }
}