using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public delegate void TriggerEnterDelegate(Collider other);
    public TriggerEnterDelegate OnTrigger = null;

    private void OnTriggerEnter(Collider other)
    {
        if (null != OnTrigger)
            OnTrigger(other);
    }
}
