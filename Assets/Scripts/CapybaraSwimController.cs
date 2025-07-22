using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CapybaraSwimController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float verticalSpeed = 1.5f;
    public float rotationFixSpeed = 8f; // ✅ скорость фиксации поворота

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1.2f;
        this.enabled = false; // ❌ по умолчанию управление выключено
    }

    // ✅ Метод для включения управления с задержкой
    public void ActivateAfterDelay(float delay)
    {
        Invoke(nameof(EnableControl), delay);
    }

    void EnableControl()
    {
        this.enabled = true;
    }

    void Update()
    {
        moveDirection = Vector2.down * verticalSpeed;

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x < Screen.width / 2)
            {
                moveDirection += Vector2.left * moveSpeed;
                Flip(-1);
            }
            else
            {
                moveDirection += Vector2.right * moveSpeed;
                Flip(1);
            }
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.position.x < Screen.width / 2)
            {
                moveDirection += Vector2.left * moveSpeed;
                Flip(-1);
            }
            else
            {
                moveDirection += Vector2.right * moveSpeed;
                Flip(1);
            }
        }
#endif
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
}