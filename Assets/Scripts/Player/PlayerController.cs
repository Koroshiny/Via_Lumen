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

        rb.MovePosition(transform.position + direction * movementSpeed * Time.deltaTime);
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
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // -------------------------
    // External control
    // -------------------------
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
    }




}
