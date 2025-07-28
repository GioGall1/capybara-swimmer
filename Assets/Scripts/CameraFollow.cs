using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private bool followEnabled = false;

    void LateUpdate()
    {
        if (!followEnabled || target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void EnableFollow()
    {
        followEnabled = true;
    }

    public void DisableFollow()
    {
        followEnabled = false;
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}