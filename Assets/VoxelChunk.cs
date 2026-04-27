using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class VoxelChunk 
{
    public Voxel[] voxels;
    private int gridSize;
    public int GridSize { get { return this.gridSize; } }
    private const int scale = 1;
  
    public int iter;
    public Mesh mesh;
    public List<Vector3> verticies;
    public List<int> triangles;
    public List<Vector3> normals;
    public GameObject connectedGmb;
    private VoxelUtils _voxelUtils;
  
    public enum ChunkType
    {
        Underground,
        Ground
    }

    public VoxelChunk(int gridSize,GameObject gmb,ChunkType chunkType)
    {
        _voxelUtils = new VoxelUtils(this);
        connectedGmb = gmb;
        this.gridSize = gridSize;
        voxels = new Voxel[this.gridSize * this.gridSize * this.gridSize];
        
        GenerateChunk(chunkType,gmb);
    }

    void GenerateChunk(ChunkType chunkType,GameObject gmb)
    {
        if (chunkType == ChunkType.Underground)
        {
            SpawnVoxels();
        }
        else
        {
            SpawnUpperVoxelLayer(gmb);
        }
    }

    void SpawnUpperVoxelLayer(GameObject gmb)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z= 0; z < gridSize; z++)
            {
                int worldX = Mathf.FloorToInt(gmb.transform.position.x  + x);
                int worldZ = Mathf.FloorToInt(gmb.transform.position.z  + z);
                
                float noise =  Mathf.PerlinNoise(worldX * 0.1f, worldZ * 0.1f);
                float fin = Mathf.FloorToInt(noise * 10);
            
                for (int y = 0; y < gridSize; y++)
                {
                    int index = GetIndex(Vector3Int.FloorToInt(new Vector3(x,y,z)),gridSize);
                    Voxel voxel;
                    if (fin > 0)
                    {
                        fin--;
                        voxel.integrity = 100;
                        voxel.ground = 1;
                    }
                    else
                    {
                        voxel.integrity = 0;
                        voxel.ground = 0;
                       
                    }
                    
                    voxels[index] = voxel;
                }
            }
        }
    }
    

    public void DamageVoxel(Vector3Int pos,float damage)
    {
        int index = GetIndex(pos, gridSize);
        voxels[index].integrity -= damage;
        if (voxels[index].integrity <= 80)
        {
            voxels[index].ground = 0;
        }
    }

    public bool IsAir(int index)
    {
        if (voxels[index].ground == 0)
        {
            return true;
        }
        return false;
    }
    
    public struct Voxel
    {
        public byte ground;
        public float integrity;
    }
    
    void SpawnVoxels()
    {
        Vector3 pos = new Vector3(0,0,0);
    
        for (int k =0; k < gridSize  ; k++)
        {
            iter = k;
            for (int i = 0; i < gridSize ; i++)
            {
          
                for (int j = 0; j < gridSize ; j++)
                {
                    //   GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // cube.transform.SetParent(hierarchyParent.transform);
                    //  cube.transform.localScale = scale;
                    //   cube.transform.position = pos;
                    int index = GetIndex(Vector3Int.FloorToInt(pos),gridSize);
                    Voxel voxel;
                    voxel.integrity = 100;
                    voxel.ground = 1;
                    voxels[index] = voxel;
                    pos.x += scale;
                }
                pos.z += scale;
                pos.x = 0;
            }

            pos.y += scale;
            pos.z = 0;
        }
        
    } 
    
    public static bool IsOutsideChunk(Vector3Int pos,int gridSize)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;
        if ((x < 0 || x > gridSize - 1) ||  (y < 0 || y > gridSize - 1) || (z < 0 || z > gridSize - 1))
        {
            return true;
        }

        return false;
    }
    
   
    
    public int GetIndex(Vector3Int pos,int size)
    {
        if (pos.x < 0 || pos.x >= size ||
            pos.y < 0 || pos.y >= size ||
            pos.z < 0 || pos.z >= size)
        {
            Debug.LogWarning("Position is outside current voxel grid");
            return -1;
        }
           
        
        return (pos.x + pos.y * size + pos.z * size * size);
    }

   public Vector3Int FromIndex(int i, int size)
    {
        int x = i % size;
        int y = (i / size) % size;
        int z = i / (size * size);
        return new Vector3Int(x, y, z);
    }
    
    public bool _IsSolid(int index)
    {
    
        if (!IsOutsideChunk(FromIndex(index,gridSize),gridSize)) return true;
        
        if (voxels[index].ground == 1)
        {
            return true;
        }
        return false;
    }
    
    public  bool _IsSolid(Vector3Int pos)
    {
        if (IsOutsideChunk(pos,gridSize)) return false;
      
        int index = GetIndex(pos, gridSize);
        if (voxels[index].ground == 1)
        {
            return true;
        }
        return false;
    }
    
   static  Vector3Int  GetChunkLocalCoord(Vector3 pos)
    {
        Vector3Int position = new Vector3Int(Mathf.FloorToInt(pos.x / ChunkManager.ChunkSize),Mathf.FloorToInt(pos.y /  ChunkManager.ChunkSize) ,Mathf.FloorToInt(pos.z /  ChunkManager.ChunkSize));
       
        return position;
    }

    public class VoxelUtils
    {
        private static VoxelChunk chunk;

        public VoxelUtils(VoxelChunk currentChunk)
        {
            chunk = currentChunk;
        }
        
        public static bool IsSolid(int index)
        {
           return chunk._IsSolid(index);
        }
        
        public  static bool IsSolid(Vector3Int pos)
        {
            return chunk._IsSolid(pos);
        }
        
       public static Vector3Int GetLocalPos(Vector3Int posToConvert,Vector3 currentTransformPos)
        {
            
            Vector3Int voxelPos = posToConvert;
           
                voxelPos.x -= Mathf.FloorToInt(currentTransformPos.x) ;
            

            
                voxelPos.y -= Mathf.FloorToInt(currentTransformPos.y) ;
            

                voxelPos.z -= Mathf.FloorToInt(currentTransformPos.z) ;
            
        
            return voxelPos;
        }
       
       public static Vector3Int GetChunkLocalCoord(Vector3 pos)
        {
            Vector3Int position = new Vector3Int(Mathf.FloorToInt(pos.x /  ChunkManager.ChunkSize),Mathf.FloorToInt(pos.y /  ChunkManager.ChunkSize) ,Mathf.FloorToInt(pos.z /  ChunkManager.ChunkSize));
       
            return position;
        }
        
        public static bool Ray(Vector3 startPos,Vector3 dir,int len, ref Vector3Int hitPos,ref Vector3 hitPosFloor,[CanBeNull] Dictionary<Vector3Int,VoxelChunk> chunks)
        {
       
     
            Vector3 endPoint = startPos + dir * 0.3f ;
            Vector3 rayDir = endPoint - startPos;
        
   
            int dist = 0;
        
            for (int i = 0; i < len; i++)
            {
                Vector3 newPoint = startPos + rayDir * dist;
                
                if (!chunks.TryGetValue(GetChunkLocalCoord(newPoint), out VoxelChunk chunker))
                {
                    dist++;
                    continue;
                }

                return true;
               
           
            
              //  Vector3Int localPos = GetLocalPos(Vector3Int.FloorToInt(newPoint),new Vector3(chunkTransformPos.x,chunkTransformPos.y,chunkTransformPos.z));
                int index  = chunker.GetIndex(new Vector3Int(),chunk.gridSize);
                if (index == -1 || chunk.IsAir(index))
                {
                    continue;
                }
                hitPos =  chunk.FromIndex(index,chunk.gridSize);
                hitPosFloor = newPoint;
                return true;

            }
            return false;
        }
    }
}
