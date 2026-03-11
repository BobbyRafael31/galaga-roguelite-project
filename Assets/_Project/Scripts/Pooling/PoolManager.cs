using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<int, object> _componentPools = new Dictionary<int, object>();
    private readonly Dictionary<int, Transform> _poolContainers = new Dictionary<int, Transform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        int prefabId = prefab.gameObject.GetInstanceID();

        if (!_componentPools.TryGetValue(prefabId, out object poolObj))
        {
            var newPool = CreatePool(prefab);
            _componentPools.Add(prefabId, newPool);
            poolObj = newPool;
        }

        var pool = (IObjectPool<T>)poolObj;
        T instance = pool.Get();

        instance.transform.SetPositionAndRotation(position, rotation);

        if (parent != null)
        {
            instance.transform.SetParent(parent);
        }

        return instance;
    }

    public void Release<T>(T instance) where T : Component
    {
        if (instance.TryGetComponent(out PooledItem item))
        {
            item.Release();
        }
        else
        {
            Debug.LogWarning($"[PoolManager] {instance.name} is missing a PooledItem. Destroying instead.");
            Destroy(instance.gameObject);
        }
    }

    private IObjectPool<T> CreatePool<T>(T prefab) where T : Component
    {
        int prefabId = prefab.gameObject.GetInstanceID();

        GameObject container = new GameObject($"[Pool] {prefab.name}");
        container.transform.SetParent(this.transform);
        _poolContainers.Add(prefabId, container.transform);

        IObjectPool<T> pool = null;
        pool = new ObjectPool<T>(
            createFunc: () =>
            {
                T instance = Instantiate(prefab, _poolContainers[prefabId]);
                PooledItem item = instance.gameObject.AddComponent<PooledItem>();

                item.Initialize(() => pool.Release(instance));
                return instance;
            },
            actionOnGet: (instance) => instance.gameObject.SetActive(true),
            actionOnRelease: (instance) => instance.gameObject.SetActive(false),
            actionOnDestroy: (instance) => Destroy(instance.gameObject),
            collectionCheck: false,
            defaultCapacity: 50,
            maxSize: 1000
        );
        return pool;
    }
}