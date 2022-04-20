using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStageController : MonoBehaviour
{
    public GameObject objectCollection;
    public GameObject objectPrefab;

    public virtual void SpawnCollection()
    {
        for (int i = objectCollection.transform.childCount - 1; i >= 0; i--)
            Destroy(objectCollection.transform.GetChild(i).gameObject);
    }
}
