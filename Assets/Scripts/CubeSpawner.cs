using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class CubeSpawner : MonoBehaviour
{
    public Vector3 startPoint;

    public Vector3 scale;

    public int gridSize;

    public float height;

    public GameObject hierarchyParent;

  

    public Material fullHPMat;

    public Material lowHpMat;

    public List<Vector3> verticies;
    public List<int> triangles;
    public List<Vector3> normals;
    
    Dictionary<int,Vector3Int[]> faceDirections;
    private Vector3[] normalDirections;
    private GameObject sphereDebug;

    private bool isBusy;
    Dictionary<Vector2Int,VoxelChunk> chunks = new Dictionary<Vector2Int,VoxelChunk>();

    struct Offset3
    {
       public sbyte xP, yP, zP;

       public Offset3 Create(sbyte x, sbyte y, sbyte z)
       {
          Offset3 newOffset = new Offset3{xP = x, yP = y, zP = z};
           return newOffset;
       }
    }
    
    private Offset3[] offsets;

    private VoxelChunk chunk;

   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
   
        
        
      //  chunk = new VoxelChunk(9,gameObject);
      print(VoxelChunk.IsOutsideChunk(new Vector3Int(0,1,8), 9)); 
        InitializeOffsets();
        InitializeFaceDirections();
        InitializeNormals();
        CombineMasks();

        StartCoroutine(RelaxBro());






    }

   public void InstDict(Dictionary<Vector2Int, VoxelChunk> dict)
   {
       chunks = dict;
   }

    IEnumerator RelaxBro()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i <chunk.voxels.Length; i++)
        {
            print(chunk.FromIndex(i,9));
           
        }
    }

    void InitializeNormals()
    {
        normalDirections = new Vector3[]
        {
            Vector3.down,
            Vector3.up,
            Vector3.left,
            Vector3.right,
            Vector3.back,
            Vector3.forward
        };
    }
    
  
    void InitializeFaceDirections()
    {
        faceDirections = new Dictionary<int,Vector3Int[]>();
        // 0: DOWN (-Y)
        faceDirections.Add(0, new Vector3Int[]
        {
           
            new Vector3Int(0, 0, 0), 
            new Vector3Int(1, 0, 0), 
            new Vector3Int(1, 0, 1), 
            new Vector3Int(0, 0, 1), 
        });

// 1: UP (+Y)
        faceDirections.Add(1, new Vector3Int[]
        {
            new Vector3Int(0, 1, 1),
            new Vector3Int(1, 1, 1),
            new Vector3Int(1, 1, 0),
            new Vector3Int(0, 1, 0),
        });

// 2: LEFT (-X)
        faceDirections.Add(2, new Vector3Int[]
        {
            new Vector3Int(0, 1, 1),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 1),
           
        });

// 3: RIGHT (+X)
        faceDirections.Add(3, new Vector3Int[]
        {
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 1, 1),
        });

// 4: BACK (-Z)
        faceDirections.Add(4, new Vector3Int[]
        {
            new Vector3Int(1, 1, 1),
            new Vector3Int(0, 1, 1),
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 1),
        });

// 5: FRONT (+Z)
        faceDirections.Add(5, new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 0, 0),
        });
    }

    void InitializeOffsets()
    {
        Offset3 initial = new Offset3();
        Offset3 offset1;
        Offset3 offset2;
        Offset3 offset3;
        Offset3 offset4;
        Offset3 offset5;
        Offset3 offset6;
        offset1 =  initial.Create(-1, 0, 0);
        offset2 =  initial.Create(1, 0, 0);
        offset3 =  initial.Create(0, -1, 0);
        offset4 =  initial.Create(0, 1, 0);
        offset5 =  initial.Create(0, 0, -1);
        offset6 =  initial.Create(0, 0, 1);

        offsets = new[] { offset3, offset4, offset1, offset2, offset6, offset5 };
    }

    // Update is called once per frame
    void Update()
    {
     
        RayShooter();
  
    }

    private void FixedUpdate()
    {
   
    }

   

    

   


   

    void RayShooter()
    {
 
        if(Input.GetKeyDown(KeyCode.Mouse0) && !isBusy)
        {
            print("Shooting ray");
           isBusy = true;
            Camera cam = Camera.main;

            VoxelChunk hitChunk = null;
            Vector3Int hitPos = new Vector3Int(int.MinValue,int.MinValue,int.MinValue);
            Vector3 hitPosFloat = new Vector3(0, 0, 0);
      bool hit  =  RayDebug(cam.transform.position, cam.transform.forward,30,ref hitPos,ref hitPosFloat,chunks,ref hitChunk);
            print(hit);
            
            if (hit)
            {
                HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
                
                int iteration = 0;
                CustomSearch(70f, hitPos,visited,ref iteration,hitChunk);
                print(iteration);
                CombineMasks();
                visited.Clear();
             
            }
               
            isBusy = false;
        }
      
    }
    
    public bool RayDebug(Vector3 startPos,Vector3 dir,int len, ref Vector3Int hitPos,ref Vector3 hitPosFloor, Dictionary<Vector2Int,VoxelChunk> chunks,ref VoxelChunk hitChunk)
    {
       
     
        Vector3 endPoint = startPos + dir * 0.3f ;
        Vector3 rayDir = endPoint - startPos;
        
   
        int dist = 0;
        
        for (int i = 0; i < len; i++)
        {
             Vector3 newPoint = startPos + rayDir * dist;
            // GameObject gmb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // gmb.transform.position = newPoint;
            Vector2Int localCord = GetChunkLocalCoord(newPoint);
            print(localCord);
            if (!chunks.TryGetValue(localCord, out VoxelChunk chunker))
            {
                dist++;
                print("Didnt find anythuing");
                continue;
            }


            hitChunk = chunker;
            print("Found chunk at" + localCord);
            
              Vector3Int localPos = GetLocalPos(Vector3Int.FloorToInt(newPoint),new Vector3(chunker.connectedGmb.transform.position.x,chunker.connectedGmb.transform.position.y,chunker.connectedGmb.transform.position.z));
            int index  = chunker.GetIndex(localPos,9);
            if (index == -1 || hitChunk.IsAir(index))
            {
                dist++;
                continue;
            }
            hitPos =  hitChunk.FromIndex(index,9);
            hitPosFloor = newPoint;
            return true;

        }
        return false;
    }
    
    Vector2Int GetChunkLocalCoord(Vector3 pos)
    {
        Vector2Int position = new Vector2Int(Mathf.FloorToInt(pos.x / 9), Mathf.FloorToInt(pos.z / 9));
    
        return position;
    }
    
    public static Vector3Int GetLocalPos(Vector3Int posToConvert,Vector3 currentTransformPos)
    {
        Vector3Int voxelPos = posToConvert;
        if (voxelPos.x > (9 ) - 1)
        {
            voxelPos.x -= Mathf.FloorToInt(currentTransformPos.x) ;
        }

        if (voxelPos.y > (9) - 1)
        {
            voxelPos.y -= Mathf.FloorToInt(currentTransformPos.y) ;
        }

        if (voxelPos.z > (9) - 1)
        {
            voxelPos.z -= Mathf.FloorToInt(currentTransformPos.z) ;
        }
        
        return voxelPos;
    }

 
    void CustomSearch(float damage, Vector3Int pos,HashSet<Vector3Int> visited,ref int iter,VoxelChunk hitChunk)
    {
        
        iter++;
        print(pos);
        bool isOutside = VoxelChunk.IsOutsideChunk(pos, 9);
        if (isOutside)
        {
            print("Outside chunk");
            return;
        }

        if (visited.Contains(pos))
        {
            // print("Visited");
            return;
        }
        print("Damage pos" + pos);
        hitChunk.DamageVoxel(pos,damage);
        float newDamage = damage / 1.7f;
      
        visited.Add(pos);
        // foreach (Offset3 offset in offsets) //6
        // {
        //     
        //     CustomSearch(newDamage, new Vector3Int(pos.x + offset.xP, pos.y + offset.yP, pos.z + offset.zP),visited,ref iter);
        // }
        
    }

  

   

  

    
    
    

    List<Rectangle2D> BuildGreedyQuads(int[,] mask)
    {
        int sizeX = mask.GetLength(0);
        int sizeY = mask.GetLength(1);
        
        List<Rectangle2D> rects = new List<Rectangle2D>();
        
        for (int y = 0; y < sizeY; y++)
        {
            for (int x  = 0; x < sizeX; x++)
            {
                if (mask[y, x] == 0)
                    continue;
                
                int width = 1;
                int dir = mask[y, x];
                while (x + width < sizeX && mask[y, x + width] == dir)
                {
                    width++;
                }

                int height = 1;
                bool canExpand = true;
                while (y + height < sizeY && canExpand)
                {
                    for (int dx = 0; dx < width; dx++)
                    {
                        if (mask[y + height, x + dx] != dir)
                        {
                            canExpand = false;
                            break;
                        }
                    }

                    if (canExpand)
                    {
                        height++;
                    }
                }

             
                if (dir == 1)
                {
                   
                }
                rects.Add(new Rectangle2D { x = x, y = y, w = width, h = height,dir = dir });

                for (int dy = 0; dy < height; dy++)
                {
                    for (int dx = 0; dx < width; dx++)
                    {
                        mask[y + dy,x + dx] = 0;
                    }
                }
            }
          
        }
        return rects;
    }

    void CombineMasks()
    {
        verticies.Clear();
        triangles.Clear();
        Mesh meshy = new Mesh();
       BuildMaskXY(meshy);
       BuildMaskZY(meshy);
        BuildMaskXZ(meshy);
    }
    
    //Along Y
    void BuildMaskXZ(Mesh meshy)
    {
        int[,] mask = new int[gridSize, gridSize];
        
        for (int slice = -1; slice < gridSize; slice++)
        {
            for (int x = 0;x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector3Int currentUvw =  new Vector3Int(x,slice,y);
                    Vector3Int neighbourUvw = new Vector3Int(x,slice + 1,y);

                    bool a = VoxelChunk.VoxelUtils.IsSolid(currentUvw);
                    bool b = VoxelChunk.VoxelUtils.IsSolid(neighbourUvw);
                    int index = 0;
                    if (a && !b)
                    {
                        index = 1;
                    }

                    if (!a && b)
                    {
                        index = -1;
                    }
                    
                    mask[x,y] = index;
                
                }
                
            }
            
            List<Rectangle2D> rects = BuildGreedyQuads(mask);
            RenderQuadsOneAxis(rects,slice + 1,meshy,1);
        }
    }
    
    //Along X
    void BuildMaskZY(Mesh meshy)
    {
        int[,] mask = new int[gridSize, gridSize];
        
        for (int slice = -1; slice < gridSize; slice++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector3Int currentUvw =  new Vector3Int(slice,z,y);
                    Vector3Int neighbourUvw = new Vector3Int(slice + 1,z,y);
                    
                    bool a =  VoxelChunk.VoxelUtils.IsSolid(currentUvw);
                    bool b =  VoxelChunk.VoxelUtils.IsSolid(neighbourUvw);
                    int index = 0;
                    if (a && !b)
                    {
                        index = 1;
                    }

                    if (!a && b)
                    {
                        index = -1;
                    }
                    
                    mask[y,z] = index;
                
                }
                
            }
            
            List<Rectangle2D> rects = BuildGreedyQuads(mask);
            RenderQuadsOneAxis(rects,slice + 1,meshy,0);
        }
    }
    
    //Along Z
    void BuildMaskXY(Mesh meshy)
    {
      
        int[,] mask = new int[gridSize, gridSize];
        
        for (int slice = -1; slice < gridSize; slice++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Vector3Int currentUvw =  new Vector3Int(x, y, slice);
                    Vector3Int neighbourUvw = new Vector3Int(x, y, slice + 1);
                    
                    bool a = VoxelChunk.VoxelUtils.IsSolid(currentUvw);
                    bool b = VoxelChunk.VoxelUtils.IsSolid(neighbourUvw);
                    int index = 0;
                    if (a && !b)
                    {
                        index = 1;
                    }

                    if (!a && b)
                    {
                        index = -1;
                    }
                    
                    mask[y,x] = index;
                
                }
                
            }
            
            List<Rectangle2D> rects = BuildGreedyQuads(mask);
            RenderQuadsOneAxis(rects,slice + 1,meshy,2);
        }
    }

    void RenderQuadsOneAxis(List<Rectangle2D> rects,int slice,Mesh mesh,int axis = 10)
    {
       
        
        int iter = 0;
        foreach (var rect in rects)
        {
            int start = verticies.Count;
            
            int[] p = new  int[3];
            p[axis] = slice;
        
            

            int u = (axis + 1) % 3;
            int v = (axis + 2) % 3;
            
            p[u] = rect.x;
            p[v] = rect.y;

            int[] du = new int[3];
            int[] dv = new int[3];

            du[u] = rect.w;
            dv[v] = rect.h;
            
            
            // Vector3 leftBottom = new Vector3(rect.x,rect.y,slice);
            // Vector3 rightBottom= new Vector3(rect.x + rect.w,rect.y,slice);
            // Vector3 rightTop= new Vector3(rect.x + rect.w,rect.y + rect.h,slice);
            // Vector3 leftTop= new Vector3(rect.x,rect.y + rect.h,slice);
            
            Vector3 leftBottom = new Vector3(p[0], p[1], p[2]);
            Vector3 rightBottom =  new Vector3(p[0] + du[0], p[1] + du[1], p[2] + du[2]);
            Vector3 rightTop = new Vector3(p[0] + du[0] + dv[0], p[1] + du[1]  + dv[1], p[2] + du[2]  + dv[2]);
            Vector3 leftTop =  new Vector3(p[0] + dv[0], p[1] + dv[1], p[2] + dv[2]);
            
            
            iter++;
            verticies.Add(leftBottom);
            verticies.Add(rightBottom);
            verticies.Add(rightTop);
            verticies.Add(leftTop);
            
            
            if (rect.dir > 0)
            {
                
                triangles.Add(start + 0);
                triangles.Add(start + 1);
                triangles.Add(start + 2);
            
                triangles.Add(start + 0);
                triangles.Add(start + 2);
                triangles.Add(start + 3);
            }
            else
            {
                triangles.Add(start + 0);
                triangles.Add(start + 2);
                triangles.Add(start + 1);
            
                triangles.Add(start + 0);
                triangles.Add(start + 3);
                triangles.Add(start + 2);
            }
        }
        
//        print("ITERATIONS" +  iter);
        
        mesh.SetVertices(verticies);
        mesh.SetTriangles(triangles, 0);
        
        mesh.RecalculateBounds();
        
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    
   
    
    
    struct Rectangle2D
    {
        public int x;
        public int y;
        public int w;
        public int h;
        public int dir;
    }



 
    
    




    #region NAIVE_MESHING
    
    
    
    // void RebuildMesh()
    // {
    //     verticies.Clear();
    //     normals.Clear();
    //     triangles.Clear();
    //     for (int x = 0; x < gridSize; x++)
    //     {
    //         for (int y = 0; y < gridSize; y++)
    //         {
    //             for (int z = 0; z < gridSize ; z++)
    //             {
    //                 Vector3Int voxelPos =  new Vector3Int(x, y, z);
    //                
    //                 if (IsSolid(voxelPos) == false)
    //                 {
    //                     continue;
    //                 }
    //                
    //                 for (int i = 0; i < offsets.Length; i++)
    //                 {
    //                
    //                     bool outsideChunk = IsOutsideChunk(new Vector3Int(voxelPos.x + offsets[i].xP,
    //                         voxelPos.y + offsets[i].yP, voxelPos.z + offsets[i].zP));
    //                     if (outsideChunk)
    //                     {
    //                        AddFaces(voxelPos,i);
    //                         continue;
    //                     }
    //                   
    //                     int  index = GetIndex(new Vector3Int(voxelPos.x + offsets[i].xP,voxelPos.y + offsets[i].yP,voxelPos.z + offsets[i].zP),gridSize);
    //                     
    //                     bool drawFace = IsSolid(index) ;
    //                     if (drawFace)
    //                     {
    //                         continue;
    //                     }
    //                     AddFaces(voxelPos,i);
    //                 }
    //             }
    //         }
    //     }
    //     
    //     Mesh mesh = new Mesh();
    //    mesh.SetVertices(verticies);
    //    mesh.SetTriangles(triangles, 0);
    //    mesh.SetNormals(normals);
    //    mesh.RecalculateBounds();
    //    GetComponent<MeshFilter>().mesh = mesh;
    //    GetComponent<MeshCollider>().sharedMesh = null;
    //    GetComponent<MeshCollider>().sharedMesh = mesh;
    // }
    
    
    
    void AddFaces(Vector3Int pos,int dir)
    {
        Vector3Int voxPos = pos;
        int start = verticies.Count;
        
        foreach (Vector3Int direction in faceDirections[dir])
        {
            verticies.Add(voxPos + direction);
            normals.Add(normalDirections[dir]);
        }
        
      
        triangles.Add(start + 0);
        triangles.Add(start + 1);
        triangles.Add(start + 2);
        
        triangles.Add(start + 0);
        triangles.Add(start + 2);
        triangles.Add(start + 3);
        
    }
    

    #endregion
    

  

 


  

    

  
}
