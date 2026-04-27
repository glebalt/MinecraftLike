using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCaster : MonoBehaviour
{
    public bool isBusy = false;
    public VoxelDebug voxelDebug;
    private Camera cam;
    private GameObject rayCubeDebug;
    public Vector3 dirOffset;
    public Material chunkDebug;
    public ChunkManager chunkManager;
    private List<GameObject> debugCubes;
    
    private ChunkManager.Offset3[] offsets;
    private void Start()
    {
        InitializeOffsets();
        debugCubes = new  List<GameObject>();
       cam = Camera.main;
    }
    
    void InitializeOffsets()
    {
        ChunkManager.Offset3 initial = new ChunkManager.Offset3();
        ChunkManager.Offset3 offset0;
        ChunkManager.Offset3 offset1;
        ChunkManager.Offset3 offset2;
        ChunkManager.Offset3 offset3;
        ChunkManager.Offset3 offset4;
        ChunkManager.Offset3 offset5;
        ChunkManager.Offset3 offset6;
        ChunkManager.Offset3 offset7;
        ChunkManager.Offset3 offset8;


        offset0 = initial.Create(0, 0, 0);
        offset1 = initial.Create(-1, 0, 0);
        offset2 = initial.Create(1, 0, 0);

        offset3 = initial.Create(0, 0, 1);
        offset4 = initial.Create(-1, 0, 1);
        offset5 = initial.Create(1, 0, 1);

        offset6 = initial.Create(0, 0, -1);
        offset7 = initial.Create(-1, 0, -1);
        offset8 = initial.Create(1, 0, -1);

        offsets = new[] { offset0, offset3, offset4, offset1, offset2, offset6, offset5, offset7, offset8 };
    }

    public void RayShooter(Dictionary<Vector3Int,VoxelChunk> chunks)
    {
            
      
           // print("Shooting ray");
       
        
            
            VoxelChunk hitChunk = null;
            Vector3Int hitPos = new Vector3Int(int.MinValue,int.MinValue,int.MinValue);
            Vector3 hitPosFloat = new Vector3(0, 0, 0);
        
      
       bool hit = DDA(cam.transform.position,cam.transform.forward,50,chunks,ref hitPos,ref hitChunk);


           chunkManager.NotifyChanged(hitChunk);
            if (hit)
            {
                
                if (Input.GetKeyDown(KeyCode.Mouse0) && !isBusy)
                {
                    isBusy = true;
                    HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

                    int iteration = 0;
                    CustomSearch(70f, hitPos, visited, ref iteration, hitChunk);
                //    print(iteration);
                    visited.Clear();
                }
            }
               
            isBusy = false;
        
      
    }
    
    public bool RayDebug(Vector3 startPos,Vector3 dir,int len, ref Vector3Int hitPos,ref Vector3 hitPosFloor, Dictionary<Vector3Int,VoxelChunk> chunks,ref VoxelChunk hitChunk)
    {
       
     //  Destroy(rayCubeDebug);
        Vector3 endPoint = startPos + dir ;
        Vector3 rayDir = endPoint - startPos;
    //    print("RAYDIR" + rayDir);
       
        int dist = 0;
        
        for (int i = 0; i < len; i++)
        {
           
            Vector3 newPoint = startPos + (rayDir) * dist;
         
            
            Vector3Int localCord =VoxelChunk.VoxelUtils.GetChunkLocalCoord(newPoint);
      
          
         //   print(localCord);
            if (!chunks.TryGetValue(localCord, out VoxelChunk chunker))
            {
               
                dist++;
          //      print("Didnt find anythuing");
                continue;
            }


            hitChunk = chunker;


           
            
            Vector3Int localPos =VoxelChunk.VoxelUtils.GetLocalPos(Vector3Int.FloorToInt(newPoint),new Vector3(chunker.connectedGmb.transform.position.x,chunker.connectedGmb.transform.position.y,chunker.connectedGmb.transform.position.z));
          //  print("local position of voxel = " + localPos);
            int index  = chunker.GetIndex(localPos,chunkManager.chunkSize);
            if (index == -1 || hitChunk.IsAir(index))
            {
          //      Debug.LogWarning("Doesnt hit!!!");
                dist++;
                continue;
            }
            
            hitPos =  hitChunk.FromIndex(index,chunkManager.chunkSize);
            hitPosFloor = (newPoint - rayDir * 0.01f);
          //  rayCubeDebug = GameObject.CreatePrimitive(PrimitiveType.Cube);
           // rayCubeDebug.transform.position = newPoint;
            return true;

        }
        
        
        return false;
    }
    
   
  

    bool DDA(Vector3 pos,Vector3 dir,int len,Dictionary<Vector3Int,VoxelChunk> chunks,ref Vector3Int hit,ref VoxelChunk hitChunk)
    {

        foreach (GameObject gmb in debugCubes)
        {
            Destroy(gmb);
        }
        debugCubes.Clear();
        
        Vector3 normal = new Vector3();
        Vector3Int voxelStartPos = Vector3Int.FloorToInt(pos);
        
        Vector3Int step= new Vector3Int(dir.x > 0 ? 1 : -1,dir.y > 0 ? 1 : -1,dir.z > 0 ? 1 : -1);

        Vector3Int border = new Vector3Int(step.x > 0 ? voxelStartPos.x + 1 : voxelStartPos.x,
            step.y > 0 ? voxelStartPos.y + 1 : voxelStartPos.y, step.z > 0 ? voxelStartPos.z + 1 : voxelStartPos.z);
        
        Vector3 tMax = new Vector3((border.x - pos.x) / dir.x, (border.y - pos.y)/  dir.y, (border.z - pos.z)/dir.z);
       
        
        Vector3 tDelta = new Vector3(
            Mathf.Abs(1f / dir.x),
            Mathf.Abs(1f / dir.y),
            Mathf.Abs(1f / dir.z)
          );
     
        for (int i = 0; i < 30; i++)
        {
            Vector3Int localCord =VoxelChunk.VoxelUtils.GetChunkLocalCoord(voxelStartPos);
        
            if (!chunks.TryGetValue(localCord, out VoxelChunk chunker))
            {

                if (tMax.x < tMax.y)
                {
                    if (tMax.x < tMax.z)
                    {
                        voxelStartPos.x += step.x;
                        tMax.x += tDelta.x;
                        normal = new Vector3(-step.x,0,0);
                    }
                    else
                    {
                        voxelStartPos.z += step.z;
                        tMax.z += tDelta.z;
                        normal = new Vector3(0, 0, -step.z);
                    }
                }
                else
                {
                    if (tMax.y < tMax.z)
                    {
                        voxelStartPos.y += step.y;
                        tMax.y +=  tDelta.y;
                        normal = new Vector3(0, -step.y, 0);
                    }
                    else
                    {
                        voxelStartPos.z += step.z;
                        tMax.z +=  tDelta.z;
                        normal = new Vector3(0, 0, -step.z);
                    }
                }
                
                
           //     gmbCube.transform.position = voxelStartPos;
            //    debugCubes.Add(gmbCube);
               
            }
            else
            {
                Vector3Int localPos =VoxelChunk.VoxelUtils.GetLocalPos(voxelStartPos,new Vector3(chunker.connectedGmb.transform.position.x,chunker.connectedGmb.transform.position.y,chunker.connectedGmb.transform.position.z));
                int index  = chunker.GetIndex(localPos,chunkManager.chunkSize);
                
                if (index == -1 || chunker.IsAir(index))
                {
                    
                    if (tMax.x < tMax.y)
                    {
                        if (tMax.x < tMax.z)
                        {
                            voxelStartPos.x += step.x;
                            tMax.x += tDelta.x;
                            normal = new Vector3(-step.x,0,0);
                        }
                        else
                        {
                            voxelStartPos.z += step.z;
                            tMax.z += tDelta.z;
                            normal = new Vector3(0, 0, -step.z);
                        }
                    }
                    else
                    {
                        if (tMax.y < tMax.z)
                        {
                            voxelStartPos.y += step.y;
                            tMax.y +=  tDelta.y;
                            normal = new Vector3(0, -step.y, 0);
                        }
                        else
                        {
                            voxelStartPos.z += step.z;
                            tMax.z +=  tDelta.z;
                            normal = new Vector3(0, 0, -step.z);
                        }
                    }
                }
                else
                {
                    Vector3Int hitPos =  chunker.FromIndex(index,chunkManager.chunkSize);
                    hit = hitPos;
                    hitChunk = chunker;
                    voxelDebug.HighlightVoxel(hitPos,chunker);
                   // chunker.connectedGmb.GetComponent<MeshRenderer>().material = chunkDebug;
                    return true;
                }

               
            }
        }
        
        
        return false;
    }

    public void SimpleRay()
    {
        
    }
    
    void CustomSearch(float damage, Vector3Int pos,HashSet<Vector3Int> visited,ref int iter,VoxelChunk hitChunk)
    {
        
        iter++;
     //   print(pos);
        bool isOutside = VoxelChunk.IsOutsideChunk(pos, chunkManager.chunkSize);
        if (isOutside)
        {
      //      print("Outside chunk");
            return;
        }

        if (visited.Contains(pos))
        {
            // print("Visited");
            return;
        }
     //   print("Damage pos" + pos);
        hitChunk.DamageVoxel(pos,damage);
        float newDamage = damage / 1.7f;
      
        visited.Add(pos);
        // foreach (ChunkManager.Offset3 offset in offsets) //6
        // {
        //     
        //     CustomSearch(newDamage, new Vector3Int(pos.x + offset.xP, pos.y + offset.yP, pos.z + offset.zP),visited,ref iter,hitChunk);
        // }
        
    }
}
