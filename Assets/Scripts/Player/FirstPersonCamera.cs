using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 2f;

    float xRotation = 0f;
    Rigidbody playerRb;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerRb = transform.parent.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // --- вертикаль камеры ---
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // --- горизонт игрока ---
        Quaternion deltaRotation = Quaternion.Euler(0f, mouseX, 0f);
        playerRb.MoveRotation(playerRb.rotation * deltaRotation);
    }

    // если потом захочешь вынести в настройки
    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
    }
}
