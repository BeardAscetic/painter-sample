using UnityEngine;

namespace BeardAscetic
{
    public class AreaBombAttack : AttackModule
    {
        // TODO : Scriptable
        [SerializeField] private GameObject lightningPrefab;
        [SerializeField] private int lightningCount = 3;
        [SerializeField] private float lightningRange = 5f;
        [SerializeField] private float minLightningDistance = 2f;
        [SerializeField] private LayerMask enemyLayerMask;

        private Collider2D[] hitBuffer = new Collider2D[100];
        public AttackData AttackData;

        protected override void PerformAttack()
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                transform.position, lightningRange, hitBuffer, enemyLayerMask);
            int strikesOnEnemies = Mathf.Min(hitCount, lightningCount);

            for (int i = 0; i < strikesOnEnemies; i++)
            {
                int idx = Random.Range(i, hitCount);
                var tmp = hitBuffer[i];
                hitBuffer[i] = hitBuffer[idx];
                hitBuffer[idx] = tmp;

                SpawnBomb(hitBuffer[i].transform.position);
            }

            int remaining = lightningCount - strikesOnEnemies;
            for (int i = 0; i < remaining; i++)
            {
                Vector2 dir = Random.insideUnitCircle.normalized;
                float radius = Random.Range(minLightningDistance, lightningRange);
                var randPos = (Vector2)transform.position + dir * radius;
                SpawnBomb(randPos);
            }
        }

        private void SpawnBomb(Vector2 pos)
        {
            BombManager.TrySpawn(pos, AttackData);
        }
    }
}