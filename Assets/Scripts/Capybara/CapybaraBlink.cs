using UnityEngine;

public class EyeBlinkByZ : MonoBehaviour
{
    public float minBlinkDelay = 2f;
    public float maxBlinkDelay = 5f;
    public float blinkDuration = 0.15f;
    public float hiddenZ = -20.7f; // За голову (уводи чуть больше текущего Z)
    private float originalZ;

    void Start()
    {
        originalZ = transform.localPosition.z;
        StartCoroutine(BlinkRoutine());
    }

    private System.Collections.IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minBlinkDelay, maxBlinkDelay));

            // Спрятать глаз
            Vector3 pos = transform.localPosition;
            pos.z = hiddenZ;
            transform.localPosition = pos;

            yield return new WaitForSeconds(blinkDuration);

            // Вернуть глаз
            pos.z = originalZ;
            transform.localPosition = pos;
        }
    }
}