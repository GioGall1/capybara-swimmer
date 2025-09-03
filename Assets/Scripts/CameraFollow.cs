using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private bool followEnabled = true; // включено по умолчанию

    void LateUpdate()
    {
        if (!followEnabled || target == null) return;

        // Мгновенное перемещение камеры за капибарой
        transform.position = target.position + offset;
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