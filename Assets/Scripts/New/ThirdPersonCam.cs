using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    public Transform orientation;
    public Transform player;
    public Transform playerObj;

    public GameObject thirdPersonCam;
    public GameObject combatCam;
    public GameObject topDownCam;

    public CameraStyle currentStyle;

    public enum CameraStyle
    {
        Basic,
        Combat,
        Topdown
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SwitchCameraStyle(CameraStyle.Basic);
    }

    private void Update()
    {
        Vector3 viewDir = player.position -
            new Vector3(transform.position.x, player.position.y, transform.position.z);

        orientation.forward = viewDir.normalized;
    }

    public void SwitchCameraStyle(CameraStyle newStyle)
    {
        thirdPersonCam.SetActive(false);
        combatCam.SetActive(false);
        topDownCam.SetActive(false);

        if (newStyle == CameraStyle.Basic)
            thirdPersonCam.SetActive(true);

        if (newStyle == CameraStyle.Combat)
            combatCam.SetActive(true);

        if (newStyle == CameraStyle.Topdown)
            topDownCam.SetActive(true);

        currentStyle = newStyle;
    }
}
