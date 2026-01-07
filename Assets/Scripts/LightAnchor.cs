using UnityEngine;

public class LightAnchor : MonoBehaviour
{
    [Header("State")]
    public bool IsLit = false;

    [Header("Teleport")]
    public Transform teleportPoint;

    [Header("Visuals")]
    public Renderer[] renderers; // все рендереры в префабе, которые нужно подсвечивать
    private Color defaultColor = Color.yellow;
    private Color highlightColor = Color.white;

    void Start()
    {
        // Если рендереры не назначены, пробуем найти их на дочерних объектах
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        UpdateVisual();
    }

    public void LightUp()
    {
        IsLit = true;
        UpdateVisual();
    }

    public void SetHighlight(bool highlight)
    {
        if (!IsLit) return;

        foreach (var rend in renderers)
        {
            if (highlight)
                rend.material.color = highlightColor;
            else
                rend.material.color = defaultColor;
        }
    }

    private void UpdateVisual()
    {
        Color c = IsLit ? defaultColor : Color.gray;
        foreach (var rend in renderers)
        {
            rend.material.color = c;
        }
    }
}
