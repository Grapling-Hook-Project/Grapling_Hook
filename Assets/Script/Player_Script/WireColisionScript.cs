using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireColisionScript : MonoBehaviour
{
    private bool wireColl;
    private void Update()
    {
        wireColl = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        wireColl = true;
    }
    public bool WireContact()
    {
        return wireColl;
    }
}
