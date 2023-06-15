using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHeatmap : MonoBehaviour
{



    [Tooltip("Terrain's GameObject reference")] public GameObject terrain;
    
    [Tooltip("The matrix of cells")] public GameObject[,] cellRefMatrix = new GameObject[8,8];
    [Tooltip("The heatmap")] public int[,] heatmap = new int[8,8];    
    
    [Tooltip("The colors for the heatmap")] public Material[] materials = new Material[6];

    [Tooltip("The posisitons to spawn people")] public Vector3[] positions = new Vector3[5];
    
    [Tooltip("Person model")] public GameObject person;
    
    [Tooltip("References to the people spawned")] public List<GameObject> peopleRefs = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        terrain = gameObject;
        
        // Initialize the cells references
        int i,j,k;
        
        GameObject rowGameObject;
        
        
        
        for (i = 0; i<8; i++)
        {
            rowGameObject = terrain.transform.Find($"Row_{i}").gameObject;
            
            for (j = 0; j < 8; j++)
            {
                cellRefMatrix[i,j] = rowGameObject.transform.Find($"Cell_{j}").gameObject;
            }
        }
        
        generateNewHeatmap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
 
 
    public int getSumHeatmap() 
    {
        int i,j;
        int total = 0;
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++) 
            {        
                total += heatmap[i,j];
            }
        }
        return total;
    }
    
    
    public void generateNewHeatmap()
    {
        
        // Apply Perlim noise to the terrain
        float perlinNoiseMax = 10f;
        float perlinNoiseXSeed = Random.Range(0, perlinNoiseMax);
        float perlinNoiseYSeed = Random.Range(0, perlinNoiseMax);
        string info;
        int i, j, k;
        float  x, y;
        int sampledValue;
        Material mat = null;
        
        for (i = 0; i < 8; i++)
        {
            info = "";
            for (j = 0; j < 8; j++) 
            {
                x = perlinNoiseXSeed + i * 0.2f;
                y = perlinNoiseYSeed + j * 0.2f;
                sampledValue = (int)(Mathf.Floor(Mathf.Clamp(Mathf.PerlinNoise(x, y), -1.0f, 1.0f ) * 6));
                heatmap[i,j] = sampledValue;
                try {
                    mat = materials[sampledValue];
                } catch (System.Exception e) {
                    Debug.Log($"materials: {materials.Length}, sampledValue: {sampledValue}");
                }
                cellRefMatrix[i,j].GetComponent<MeshRenderer>().material = mat;
                if (heatmap[i,j] > 0) {
                    Vector3 cellPos = cellRefMatrix[i,j].transform.position;
                    for (k = 0; k < heatmap[i,j]; k++) 
                    {
                        //Instantiate(person, cellPos + positions[k], Quaternion.identity);
                    }
                }

                info += $" {heatmap[i,j]} |";
            }
            //Debug.Log(info);
        }
    }  
}
