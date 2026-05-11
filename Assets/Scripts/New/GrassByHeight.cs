using UnityEngine;

[ExecuteAlways]
public class GrassHeightFilter : MonoBehaviour
{
    [Header("Высота травы")]
    [SerializeField] float minHeight = 50f;
    [SerializeField] float maxHeight = 90f;

    [Header("Слой травы")]
    [SerializeField] int detailLayer = 0;

    [ContextMenu("Filter Grass")]
    public void FilterGrass()
    {
        Terrain terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogError($"Terrain not found on {gameObject.name}");
            return;
        }

        TerrainData td = terrain.terrainData;

        int detailWidth = td.detailWidth;
        int detailHeight = td.detailHeight;

        // Получаем уже существующую траву
        int[,] details = td.GetDetailLayer(
            0,
            0,
            detailWidth,
            detailHeight,
            detailLayer
        );

        for (int y = 0; y < detailHeight; y++)
        {
            for (int x = 0; x < detailWidth; x++)
            {
                // Перевод координат detail map -> heightmap
                int hx = Mathf.FloorToInt(
                    (float)x / detailWidth * (td.heightmapResolution - 1)
                );

                int hy = Mathf.FloorToInt(
                    (float)y / detailHeight * (td.heightmapResolution - 1)
                );

                // Высота именно terrain heightmap
                float terrainHeight = td.GetHeight(hx, hy);

                // Если высота НЕ подходит - удаляем траву
                if (terrainHeight < minHeight || terrainHeight > maxHeight)
                {
                    details[y, x] = 0;
                }
            }
        }

        // Применяем изменения
        td.SetDetailLayer(0, 0, detailLayer, details);

        Debug.Log($"Grass filtered on {gameObject.name}");
    }
}