using UnityEngine;

public class BubbleRefill : MonoBehaviour
{
    public float refillAmount = 20f;

    [Header("Всплытие пузыря")]
    public float riseSpeed = 0.5f; // скорость всплытия вверх

    private void Update()
    {
        // медленное всплытие вверх
        transform.Translate(Vector2.up * riseSpeed * Time.deltaTime);
    }

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