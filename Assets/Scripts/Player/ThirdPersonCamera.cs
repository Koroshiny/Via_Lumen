using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] GameObject player;

    [Header("Mouse")]
    [SerializeField]
    [Range(0.5f, 2f)]
    float mouseSense = 1f;

    [Header("Vertical Clamp")]
    [SerializeField]
    [Range(-20, -10)]
    int lookUp = -15;

    [SerializeField]
    [Range(15, 25)]
    int lookDown = 20;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ⛔ не двигаем камеру на паузе
        if (Time.timeScale == 0f)
            return;

        float rotateX = Input.GetAxis("Mouse X") * mouseSense;
        float rotateY = Input.GetAxis("Mouse Y") * mouseSense;

        Vector3 rotCamera = transform.rotation.eulerAngles;
        Vector3 rotPlayer = player.transform.rotation.eulerAngles;

        // перевод угла в -180..180
        rotCamera.x = (rotCamera.x > 180) ? rotCamera.x - 360 : rotCamera.x;

        // вертикаль
        rotCamera.x -= rotateY;
        rotCamera.x = Mathf.Clamp(rotCamera.x, lookUp, lookDown);

        // горизонталь
        rotCamera.z = 0;
        rotPlayer.y += rotateX;

        transform.rotation = Quaternion.Euler(rotCamera);
        player.transform.rotation = Quaternion.Euler(rotPlayer);
    }

    // -------- UI ACCESS --------

    public float MouseSensitivity => mouseSense;

    public void SetMouseSensitivity(float value)
    {
        mouseSense = value;
    }
}
