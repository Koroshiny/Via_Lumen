using UnityEngine;

public class MoonLightSync : MonoBehaviour
{
    public Light directionalLight;
    public Transform moon;

    public float distance = 1000f;

    void LateUpdate()
    {
        if (!directionalLight || !moon) return;

        // направление света
        Vector3 dir = directionalLight.transform.forward;

        // луна ¬—≈√ƒј там, откуда светит directional light
        moon.position = -dir * distance;

        // чтобы она смотрела в центр сцены
        moon.LookAt(Vector3.zero);
    }
}