using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolableObject : MonoBehaviour
{
    protected ObjectPoolSO poolParent;

    public void SetPoolParent(ObjectPoolSO pool)
    {
        poolParent = pool;
    }

    public void ReturnToPool()
    {
        poolParent.ReturnObjectToPool(this);
    }
}
