using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] public ReplayContolloer replayContolloer;
    [SerializeField] private Transform point;

    // Start is called before the first frame update
    void OnCollision(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            replayContolloer.respornPoint = point;
            Debug.Log("point");
        }
    }

    void Update()
    {

    }
}
