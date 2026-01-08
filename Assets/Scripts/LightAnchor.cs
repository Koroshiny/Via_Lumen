using UnityEngine;

public class LightAnchor : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool isLit;   // ? ГАЛОЧКА В ИНСПЕКТОРЕ
    public bool IsLit => isLit;

    [Header("References")]
    [SerializeField] private Renderer visualRenderer;
    [SerializeField] private Transform teleportPoint;

    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.gray;
    [SerializeField] private Color litColor = Color.yellow;
    [SerializeField] private Color highlightColor = Color.cyan;

    void Start()
    {
        UpdateVisual();
    }

    // -------------------------
    // Зажечь фонарь
    // -------------------------
    public void LightUp()
    {
        isLit = true;
        UpdateVisual();
    }

    // -------------------------
    // Потушить фонарь
    // -------------------------
    public void Extinguish()
    {
        isLit = false;
        UpdateVisual();
    }

    // -------------------------
    // Подсветка при взгляде
    // -------------------------
    public void SetHighlight(bool highlight)
    {
        if (visualRenderer == null) return;

        if (highlight && isLit)
            visualRenderer.material.color = highlightColor;
        else
            UpdateVisual();
    }

    // -------------------------
    // Обновление цвета
    // -------------------------
    void UpdateVisual()
    {
        if (visualRenderer == null) return;

        visualRenderer.material.color = isLit ? litColor : defaultColor;
    }

    // -------------------------
    // Точка телепорта
    // -------------------------
    public Transform GetTeleportPoint()
    {
        return teleportPoint != null ? teleportPoint : transform;
    }
}
