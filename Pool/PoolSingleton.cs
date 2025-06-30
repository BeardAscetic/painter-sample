using System.Collections.Generic;
using UnityEngine;

namespace BeardAscetic
{
    public abstract class PoolSingleton<TPool, TItem> : MonoBehaviour
        where TPool : PoolSingleton<TPool, TItem>
        where TItem : Component, IPoolable
    {
        private static TPool instance;
        public static TPool Instance
        {
            get
            {
                if (instance == null)
                {
                    
                    instance = FindObjectOfType<TPool>(true);
                    if (instance == null)
                    {
                        Debug.LogError("" + typeof(TPool).Name + " 인스턴스가 씬에 존재하지 않습니다. PoolSingleton을 사용하려면 해당 타입의 MonoBehaviour를 씬에 추가해야 합니다.");
                        return null;
                    }
                }
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitStatic()
        {
            instance = null;
        }

        
        [Header("풀링할 프리팹 (IPoolable 컴포넌트가 붙어 있어야 함)")]
        [SerializeField] protected GameObject itemPrefab;

        [Header("초기 풀 크기")]
        [SerializeField] protected int initialPoolSize = 50;

        [Header("동시 활성화 최대 개수 (0 이하 = 무제한)")]
        [SerializeField] protected int maxActiveCount = 0;

        private readonly Queue<TItem> pool = new Queue<TItem>();
        private int activeCount = 0;
        public int ActiveCount => activeCount;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as TPool;
                
            }
            else if (instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            if (itemPrefab == null)
            {
                Debug.LogError($"[{typeof(TPool).Name}] itemPrefab이 할당되지 않았습니다.");
                return;
            }

        }

        #region Pool

        protected void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                TItem newItem = CreateNewInstance();
                ReturnToPool(newItem);
            }
        }

        public virtual bool TrySpawn(Vector3 position, Quaternion rotation, out TItem item)
        {
            if (maxActiveCount > 0 && activeCount >= maxActiveCount)
            {
                item = null;
                return false;
            }

            if (pool.Count > 0)
            {
                item = pool.Dequeue();
                OnDequeue(item);
            }
            else
            {
                item = CreateNewInstance();
                OnCreateNew(item);
            }

            activeCount++;
            item.transform.SetPositionAndRotation(position, rotation);
            OnSpawn(item);
            return true;
        }

        public void Despawn(TItem item)
        {
            if (item == null || item.IsPooled) return;

            OnDespawn(item);
            ReturnToPool(item);
            activeCount = Mathf.Max(0, activeCount - 1);
        }

        protected virtual TItem CreateNewInstance()
        {
            var go = Instantiate(itemPrefab, this.transform);
            var comp = go.GetComponent<TItem>();
            if (comp == null)
            {
                Debug.LogError($"[{typeof(TPool).Name}] itemPrefab에 {typeof(TItem).Name} 컴포넌트가 없습니다.");
                return null;
            }
            comp.OnReturnToPool();
            return comp;
        }

        protected virtual void OnDequeue(TItem item) { }

        protected virtual void OnCreateNew(TItem item) { }

        protected virtual void OnSpawn(TItem item)
        {
            item.OnSpawnFromPool();
        }

        protected virtual void OnDespawn(TItem item) { }

        protected virtual void ReturnToPool(TItem item)
        {
            item.OnReturnToPool();
            pool.Enqueue(item);
        }

        #endregion
    }
}
