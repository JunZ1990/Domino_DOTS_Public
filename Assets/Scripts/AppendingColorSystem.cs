using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;

namespace Jun
{
    [RequireMatchingQueriesForUpdateAttribute]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(ItemPlacementSystem))]
    [BurstCompile]
    public partial struct AppendingColorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AppendingColorEntity>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            new AppendColorJob
            {
                ECB = ecb
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct AppendColorJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute(in AppendingColorEntity comp, in DynamicBuffer<LinkedEntityGroup> groupBuffer,
            [EntityIndexInQuery] int sortKey)
        {
            var child = groupBuffer[comp.Index].Value;
            ECB.AddComponent(sortKey, child, comp.Value);
            ECB.RemoveComponent<AppendingColorEntity>(sortKey, groupBuffer[0].Value);
        }
    }
}