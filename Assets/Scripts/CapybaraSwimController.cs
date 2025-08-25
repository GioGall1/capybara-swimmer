using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CapybaraSwimController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float verticalSpeed = 1.5f;
    public float rotationFixSpeed = 8f; // ✅ скорость фиксации поворота
    public float boostedVerticalSpeed = 10f;   // 🥥 ускорение от кокоса
    public float boostDuration = 5f;          // ⏱️ сколько длится
    public float coconutBoostForce = 10f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Coroutine boostCoroutine;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1.2f;
        // originalVerticalSpeed = verticalSpeed;
        this.enabled = false; // ❌ по умолчанию управление выключено
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
    }

    void FixedUpdate()
    {

        rb.velocity = moveDirection;

        Debug.Log("🔽 rb.velocity.y = " + rb.velocity.y);

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
    

    public void ApplyCoconutBoost()
{
    Debug.Log("🐹 Вызван ApplyCoconutBoost!");
    if (boostCoroutine != null)
        StopCoroutine(boostCoroutine);

    boostCoroutine = StartCoroutine(BoostVerticalSpeed());
}

private IEnumerator BoostVerticalSpeed()
{
    float original = verticalSpeed;
    verticalSpeed = boostedVerticalSpeed;
    Debug.Log("🥥 Кокос: вертикальная скорость увеличена до " + verticalSpeed);

    yield return new WaitForSeconds(boostDuration);

    verticalSpeed = original;
    Debug.Log("🧘 Кокос эффект закончился, вертикальная скорость возвращена: " + verticalSpeed);
}
}