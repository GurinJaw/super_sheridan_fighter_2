using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public delegate void TriggerEnterDelegate(Collider _other);
    public TriggerEnterDelegate OnTrigger = null;

    private void OnTriggerEnter(Collider _other)
    {
        if (null != OnTrigger)
            OnTrigger(_other);
    }
}
