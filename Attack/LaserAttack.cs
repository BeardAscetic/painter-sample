using UnityEngine;

namespace BeardAscetic
{
    public class LaserAttack : AttackModule
    {
        // TODO : Scriptable
        [Header("레이저 설정")]
        [Tooltip("레이저 최대 길이")]
        [SerializeField] private float maxLength = 10f;
        [Tooltip("Dot 간격")]
        [SerializeField] private float dotSpacing = 1f;
        [SerializeField] private float dotRadius = 0.5f;
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private float damage = 1f;

        protected override void PerformAttack()
        {
            Vector2 dir = attackDirection;
            Vector2 origin = (Vector2)transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            float boxLength = maxLength + (dotRadius * 0.5f);
            float halfThick = dotRadius;
            Vector2 boxSize = new Vector2(boxLength, halfThick * 2f);
            Vector2 boxCenter = origin + dir * (boxLength * 0.5f);

            HitChecker.TryCheckBoxHit(
                origin,
                dir,
                maxLength,
                dotRadius,
                targetLayerMask,
                ColliderManager.ResultColliders,
                OnHit            // onHit 콜백
            );
            
            OnVisual(origin, dir, boxCenter, angle, boxSize);
        }

        public void OnVisual(Vector2 origin, Vector2 dir, Vector2 boxCenter, float angle, Vector2 boxSize)
        {
            AttackIndicatorManager.Instance.OnIndicator(
                boxCenter,
                angle,
                boxSize
            );

            int dotCount = Mathf.FloorToInt(maxLength / dotSpacing);

            for (int i = 1; i <= dotCount; i++)
            {
                Vector2 spawnPos = origin + dir * dotSpacing * i;
                ColorManager.Instance.OnSplat(true, SplatType.Bullet_1, spawnPos, Quaternion.identity,
                    dotRadius);
            }
        }

        public void OnHit(Collider2D col)
        {
            if (ColliderManager.TryGetCharacter(col, out var enemy))
            {
                enemy.OnDamaged(1);
            }
        }
    }
}