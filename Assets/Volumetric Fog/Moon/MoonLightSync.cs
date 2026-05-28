using UnityEngine;

public class MoonLightSync : MonoBehaviour
{
    public Transform moon;

    void LateUpdate()
    {
        if (!moon) return;

        // Свет всегда направлен на луну
        transform.rotation =
            Quaternion.LookRotation(-moon.forward);

        // Optional:
        // чтобы gizmo света был в луне
        transform.position = moon.position;
    }
}