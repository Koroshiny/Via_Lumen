using UnityEngine;

public class LightAnchor : MonoBehaviour
{
    // -------------------------
    // Параметры фонаря
    // -------------------------
    public bool IsLit { get; private set; } = false;

    [SerializeField] private Renderer visualRenderer;   // Ссылка на меш, чтобы менять цвет
    [SerializeField] public Transform teleportPoint;   // Точка телепорта

    private Color defaultColor = Color.gray;
    private Color litColor = Color.yellow;

    void Start()
    {
        if (visualRenderer != null)
            visualRenderer.material.color = defaultColor;
    }

    // -------------------------
    // Зажечь фонарь
    // -------------------------
    public void LightUp()
    {
        IsLit = true;
        if (visualRenderer != null)
            visualRenderer.material.color = litColor;
    }

    // -------------------------
    // Потушить фонарь
    // -------------------------
    public void Extinguish()
    {
        IsLit = false;
        if (visualRenderer != null)
            visualRenderer.material.color = defaultColor;
    }

    // -------------------------
    // Подсветка при взгляде
    // -------------------------
    public void SetHighlight(bool highlight)
    {
        if (visualRenderer == null) return;

        if (highlight && IsLit)
            visualRenderer.material.color = Color.cyan; // подсветка при взгляде
        else if (IsLit)
            visualRenderer.material.color = litColor;
        else
            visualRenderer.material.color = defaultColor;
    }

    // -------------------------
    // Точка телепорта для игрока
    // -------------------------
    public Transform GetTeleportPoint()
    {
        return teleportPoint != null ? teleportPoint : this.transform;
    }
}
