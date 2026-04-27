using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct CubesUpdateSystem : ISystem
{
   public void OnCreate(ref SystemState state)
   {
       var entity = state.EntityManager.CreateEntity();
       state.EntityManager.AddBuffer<VoxelBuffer>(entity);
       state.EntityManager.AddComponent<Settings>(entity);
       var compData = state.EntityManager.GetBuffer<VoxelBuffer>(entity);
       var settingsData = state.EntityManager.GetComponentData<Settings>(entity);
       settingsData.GridSize = 3;

       Voxel[] voxels =  CreateVoxels(1,3);
       
       for (int i = 0; i < voxels.Length; i++)
       {
           compData.Add(new VoxelBuffer { Voxel = voxels[i] });
       }
   
   }

   public void OnUpdate(ref SystemState state)
   {
      
   }

   public void OnDestroy(ref SystemState state)
   {
      
   }

   void RebuilMesh()
   {
      
   }
   [InternalBufferCapacity(128)]  
   public struct VoxelBuffer : IBufferElementData
   {
       public Voxel Voxel;
   }

   public struct Settings : IComponentData
   {
       public int GridSize;
   }
   

   public struct Voxel
   {
       public byte IsDestroyed;
       public byte Ground;
       public ushort Integrity;
   }

   Voxel[] CreateVoxels(int scale,int gridSize)
   {
       Voxel[] voxels = new Voxel[gridSize * gridSize * gridSize];
       
       for (int y =0; y < gridSize  ; y++)
       {
           for (int z = 0; z < gridSize ; z++)
           {
               for (int x = 0; x < gridSize ; x++)
               {
                    int3 pos = new  int3(x, y, z);
                   int index =VoxelHelper.GetIndex(pos,gridSize);
                   Voxel voxel = new Voxel{Ground =  1,IsDestroyed = 0,Integrity = 100};
                   voxels[index] = voxel;
               }
           }
       }
       return voxels;
   }

   public class VoxelHelper
   {
       public static int GetIndex(int3 pos,int size)
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

       public static int3 FromIndex(int i,int size)
       {
           int x = i % size;
           int y = (i / size) % size;
           int z = i / (size * size);
           
           return new int3(x,y,z);
       }
   }
}
