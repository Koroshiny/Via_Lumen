using UnityEngine;
using UnityEngine.UI;

public class CameraSettingsUI : MonoBehaviour
{
    [SerializeField] ThirdPersonCamera thirdPersonCamera;
    [SerializeField] Slider sensitivitySlider;

    void Start()
    {
        if (thirdPersonCamera == null || sensitivitySlider == null)
            return;

        sensitivitySlider.minValue = 0.5f;
        sensitivitySlider.maxValue = 2f;
        sensitivitySlider.value = thirdPersonCamera.MouseSensitivity;

        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    void SetSensitivity(float value)
    {
        thirdPersonCamera.SetMouseSensitivity(value);
    }
}
