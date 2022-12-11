using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlayer : MonoBehaviour
{
    [SerializeField] public ReplayContolloer replayPlayerContolloer;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Destroy(collision.gameObject);
            Debug.Log("deth");
            replayPlayerContolloer.alive = false;
        }
    }
}
