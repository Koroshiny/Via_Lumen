using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float jumpForce = 7f;

    Rigidbody rb;
    Vector3 direction;
    bool isGrounded = true;
    bool movementEnabled = true;

    [Header("Slope")]
    [SerializeField] float maxSlopeAngle = 35f;

    [Header("Extra Gravity / Air Control")]
    [SerializeField, Tooltip("Дополнительная гравитация в воздухе для веса персонажа")]
    float extraGravity = 20f;

    [SerializeField, Tooltip("Множитель управления в воздухе")]
    float airControlMultiplier = 0.4f;

    [Header("Sliding on steep slopes")]
    [SerializeField, Tooltip("Сила сползания по склонам круче maxSlopeAngle")]
    float slideDownForce = 5f;

    [Header("Acceleration / Deceleration")]
    [SerializeField] float acceleration = 20f;  // скорость нарастания
    [SerializeField] float deceleration = 25f;  // скорость торможения

    float currentSpeed = 0f; // текущая горизонтальная скорость

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!movementEnabled)
        {
            direction = Vector3.zero;
            return;
        }

        HandleMovement();
    }

    void FixedUpdate()
    {
        if (!movementEnabled)
            return;

        // Extra gravity в воздухе
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }

        // Сползание на крутых склонах
        ApplySlopeSlide();

        // -------------------------
        // Acceleration / Deceleration
        // -------------------------
        float targetSpeed = direction.magnitude * movementSpeed;

        if (currentSpeed < targetSpeed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        else if (currentSpeed > targetSpeed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.fixedDeltaTime);

        // движение
        if (CanMoveOnSlope(direction))
        {
            float control = isGrounded ? 1f : airControlMultiplier;
            rb.MovePosition(transform.position + direction.normalized * currentSpeed * control * Time.fixedDeltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        direction = transform.TransformDirection(new Vector3(h, 0f, v));

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Гасим горизонтальную скорость для более вертикального прыжка
            rb.velocity = new Vector3(rb.velocity.x * 0.6f, 0f, rb.velocity.z * 0.6f);

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;

            // currentSpeed сохраняется и после приземления будет нарастать с текущего значения
        }
    }

    // -------------------------
    // External control
    // -------------------------
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
    }

    // -------------------------
    // Can move on slope
    // -------------------------
    bool CanMoveOnSlope(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero)
            return true;

        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, moveDir);
        if (Physics.Raycast(ray, out RaycastHit hit, 1f))
        {
            // Игнорируем триггерные коллайдеры (фонари и другие)
            if (hit.collider.isTrigger)
                return true;

            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle <= maxSlopeAngle;
        }

        return true;
    }

    // -------------------------
    // Sliding on steep slopes
    // -------------------------
    void ApplySlopeSlide()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle > maxSlopeAngle && isGrounded)
            {
                Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                rb.AddForce(slideDir * slideDownForce, ForceMode.Acceleration);
            }
        }
    }
}
