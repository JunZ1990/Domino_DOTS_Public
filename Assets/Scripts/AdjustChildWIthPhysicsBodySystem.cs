using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

namespace Jun
{
    [RequireMatchingQueriesForUpdateAttribute]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(ItemPlacementSystem))]
    [BurstCompile]
    public partial struct AdjustChildWithPhysicsBodySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChildWithPhysicsBodyComponent>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new AdjustChildWithPhysicsBodyJob
            {
                ECB = ecb
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct AdjustChildWithPhysicsBodyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        void Execute(in LocalTransform transform, in ChildWithPhysicsBodyComponent comp,
            in DynamicBuffer<LinkedEntityGroup> group, [EntityIndexInQuery] int sortKey)
        {
            var child = group[comp.Index].Value;
            var newTrans = new LocalTransform
            {
                Position = transform.Position + comp.Offset_Position,
                Rotation = math.mul(transform.Rotation, quaternion.Euler(comp.Offset_Rotation)),
                Scale = 1
            };
            //Debug.Log($"Adjust {child.ToFixedString()} trans.");
            ECB.SetComponent(sortKey, child, newTrans);

            ECB.RemoveComponent<ChildWithPhysicsBodyComponent>(sortKey, group[0].Value);

            ECB.SetEnabled(sortKey, child, true);
            ECB.SetEnabled(sortKey, group[comp.Index + 1].Value, true); // A new child entity for physics joint
        }
    }
}