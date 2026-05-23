using UnityEngine;

public class TreeRandomizer : MonoBehaviour
{
    public GameObject[] treePrefabs;

    [Header("Scale")]
    public Vector2 scaleRange = new Vector2(0.8f, 1.3f);

    [Header("Rotation")]
    public Vector2 rotationRange = new Vector2(0f, 360f);

    [ContextMenu("Randomize Trees")]
    void RandomizeTrees()
    {
        Terrain terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogError("No Terrain found");
            return;
        }

        TerrainData td = terrain.terrainData;
        TreeInstance[] trees = td.treeInstances;

        if (trees.Length == 0)
        {
            Debug.Log("No trees to randomize");
            return;
        }

        TreePrototype[] prototypes = new TreePrototype[treePrefabs.Length];

        // создаем новые prototypes
        for (int i = 0; i < treePrefabs.Length; i++)
        {
            TreePrototype tp = new TreePrototype();
            tp.prefab = treePrefabs[i];
            prototypes[i] = tp;
        }

        td.treePrototypes = prototypes;

        for (int i = 0; i < trees.Length; i++)
        {
            TreeInstance t = trees[i];

            // случайный prefab
            t.prototypeIndex = Random.Range(0, prototypes.Length);

            // scale
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            t.widthScale = scale;
            t.heightScale = scale;

            // rotation (очень важно - в радианах)
            t.rotation = Random.Range(rotationRange.x, rotationRange.y) * Mathf.Deg2Rad;

            trees[i] = t;
        }

        td.treeInstances = trees;

        Debug.Log("Trees randomized");
    }
}