using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField]
    UnityEvent<Collider> onTriggerStay;

    private void OnTriggerStay(Collider other)
    {
        onTriggerStay.Invoke(other);
    }
}
