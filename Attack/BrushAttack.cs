using UnityEngine;

namespace BeardAscetic
{
    public class BrushAttack : AttackModule
    {
        public AttackData AttackData;

        protected override void PerformAttack()
        {
            Vector2 lastDir = attackDirection;
            float angle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector3 spawnPos = transform.position + (Vector3)lastDir * 2f; // offset

            Vector2 offset = rot * AttackData.RangeData.LocalPositionOffset;
            Vector2 origin = transform.position;
            Vector2 indicatorPos = origin + offset;
            float finalRot = angle + AttackData.RangeData.LocalRotationOffset;

            HitChecker.TryCheckConeHit(
                indicatorPos, finalRot,
                AttackData.RangeData.Angle,
                AttackData.RangeData.Radius,
                LayerManager.EnemyHitLayerMask,
                ColliderManager.ResultColliders,
                OnHit);
            
            OnVisual(spawnPos, rot, indicatorPos, finalRot);
        }

        private void OnVisual(Vector2 spawnPos, Quaternion rot, Vector2 indicatorPos, float finalRot)
        {
            ColorManager.Instance.OnFixedSplat(
                false, SplatType.Brush, spawnPos, rot);
            
            AttackIndicatorManager.Instance.OnIndicator(
                indicatorPos, finalRot,
                AttackData.RangeData.Angle,
                AttackData.RangeData.Radius);
        }
 

        private void OnHit(Collider2D col)
        {
            //충돌콜백
            if (ColliderManager.TryGetCharacter(col, out var enemy))
                enemy.OnDamaged(1);
        }
    }
}