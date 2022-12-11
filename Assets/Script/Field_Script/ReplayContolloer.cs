using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ReplayContolloer : MonoBehaviour
{
    public Transform respornPoint;
    [SerializeField] public GameObject playerPrefab;
    public bool alive = true;
    void Start()
    {
    }

    void Update()
    {
        GameObject playerObj = GameObject.Find(playerPrefab.name);
        if (alive == false)
        {
            GameObject newPlayerObj = Instantiate(playerPrefab);
            newPlayerObj.name = playerPrefab.name;
            newPlayerObj.transform.position = respornPoint.transform.position;
            Debug.Log("ê∂ê¨");
            alive = true;
        }
    }
}
