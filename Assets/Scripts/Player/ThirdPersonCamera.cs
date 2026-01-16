using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] GameObject player;

    [SerializeField]
    [Range(0.5f, 2f)]
    float mouseSense = 1f;

    [SerializeField]
    [Range(-60, 0)]
    int lookUp = -15;

    [SerializeField]
    [Range(5, 65)]
    int lookDown = 20;

    Rigidbody playerRb;

    float rotationX;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerRb = player.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSense;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSense;

        // --- вертикаль камеры ---
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, lookUp, lookDown);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // --- горизонт игрока ---
        Quaternion deltaRotation = Quaternion.Euler(0f, mouseX, 0f);
        playerRb.MoveRotation(playerRb.rotation * deltaRotation);
    }

    // --- UI ACCESS ---
    public float MouseSensitivity => mouseSense;

    public void SetMouseSensitivity(float value)
    {
        mouseSense = value;
    }
}
