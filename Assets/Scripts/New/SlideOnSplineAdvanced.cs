using UnityEngine;
using UnityEngine.Splines;

public class SlideOnSplineAdvanced : MonoBehaviour
{
    [Header("Spline")]
    public SplineContainer spline;
    public float baseSlideSpeed = 0.25f;
    public AnimationCurve speedCurve;

    [Header("Player Control")]
    [Range(0f, 1f)]
    public float sideControlStrength = 0.3f;
    public float maxSideOffset = 0.6f;

    [Header("References")]
    public PlayerMovementAdvanced movement;
    public Rigidbody rb;
    public ThirdPersonCam cam;

    [Header("Camera")]
    public ThirdPersonCam.CameraStyle slideCamera =
        ThirdPersonCam.CameraStyle.Topdown; // тут будет CM_Slide

    [Header("VFX")]
    public GameObject slideVFX;   // сюда ты положишь VFX
    public TrailRenderer trail;   // опционально, если используешь Trail

    private float t;
    private bool sliding;
    private float sideOffset;

    public void StartSlide()
    {
        if (spline == null) return;

        sliding = true;
        t = 0f;
        sideOffset = 0f;

        //movement.movementEnabled = false;
        rb.isKinematic = true;

        //cam.SwitchCameraStyle(slideCamera);

        // VFX ON
        if (slideVFX != null)
            slideVFX.SetActive(true);

        if (trail != null)
            trail.emitting = true;
    }

    private void Update()
    {
        if (!sliding) return;

        float curveSpeed = speedCurve != null
            ? speedCurve.Evaluate(t)
            : 1f;

        t += Time.deltaTime * baseSlideSpeed * curveSpeed;
        t = Mathf.Clamp01(t);

        Vector3 centerPos = (Vector3)spline.EvaluatePosition(t);
        Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;

        Vector3 side = Vector3.Cross(Vector3.up, tangent).normalized;

        float input = Input.GetAxis("Horizontal");
        sideOffset += input * sideControlStrength * Time.deltaTime;
        sideOffset = Mathf.Clamp(sideOffset, -maxSideOffset, maxSideOffset);

        transform.position = centerPos + side * sideOffset;
        transform.rotation = Quaternion.LookRotation(tangent);

        if (t >= 1f)
            StopSlide();
    }

    private void StopSlide()
    {
        sliding = false;

        rb.isKinematic = false;
        //movement.movementEnabled = true;

        //cam.SwitchCameraStyle(ThirdPersonCam.CameraStyle.Basic);

        // VFX OFF
        if (slideVFX != null)
            slideVFX.SetActive(false);

        if (trail != null)
            trail.emitting = false;
    }
}
