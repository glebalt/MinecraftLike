using System.Collections.Generic;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    public List<Vector3> verticies;
    public List<int> triangles;
    public List<Vector3> normals;
        public List<Vector2> uvs = new List<Vector2>();
    public int gridSize;
    
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

    public Mesh GetMesh(VoxelChunk chunk)
    {
      
        verticies.Clear();
        triangles.Clear();
        uvs.Clear();
        Mesh meshy = new Mesh();
       BuildMaskXY(meshy,chunk);
       BuildMaskZY(meshy, chunk);
        BuildMaskXZ(meshy,chunk);

        return meshy;
    }

   

    
    //Along Y
    void BuildMaskXZ(Mesh meshy,VoxelChunk chunk)
    {
        int[,] mask = new int[ChunkManager.ChunkSize, ChunkManager.ChunkSize];
        
        for (int slice = -1; slice < ChunkManager.ChunkSize; slice++)
        {
            for (int x = 0;x < ChunkManager.ChunkSize; x++)
            {
                for (int y = 0; y < ChunkManager.ChunkSize; y++)
                {
                    Vector3Int currentUvw =  new Vector3Int(x,slice,y);
                    Vector3Int neighbourUvw = new Vector3Int(x,slice + 1,y);

                    bool a = chunk._IsSolid(currentUvw);
                    bool b = chunk._IsSolid(neighbourUvw);
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
    void BuildMaskZY(Mesh meshy,VoxelChunk chunk)
    {
        int[,] mask = new int[ChunkManager.ChunkSize, ChunkManager.ChunkSize];
        
        for (int slice = -1; slice < ChunkManager.ChunkSize; slice++)
        {
            for (int z = 0; z < ChunkManager.ChunkSize; z++)
            {
                for (int y = 0; y < ChunkManager.ChunkSize; y++)
                {
                    Vector3Int currentUvw =  new Vector3Int(slice,z,y);
                    Vector3Int neighbourUvw = new Vector3Int(slice + 1,z,y);
                    
                    bool a = chunk._IsSolid(currentUvw);
                    bool b = chunk._IsSolid(neighbourUvw);
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
    void BuildMaskXY(Mesh meshy,VoxelChunk chunk)
    {
      
        int[,] mask = new int[ChunkManager.ChunkSize, ChunkManager.ChunkSize];
        
        for (int slice = -1; slice < ChunkManager.ChunkSize; slice++)
        {
            for (int x = 0; x < ChunkManager.ChunkSize; x++)
            {
                for (int y = 0; y < ChunkManager.ChunkSize; y++)
                {
                    Vector3Int currentUvw =  new Vector3Int(x, y, slice);
                    Vector3Int neighbourUvw = new Vector3Int(x, y, slice + 1);
                    
                    bool a = chunk._IsSolid(currentUvw);
                    bool b = chunk._IsSolid(neighbourUvw);
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
            
            float step = 0.25f;

            float uMin = 1 * step; 
            float vMin = 1 * step; 

            float uMax = uMin + step  * rect.w; 
            float vMax = vMin + step * rect.h;
            if (rect.w == 3 && rect.h == 3)
            {
                print(uMax + " " + vMax);
            }
            
            uvs.Add(new Vector2(uMin,vMin));
            uvs.Add(new Vector2(uMax,vMin));
            uvs.Add(new Vector2(uMax,vMax));
            uvs.Add(new Vector2(uMin,vMax));
            
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
        mesh.SetUVs(0, uvs);
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }
    
   
    
    
    struct Rectangle2D
    {
        public int x;
        public int y;
        public int w;
        public int h;
        public int dir;
    }
}
