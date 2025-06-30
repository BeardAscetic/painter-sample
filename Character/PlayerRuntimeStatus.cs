using UniRx;
using UnityEngine;

namespace BeardAscetic
{
    public class PlayerRuntimeStatus : MonoBehaviour
    {
        public PlayerStatus StatusData;

        private ReactiveProperty<int> maxHealth = new ReactiveProperty<int>();
        private ReactiveProperty<int> currentHealth = new ReactiveProperty<int>();
        private ReactiveProperty<float> currentMoveSpeed = new ReactiveProperty<float>();

        public IReadOnlyReactiveProperty<int> MaxHealth => maxHealth;
        public IReadOnlyReactiveProperty<int> CurrentHealth => currentHealth;
        public IReadOnlyReactiveProperty<float> CurrentMoveSpeed => currentMoveSpeed;

        public bool IsDead;

        public void InitStatus()
        {
            maxHealth.Value = StatusData.Health;
            currentMoveSpeed.Value = StatusData.MoveSpeed;
            currentHealth.Value = maxHealth.Value;
        }
        
        public void OnDamage(int damage)
        {
            currentHealth.Value -= damage;
            if (currentHealth.Value <= 0)
            {
                currentHealth.Value = 0; // Health가 0 이하로 떨어지지 않도록 보장
                IsDead = true;
            }
        }

        public void OnRevive()
        {
            IsDead = false;
            currentHealth.Value = maxHealth.Value;
        }
    }
}
