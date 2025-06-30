using UnityEngine;

namespace BeardAscetic
{
    public class GunAttack : AttackModule
    {
        public BulletAttackData AttackData;
        protected override float AttackInterval => AttackData.AttackInterval;

        protected override void PerformAttack()
        {
            Vector2 dir = attackDirection;
            if (dir == Vector2.zero) return;

            float bulletCount = AttackData.BulletCount;
            float bulletAngle = AttackData.BulletAngle;

            if (bulletCount <= 1 || Mathf.Approximately(bulletAngle, 0f))
            {
                Fire(dir);
                return;
            }

            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float step = bulletAngle / (bulletCount - 1);
            float start = baseAngle - bulletAngle * 0.5f;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = start + step * i;
                float rad = angle * Mathf.Deg2Rad;
                Fire(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)));
            }
        }

        private void Fire(Vector2 direction)
        {
            BulletManager.TrySpawnAndLaunch(
                transform.position, direction,
                AttackData.BulletSpeed, AttackData.BulletMaxDistance, AttackData.BulletRadius, true);
        }
    }
}