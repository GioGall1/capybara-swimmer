using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BreathingManager : MonoBehaviour
{
    private const float maxBreath = 100f;
    private float currentBreath = maxBreath;
    private const float breathDecreaseRate = 1f;

    public UnityEvent<float> onBreathChanged;

    public static BreathingManager Instance { get; private set; }


    private bool isUnderwater = false;
    private bool deathStarted = false;

    void Start()
    {
        currentBreath = maxBreath;
        onBreathChanged?.Invoke(currentBreath);
   
    }
    
   void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); // 💡 сохраняется между сценами
    }
    else if (Instance != this)
    {
        Debug.LogWarning("🔁 Повторный BreathingManager уничтожается: " + gameObject.name);
        Destroy(gameObject); // уничтожается только лишний
    }
}

    public void StartBreathing()
    {
        Debug.Log("🫁 StartBreathing вызван!");

        if (isUnderwater)
        {
            Debug.Log("⛔ Уже под водой, выходим из StartBreathing");
            return;
        }

        isUnderwater = true;
        Debug.Log("✅ Дыхание запущено");
        InvokeRepeating(nameof(DecreaseBreath), 1f, 1f);

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


    onBreathChanged?.Invoke(currentBreath);

    if (currentBreath <= 0f)
    {
        Debug.Log("❗ Капибара задыхается!");
        StartCoroutine(HandleDeathSequence());
    }
    }

    public void RefillBreath(float amount)
    {
        Debug.Log($"До: {currentBreath}");

        currentBreath += amount;
        currentBreath = Mathf.Clamp(currentBreath, 0f, maxBreath);

        Debug.Log($"После: {currentBreath}");
        Debug.Log($"🫧 Пузырь восстановил дыхание на {amount}%. Текущее дыхание: {Mathf.RoundToInt(currentBreath)}%");
        onBreathChanged?.Invoke(currentBreath); // <<< вот это обновляет UI

    }
    
    public void ChangeBreath(float delta)
{
    currentBreath += delta; // delta может быть отрицательным (например, -10)
    currentBreath = Mathf.Clamp(currentBreath, 0f, maxBreath);
    onBreathChanged?.Invoke(currentBreath);

    if (currentBreath <= 0f)
    {
            Debug.Log("❗ Капибара задыхается!");
            StartCoroutine(HandleDeathSequence());
    }
}

    private IEnumerator HandleDeathSequence()
    {
        if (deathStarted) yield break;
        deathStarted = true;

        StopBreathing();

        var capy = FindObjectOfType<CapybaraDeathController>();
        if (capy != null)
            capy.PlayDeathPose();

        var swim = FindObjectOfType<CapybaraSwimController>();
        if (swim != null)
            swim.enabled = false;

        yield return new WaitForSeconds(2f);

        GameManager.Instance.GameOver();
    }
}