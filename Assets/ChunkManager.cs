using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
   public Dictionary<Vector3Int, VoxelChunk> chunks = new Dictionary<Vector3Int, VoxelChunk>();

   public int chunkCount;
   public int chunkSize;
   public static int ChunkSize;
   public ChunkRenderer renderer;

   public RayCaster caster;
   public Material material;

   public struct Offset3
   {
      public sbyte xP, yP, zP;

      public Offset3 Create(sbyte x, sbyte y, sbyte z)
      {
         Offset3 newOffset = new Offset3 { xP = x, yP = y, zP = z };
         return newOffset;
      }
   }

   private Offset3[] offsets;

   void InitializeOffsets()
   {
      Offset3 initial = new Offset3();
      Offset3 offset0;
      Offset3 offset1;
      Offset3 offset2;
      Offset3 offset3;
      Offset3 offset4;
      Offset3 offset5;
      Offset3 offset6;
      Offset3 offset7;
      Offset3 offset8;


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

   private void Start()
   {
      ChunkSize = chunkSize;
      InitializeOffsets();
      SpawnInitChunks();

      foreach (var VARIABLE in chunks)
      {
         Mesh meshy = renderer.GetMesh(VARIABLE.Value);
         MeshFilter filter = VARIABLE.Value.connectedGmb.GetComponent<MeshFilter>();
         filter.mesh = meshy;
        VARIABLE.Value.connectedGmb.GetComponent<MeshRenderer>().material = material;
      }
     
   }
   


   void SpawnInitChunks()
   {

      for (int x = 0; x < chunkCount; x++)
      {
         for (int z = 0; z < chunkCount; z++)
         {
            GameObject gmb = new GameObject("Chunk");
            gmb.transform.position = new Vector3(x * chunkSize, chunkSize, z* chunkSize);
            gmb.transform.localScale = new Vector3(1, 1, 1);

            Vector3Int crd =VoxelChunk.VoxelUtils.GetChunkLocalCoord(gmb.transform.position);
            print(crd);
            chunks.Add(crd, new VoxelChunk(chunkSize, gmb,VoxelChunk.ChunkType.Ground));

            gmb.AddComponent<MeshFilter>();
            gmb.AddComponent<MeshRenderer>();
         }
      }
      
      
      for (int x = 0; x < chunkCount; x++)
      {
         for (int z = 0; z < chunkCount; z++)
         {
            GameObject gmb = new GameObject("Chunk");
            gmb.transform.position = new Vector3(x * chunkSize, 0, z* chunkSize);
            gmb.transform.localScale = new Vector3(1, 1, 1);

            Vector3Int crd =VoxelChunk.VoxelUtils.GetChunkLocalCoord(gmb.transform.position);
            print(crd);
            chunks.Add(crd, new VoxelChunk(chunkSize, gmb,VoxelChunk.ChunkType.Underground));

            gmb.AddComponent<MeshFilter>();
            gmb.AddComponent<MeshRenderer>();
         }
      }
      
      
      

   }

   private void Update()
   {
      caster.RayShooter(chunks);
   
   }

   private void FixedUpdate()
   {
   
   }

   public  void NotifyChanged(VoxelChunk chunk)
   {
      RenderChunks(chunk);
   }
  

   public void RenderChunks(VoxelChunk chunk)
   {
     
         Mesh meshy = renderer.GetMesh(chunk);
         MeshFilter filter = chunk.connectedGmb.GetComponent<MeshFilter>();
         filter.mesh = meshy;
        
      
   }
}
