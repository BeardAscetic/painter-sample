using UnityEngine;

namespace BeardAscetic
{
    public enum CharacterGroup
    {
        Player,
        Enemy
    }
    public abstract class CharacterBase : MonoBehaviour
    {
        public CharacterGroup Group = CharacterGroup.Enemy;
        public abstract void OnDamaged(int damage);
    }
}
