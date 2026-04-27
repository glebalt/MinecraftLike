using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct CubeMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach ((var rotator,var mover,var transform) in (SystemAPI.Query<RefRO<CubeMovementAuthoring.Rotate>,RefRO<CubeMovementAuthoring.Move>,RefRW<LocalTransform>>()))
        {
            transform.ValueRW = transform.ValueRW.RotateY(rotator.ValueRO.yAngle * SystemAPI.Time.DeltaTime);
            transform.ValueRW = transform.ValueRW.Translate(mover.ValueRO.movementVector * SystemAPI.Time.DeltaTime);
        }
    }
}
