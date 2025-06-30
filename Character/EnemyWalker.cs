using System;
using UnityEngine;

namespace BeardAscetic
{
    public class EnemyWalker : EnemyCharacter
    {
        protected override void OnSpawnChild()
        {
            Footprint?.StartLoop();
        }

        protected override void FixedUpdate()
        {
            if (!isInitialized || targetPlayer == null || isPause)
                return;

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
    }
}
