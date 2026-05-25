using UnityEngine;

public class LightShaftsController : MonoBehaviour
{
    public Light directionalLight;
    public Camera cam;
    public Material shaftsMaterial;

    void LateUpdate()
    {
        if (!directionalLight || !cam || !shaftsMaterial) return;

        // направление света (куда светит)
        Vector3 lightDir = directionalLight.transform.forward;

        // переводим направление в viewport space (СТАБИЛЬНО, без world tricks)
        Vector3 viewportDir = cam.WorldToViewportPoint(
            cam.transform.position + lightDir * 1000f
        );

        Vector2 lightPos = new Vector2(viewportDir.x, viewportDir.y);

        shaftsMaterial.SetVector("_LightPos", new Vector4(lightPos.x, lightPos.y, 0, 0));
    }
}