using UnityEngine;

using TMPro;

public class PlayerController : MonoBehaviour
{


    [SerializeField] float movementSpeed = 5f;
    [SerializeField] Rigidbody rb;
    [SerializeField] float jumpForce = 7f;

    float currentSpeed;
    Vector3 direction;

    bool isGrounded = true;


    void Start()
    {        
        rb = GetComponent<Rigidbody>();
        currentSpeed = movementSpeed;
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        direction = new Vector3(moveHorizontal, 0.0f, moveVertical);
        direction = transform.TransformDirection(direction);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            isGrounded = false;
        }
    }
    

    void FixedUpdate()
    {
        rb.MovePosition(transform.position + direction * currentSpeed * Time.deltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }
}
