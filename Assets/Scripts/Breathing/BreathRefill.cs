using UnityEngine;

public class BubbleRefill : MonoBehaviour
{
    public float refillAmount = 20f;

  private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player") && BreathingManager.Instance != null)
    {
        BreathingManager.Instance.RefillBreath(refillAmount);
        Debug.Log("🫧 Пузырь восстановил дыхание на " + refillAmount + "%");
        Destroy(gameObject);
    }
}
}