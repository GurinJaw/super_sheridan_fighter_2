using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraDefaultPos = null;

    private bool initialized = false;

    private Transform targetA = null;
    private Transform targetB = null;

    private const float heightOffset = 1f;
    private const float maxCameraDistance = 7f;
    private const float minCharacterDistance = 6f;
    private const float maxCharacterDistance = 15f;

    void Start()
    {
    }

    void Update()
    {
        if (!initialized) return;

        UpdatePositionAndZoom();
    }

    public void Initialize(Transform _targetA, Transform _targetB)
    {
        targetA = _targetA;
        targetB = _targetB;

        initialized = true;
    }

    void UpdatePositionAndZoom()
    {
        Vector3 targetPosition = (targetA.position + targetB.position) / 2f;

        targetPosition.y = transform.position.y;
        targetPosition.z = transform.position.z;

        Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, 3f * Time.deltaTime);

        float charDistance = Vector3.Distance(targetA.position, targetB.position);
        charDistance = Mathf.Clamp(charDistance, minCharacterDistance, maxCharacterDistance);

        float maxCamDistance = cameraDefaultPos.position.z;
        float minCamDistance = cameraDefaultPos.position.z - maxCameraDistance;

        float newDistance = (((maxCharacterDistance - charDistance) / (maxCharacterDistance - minCharacterDistance)) * (maxCamDistance - minCamDistance)) + minCamDistance;

        newPos.z = Mathf.Clamp(newDistance, minCamDistance, maxCamDistance);


        transform.position = newPos;
    }
}
