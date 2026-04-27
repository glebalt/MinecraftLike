using UnityEngine;

public class VoxelDebug : MonoBehaviour
{
    public bool highlighted = false;
    public Material highlightMaterial;
    private GameObject voxelHighlightCube;
    private Vector3 currentVelocity;
   
    void Start()
    {
        voxelHighlightCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        voxelHighlightCube.name = "Voxel Highlight";
        voxelHighlightCube.transform.localScale = new Vector3(1.01f, 1.01f, 1.01f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (highlighted)
        {
            
        }
    }

    public void HighlightVoxel(Vector3 position,VoxelChunk voxel)
    {
        position.x += 0.5f + voxel.connectedGmb.transform.position.x;
        position.y += 0.5f +voxel.connectedGmb.transform.position.y;
        position.z += 0.5f + voxel.connectedGmb.transform.position.z;
        voxelHighlightCube.transform.position = Vector3.SmoothDamp(voxelHighlightCube.transform.position ,position,ref currentVelocity,0.05f);
    }
}
