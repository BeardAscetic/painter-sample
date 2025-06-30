using UniRx;
using UnityEngine;

namespace BeardAscetic
{
    public class PlayerHPBar : MonoBehaviour
    {
        public PlayerRuntimeStatus RuntimeStatus;
        [SerializeField] private Transform fillBar;

        private float maxHp;
        private float curHp;

        public void Init()
        {
            RuntimeStatus.MaxHealth.Subscribe(hp =>
            {
                maxHp = hp;
                SetHealth();
            }).AddTo(this);

            RuntimeStatus.CurrentHealth.Subscribe(hp =>
            {
                curHp = hp;
                SetHealth();
            }).AddTo(this);
        }

        public void SetHealth()
        {
            curHp = Mathf.Clamp(curHp, 0, maxHp);
            float ratio = curHp / maxHp;
            fillBar.localScale = new Vector3(ratio, 1f, 1f);
        }
    }
}