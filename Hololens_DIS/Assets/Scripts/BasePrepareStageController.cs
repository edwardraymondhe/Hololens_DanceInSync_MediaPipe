using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public abstract class BasePrepareStageController : MonoBehaviour
{
    public GameObject objectCollection;
    public GameObject objectPrefab;
    public bool isCollectionRefresh = false;

    protected virtual void SpawnCollection()
    {
        for (int i = objectCollection.transform.childCount - 1; i >= 0; i--)
            Destroy(objectCollection.transform.GetChild(i).gameObject);
    }

    public virtual void InitStage(bool acrossStage)
    {
    }

    public virtual void NextStage()
    {
        GlobalController.Instance.NextStage();
    }

    public virtual void PrevStage()
    {
        GlobalController.Instance.PrevStage();
    }

    protected IEnumerator RefreshCollection(GameObject gridObjectCollection)
    {
        yield return new WaitForUpdate();

        gridObjectCollection.GetComponent<GridObjectCollection>().UpdateCollection();
    }
}
