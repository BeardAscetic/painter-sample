using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BeardAscetic
{
    public class InGameLoadManager : Singleton<InGameLoadManager>
    {
        // TODO : 추후 Inspector Cache로 프로세스 최적화 
        [SerializeField] private MonoBehaviour[] loaderBehaviours;

        private IDataLoader[] loaders;

        private UniTask loadTask;
        public UniTask LoadTask => loadTask;
        public bool IsLoadComplete = false;

        protected override void Awake()
        {
            base.Awake();

            loadTask = LoadAllDataAsync();
            loadTask.Forget();
        }

        private async UniTask LoadAllDataAsync()
        {
            // TODO : 추후 Inspector Cache로 프로세스 최적화
            CacheDataLoaders();

            loaders = loaderBehaviours
                .OfType<IDataLoader>()
                .ToArray();

            await UniTask.WhenAll(loaders.Select(x => x.LoadDataAsync()));

            IsLoadComplete = true;
        }

        [Sirenix.OdinInspector.Button]
        private void CacheDataLoaders()
        {
            var list = FindObjectsOfType<MonoBehaviour>()
                .OfType<IDataLoader>()
                .Cast<MonoBehaviour>()
                .ToArray();

            loaderBehaviours = list;

            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
}