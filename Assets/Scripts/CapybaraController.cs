using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapybaraAnimationController))]
public class CapybaraController : MonoBehaviour
{
    public float minJumpForce = 5f;
    public float maxJumpForce = 12f;
    public float chargeTime = 1.5f;
    public float maxSpeed = 6f;

    [Header("Анимированная капибара")]
    public GameObject swimCapybaraPrefab; // Твой перфаб KapiNEWPNG_0

    private Rigidbody2D rb;
    private CapybaraAnimationController anim;
    private bool isCharging = false;
    private bool hasJumped = false;
    private float chargeAmount = 0f;
    private bool hasSpawnedSwimCapybara = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<CapybaraAnimationController>();

     
        rb.drag = 0.5f;
        rb.angularDrag = 0.3f;
        rb.centerOfMass = new Vector2(0f, -0.7f);
        rb.freezeRotation = false;

        anim.SetIdle();
    }

    void Update()
    {
        if (!hasJumped)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isCharging = true;
                chargeAmount = 0f;
                anim.SetBend();
            }

            if (isCharging)
            {
                chargeAmount += Time.deltaTime / chargeTime;
                chargeAmount = Mathf.Clamp01(chargeAmount);
            }

            if (Input.GetMouseButtonUp(0) && isCharging)
            {
                Jump();
            }
        }

        if (hasJumped && rb.velocity.y < -0.2f)
        {
            anim.SetFall();
        }

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    void Jump()
    {
        isCharging = false;
        hasJumped = true;
        anim.SetJump();
        GetComponent<CapybaraAnimationController>().enabled = false;

        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargeAmount);
        Vector2 direction = (Vector2)(transform.right * 0.6f + transform.up * 0.4f).normalized;

        rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Water") && !hasSpawnedSwimCapybara)
        {
            hasSpawnedSwimCapybara = true;

            //КАМЕРА
            CameraFollow camFollow = FindObjectOfType<CameraFollow>(); // Добавление динамичной камеры
            camFollow.EnableFollow();
            camFollow.SetOffset(new Vector3(0f, -8f, -10f)); // Центрируе
             //АНИМАЦИЯ
            Invoke(nameof(SpawnSwimmingCapybara), 2f); // ⏳ через 2 секунды спавним анимированную
             //ДЫХАНИЕ
            if (BreathingManager.Instance != null)
                {
                    BreathingManager.Instance.StartBreathing();
                }
        }
    }

    void SpawnSwimmingCapybara()
    {
        // ✅ Спавним анимированную капибару
        GameObject swimmingCapybara = Instantiate(
            swimCapybaraPrefab,
            transform.position,
            Quaternion.identity
        );

        // ✅ Запускаем дыхание (если на prefab'е стоит BreathingTrigger)
        var breathManager = BreathingManager.Instance;
        if (breathManager != null)
        {
            breathManager.StartBreathing();
        }

        // ✅ Включаем управление через 0.1 секунды
        var swimController = swimmingCapybara.GetComponent<CapybaraSwimController>();
        if (swimController != null)
        {
            swimController.ActivateAfterDelay(0.1f);
        }

        // ✅ Меняем таргет камеры на новую капибару
        var cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(swimmingCapybara.transform);
        }

        // 💥 Удаляем старую "сухую" капибару
        Destroy(gameObject);
    }
}