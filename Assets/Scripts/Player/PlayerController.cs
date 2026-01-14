using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float jumpForce = 7f;

    Rigidbody rb;
    Vector3 direction;
    bool isGrounded;
    bool movementEnabled = true;

    [Header("Slope")]
    [SerializeField] float maxSlopeAngle = 35f;

    [Header("Extra Gravity / Air Control")]
    [SerializeField] float extraGravity = 20f;
    [SerializeField] float airControlMultiplier = 0.4f;

    [Header("Sliding on steep slopes")]
    [SerializeField] float slideDownForce = 5f;

    [Header("Acceleration / Deceleration")]
    [SerializeField] float acceleration = 20f;
    [SerializeField] float deceleration = 25f;

    float currentSpeed;

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

        if (!isGrounded)
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        ApplySlopeSlide();

        float targetSpeed = direction.magnitude * movementSpeed;

        if (currentSpeed < targetSpeed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.fixedDeltaTime);

        if (CanMoveOnSlope(direction))
        {
            float control = isGrounded ? 1f : airControlMultiplier;
            rb.MovePosition(rb.position + direction.normalized * currentSpeed * control * Time.fixedDeltaTime);
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        direction = transform.TransformDirection(new Vector3(h, 0f, v));

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x * 0.6f, 0f, rb.velocity.z * 0.6f);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
            rb.velocity = Vector3.zero;
    }

    bool CanMoveOnSlope(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero)
            return true;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, moveDir, out RaycastHit hit, 1f))
        {
            if (hit.collider.isTrigger)
                return true;

            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle <= maxSlopeAngle;
        }

        return true;
    }

    void ApplySlopeSlide()
    {
        if (!isGrounded)
            return;

        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1.2f))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > maxSlopeAngle)
            {
                Vector3 slideDir = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                rb.AddForce(slideDir * slideDownForce, ForceMode.Acceleration);
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Vector3.Angle(contact.normal, Vector3.up) <= maxSlopeAngle)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
