using System.Collections.Generic;
using UnityEngine;

public class meshCreator : MonoBehaviour
{
    private Mesh m_Mesh;
    List<Vector3> vertices = new List<Vector3>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Spawn()
    {
        Vector3 basePos = new Vector3(0, 0, 0);


        for (int i = 0; i < 4; i++)
        {
            
        }
        // Create an empty Mesh.
        m_Mesh = new Mesh();
        m_Mesh.name = "Procedural Triangle";
        
        m_Mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0,1,0),
            new Vector3(1, 1, 0),
        };

        m_Mesh.triangles = new int[]
        {
            3,1,2,3,0,1,3,4,2
        };
        
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = m_Mesh;
    }
    
    void OnDestroy()
    {
        // Destroy the mesh to prevent memory leaks.
        if (m_Mesh != null)
            Destroy(m_Mesh);

        
    }

}
