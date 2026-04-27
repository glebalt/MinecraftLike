using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class CubeMovementAuthoring : MonoBehaviour
{
    public float yAngle;
 
        
    public struct Rotate : IComponentData
    {
        public float yAngle;
       
    }
    
    public struct Move : IComponentData
    {
        public float3 movementVector;

    }

    public struct RotatingCube : IComponentData
    {
        
    }

    public class Baker : Baker<CubeMovementAuthoring>
    {
        
        public override void Bake(CubeMovementAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Rotate { yAngle = authoring.yAngle });
            AddComponent(entity,new Move{movementVector = new float3{x = Random.Range(-1f,1f),y = 0,z = Random.Range(-1f,1f)}});
        }
    }
}
