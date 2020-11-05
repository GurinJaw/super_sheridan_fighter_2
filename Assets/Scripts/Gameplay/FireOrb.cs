using System.Collections;
using UnityEngine;

public class FireOrb : MonoBehaviour
{
    private const int destroyTime = 10;


    private void Start()
    {
        StartCoroutine(DestroyRoutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Damage"))
        {
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }
}
