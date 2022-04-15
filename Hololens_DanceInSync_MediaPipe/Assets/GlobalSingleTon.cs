using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GlobalSingleTon<T> : MonoBehaviour where T : GlobalSingleTon<T>
{
    protected static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Finds the GlobalController Object
                GameObject go = GameObject.FindGameObjectWithTag("GlobalController");
                if (go == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("GlobalController");
                    go = Instantiate(prefab);
                    DontDestroyOnLoad(go);
                }
                else
                {
                    Debug.Log("Found GlobalController");
                }

                _instance = go.GetComponent<T>();
                if (_instance == null)
                    _instance = go.AddComponent<T>();

                go.name = "GlobalController";
            }
            return _instance;
        }
    }
}