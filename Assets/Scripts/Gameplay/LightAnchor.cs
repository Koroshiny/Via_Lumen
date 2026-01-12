using UnityEngine;

public class LightAnchor : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool isLit;
    public bool IsLit => isLit;

    [Header("Interaction")]
    [SerializeField] private bool playerInside;
    public bool PlayerInside => playerInside;

    [Header("Teleport Points")]
    [SerializeField] private Transform anchorViewPoint;   // точка для FPS камеры
    [SerializeField] private Transform exitPoint;         // точка телепорта игрока (ExitPoint)

    [Header("Visuals")]
    [SerializeField] private Renderer visualRenderer;
    [SerializeField] private GameObject flameVFX;
    [SerializeField] private GameObject selectVFX;

    [Header("Selection Zone")]
    [SerializeField] private Collider selectionZone; // триггерная зона выбора цели

    public Transform ViewPoint => anchorViewPoint;
    public Transform ExitPoint => exitPoint;

    void Start()
    {
        UpdateVisual();
        SetSelected(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    // -------------------------
    // Зажечь фонарь
    // -------------------------
    public void LightUp()
    {
        if (isLit) return;
        isLit = true;
        UpdateVisual();
    }

    public void Extinguish()
    {
        isLit = false;
        UpdateVisual();
    }

    // -------------------------
    // Подсветка выбранного фонаря
    // -------------------------
    public void SetSelected(bool selected)
    {
        if (selectVFX != null)
            selectVFX.SetActive(selected);
    }

    void UpdateVisual()
    {
        if (visualRenderer != null)
            visualRenderer.material.color = isLit ? Color.yellow : Color.gray;

        if (flameVFX != null)
            flameVFX.SetActive(isLit);
    }

    public Collider GetSelectionZone() => selectionZone;

    // -------------------------
    // Метод для получения точки телепорта игрока
    // -------------------------
    public Transform GetTeleportPoint()
    {
        return exitPoint != null ? exitPoint : transform;
    }

    void OnDrawGizmos()
    {
        if (anchorViewPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(anchorViewPoint.position, 0.15f);
            Gizmos.DrawLine(anchorViewPoint.position, anchorViewPoint.position + anchorViewPoint.forward * 0.5f);
        }

        if (exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(exitPoint.position, 0.15f);
        }

        if (selectionZone != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(selectionZone.bounds.center, selectionZone.bounds.extents.magnitude);
        }
    }
}
