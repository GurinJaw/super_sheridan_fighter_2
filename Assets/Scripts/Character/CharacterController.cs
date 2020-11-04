using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public int playerNumber = 1;

    private DamageTrigger[] damageTriggers = null;

    // Start is called before the first frame update
    void Start()
    {
        damageTriggers = GetComponentsInChildren<DamageTrigger>();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    void SubscribeToEvents()
    {
        SubscribeToTriggerEvents();
    }

    void UnsubscribeFromEvents()
    {
        UnsubscribeFromTriggerEvents();
    }

    void SubscribeToTriggerEvents()
    {
        for (int i = 0; i < damageTriggers.Length; i++)
        {
            damageTriggers[i].OnTrigger += OnTrigger;
        }
    }

    void UnsubscribeFromTriggerEvents()
    {
        for (int i = 0; i < damageTriggers.Length; i++)
        {
            damageTriggers[i].OnTrigger -= OnTrigger;
        }
    }

    void OnTrigger(Collider other)
    {
        if (playerNumber == 1) return;

        if (other.tag == "Player_1_Damage")
            Debug.Log("Trigger enter! " + other.tag);
    }
}
