using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Object Pool")]
public class ObjectPoolSO : ScriptableObject
{
    [SerializeField] private PoolableObject poolableObject;
    [SerializeField] private int initialSize;

    private Queue<PoolableObject> pool = new Queue<PoolableObject>();
    private GameObject poolParentObject;

    private void Initialize()
    {
        //If we don't have a parent object in the scene, we create one
        if (poolParentObject == null)
        {
            poolParentObject = new GameObject(poolableObject.name + " (POOL)");
        }

        //Create the initial pool
        while (pool.Count < initialSize)
        {
            PoolableObject objToPool = CreatePoolableObject();
            ReturnObjectToPool(objToPool);
        }
    }

    public PoolableObject GetPooledObject()
    {
        Initialize();

        PoolableObject poolObj = pool.Count > 0 && !pool.Peek().gameObject.activeSelf ? pool.Dequeue() : CreatePoolableObject();
        poolObj.gameObject.SetActive(true);
        return poolObj;
    }

    public T GetPooledObject<T>() where T : PoolableObject
    {
        PoolableObject poolObj = GetPooledObject();
        T component = poolObj.GetComponent<T>();
        return component;
    }

    private PoolableObject CreatePoolableObject()
    {
        PoolableObject poolObj = Instantiate(poolableObject);
        poolObj.SetPoolParent(this);
        return poolObj;
    }

    public void ReturnObjectToPool(PoolableObject poolObj)
    {
        poolObj.transform.SetParent(poolParentObject.transform);
        poolObj.gameObject.SetActive(false);
        pool.Enqueue(poolObj);
    }
}