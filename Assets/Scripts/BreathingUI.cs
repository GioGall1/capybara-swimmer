using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BreathingUI : MonoBehaviour
{
    [SerializeField] private BreathingManager breathingManager;
    [SerializeField] private Text breathText;

   void Start()
{
    if (BreathingManager.Instance != null)
    {
        BreathingManager.Instance.onBreathChanged.AddListener(UpdateBreathUI);
        Debug.Log("🎯 UI подписался на BreathingManager");
    }
    else
    {
        Debug.LogWarning("❌ BreathingManager.Instance не найден");
    }
}

   private IEnumerator WaitForCapybara()
{
    yield return new WaitForSeconds(0.2f); // ждём появления капибары

    breathingManager = FindObjectOfType<BreathingManager>();

    if (breathingManager != null)
    {
        breathingManager.onBreathChanged.AddListener(UpdateBreathUI);
        Debug.Log("🎯 Подписались на дыхание UI (из корутины)");
    }
    else
    {
        Debug.LogWarning("❌ BreathingManager не найден даже после ожидания");
    }
}

    private void UpdateBreathUI(float value)
{
   

    if (breathText != null)
    {
        breathText.text = "Дыхание: " + Mathf.RoundToInt(value) + "%";
    }
    else
    {
        Debug.LogWarning("❌ breathText всё же NULL!");
    }
}
}