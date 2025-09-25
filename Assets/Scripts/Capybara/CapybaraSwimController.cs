using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CapybaraSwimController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float verticalSpeed = 1.5f;
    public float rotationFixSpeed = 8f; // ✅ скорость фиксации поворота
    public float boostedVerticalSpeed = 10f;   // 🥥 ускорение от кокоса
    public float boostDuration = 5f;          // ⏱️ сколько длится ускорение
    public float coconutBoostForce = 10f;
    public GameObject bubblePrefab;
    public Transform mouthPoint;
    public float bubbleSpawnIntervalMin = 1.5f;
    public float bubbleSpawnIntervalMax = 4f;
    public int boostBubbleCount = 10;
    public float bubbleSpread = 0.5f;   

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Coroutine boostCoroutine;
    private Animator anim;
    private float nextBubbleTime = 0f;
    private float originalVerticalSpeed;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1.2f;
        // originalVerticalSpeed = verticalSpeed;
        anim = GetComponent<Animator>();
        this.enabled = false; // ❌ по умолчанию управление выключено
        
        originalVerticalSpeed = verticalSpeed; // 🧠 запоминаем изначальную скорость
        
    }

    // ✅ Метод для включения управления с задержкой
    public void ActivateAfterDelay(float delay)
    {
        Invoke(nameof(EnableControl), delay);
    }

    void EnableControl()
    {
        rb.gravityScale = 1.2f;
        this.enabled = true;
    }

    void Update()
    {
        float horizontal = 0f;

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            horizontal = Input.mousePosition.x < Screen.width / 2 ? -1f : 1f;
            Flip(horizontal);
        }
#else
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);
        horizontal = touch.position.x < Screen.width / 2 ? -1f : 1f;
        Flip(horizontal);
    }
#endif

        // движение вниз (всегда) + влево/вправо (если надо)
        moveDirection = new Vector2(horizontal * moveSpeed, -verticalSpeed);

        if (Time.time >= nextBubbleTime)
    {
        SpawnBubble();
        nextBubbleTime = Time.time + Random.Range(bubbleSpawnIntervalMin, bubbleSpawnIntervalMax);
    }
    }

    void FixedUpdate()
    {

        rb.velocity = moveDirection;

        // ✅ капибара всегда вертикальная (плавно выравнивается)
        Quaternion targetRotation = Quaternion.identity;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationFixSpeed * Time.fixedDeltaTime);
    }

    void Flip(float direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = direction < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
    
    void SpawnBubble()
    {
        Instantiate(bubblePrefab, mouthPoint.position, Quaternion.identity);
    }
    

    public void ApplyCoconutBoost()
    {
        Debug.Log("🐹 Вызван ApplyCoconutBoost!");

        anim.SetTrigger("Boost"); // 💥 Включаем анимацию ускорения
        
        ApplyBoostBubbles();

        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            verticalSpeed = originalVerticalSpeed;         // 🧼 сбрасываем прошлое ускорение
            anim.SetBool("isBoosting", false);             // ⛔ сброс прошлой анимации
        }

        anim.SetBool("isBoosting", true); // ✅ запускаем анимацию

        boostCoroutine = StartCoroutine(BoostVerticalSpeed());
    }
    
    private void ApplyBoostBubbles()
{
    for (int i = 0; i < boostBubbleCount; i++)
    {
        Vector3 offset = new Vector3(Random.Range(-bubbleSpread, bubbleSpread), 0f, 0f);
        GameObject bubble = Instantiate(bubblePrefab, mouthPoint.position + offset, Quaternion.identity);
        
        Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float forceY = Random.Range(2f, 4f);
            rb.velocity = new Vector2(0f, forceY); // всплытие
        }

        Destroy(bubble, 2f); // пузырь исчезает через 2 секунды
    }
}

private IEnumerator BoostVerticalSpeed()
    {
        float original = verticalSpeed;
        verticalSpeed = boostedVerticalSpeed;
        Debug.Log("🥥 Кокос: вертикальная скорость увеличена до " + verticalSpeed);

        yield return new WaitForSeconds(boostDuration);

        verticalSpeed = original;

        anim.SetBool("isBoosting", false); // ✅ возвращаем анимацию обратно

        Debug.Log("🧘 Кокос эффект закончился, вертикальная скорость возвращена: " + verticalSpeed);
    }
}