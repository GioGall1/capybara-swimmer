using UnityEngine;
using UnityEngine.UI;

public class BreathingManager : MonoBehaviour
{
    private const float maxBreath = 100f;
    private float currentBreath;
    private const float breathDecreaseRate = 1f; // 1% в секунду

    public Text breathText;

    private bool isUnderwater = false;

    void Start()
    {
        currentBreath = maxBreath;
        UpdateBreathUI();
    }

    public void StartBreathing()
    {
        if (isUnderwater) return;

        isUnderwater = true;
        InvokeRepeating(nameof(DecreaseBreath), 1f, 1f); // ⏱️ строго каждую секунду
    }

    public void StopBreathing()
    {
        if (!isUnderwater) return;

        isUnderwater = false;
        CancelInvoke(nameof(DecreaseBreath));
    }

    void DecreaseBreath()
    {
        if (!isUnderwater) return;

        currentBreath -= breathDecreaseRate;
        currentBreath = Mathf.Clamp(currentBreath, 0f, maxBreath);

        UpdateBreathUI();

        if (currentBreath <= 0f)
        {
            Debug.Log("Капибара задыхается!");
            // Здесь можно вызвать конец игры, анимацию, звук и т.п.
        }
    }

    void UpdateBreathUI()
    {
        if (breathText != null)
        {
            breathText.text = "Дыхание: " + Mathf.RoundToInt(currentBreath) + "%";
        }
    }
}