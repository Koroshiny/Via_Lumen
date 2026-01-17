using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7f;

    [Header("Ground")]
    [SerializeField] float groundCheckDistance = 1.1f;

    [Header("Slope")]
    [SerializeField] float maxSlopeAngle = 35f;

    Rigidbody rb;
    Vector3 inputDirection;
    bool isGrounded;
    bool movementEnabled = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (!movementEnabled)
        {
            inputDirection = Vector3.zero;
            return;
        }

        HandleInput();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            Jump();
    }

    void FixedUpdate()
    {
        CheckGrounded();

        if (!movementEnabled || inputDirection == Vector3.zero)
            return;

        // Ограничиваем движение по склонам
        if (!CanMoveOnSlope(inputDirection))
            return;

        // Нормализуем диагональ, чтобы скорость не увеличивалась
        Vector3 move = inputDirection.normalized * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + move);
    }

    void HandleInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Преобразуем в локальные координаты
        inputDirection = transform.TransformDirection(new Vector3(h, 0f, v));
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance);
    }

    bool CanMoveOnSlope(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero) return true;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, moveDir, out RaycastHit hit, 1f))
        {
            if (hit.collider.isTrigger) return true;
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle <= maxSlopeAngle;
        }

        return true;
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
            rb.velocity = Vector3.zero;
    }
}
