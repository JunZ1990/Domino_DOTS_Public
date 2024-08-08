using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;

namespace Jun
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct ItemPlacementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ItemPlacementInput>();
            state.RequireForUpdate<ItemPrefabEntitiesBuffer>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            new PlaceItemJob
            {
                ECB = ecb
            }.Schedule();
        }
    }

    [BurstCompile]
    public partial struct PlaceItemJob : IJobEntity
    {
        public EntityCommandBuffer ECB;

        void Execute(ref DynamicBuffer<ItemPlacementInput> input,
            in DynamicBuffer<ItemPrefabEntitiesBuffer> itemPrefabsBuffer)
        {
            if (input.IsEmpty) return;

            foreach (var placementInput in input)
            {
                var itemType = placementInput.ItemType;
                var newItem = ECB.Instantiate(itemPrefabsBuffer[(int)itemType].ItemEntity);
                
                ECB.SetComponent(newItem, placementInput.LocalTransform);

                // For adding color to entity
                if (itemType == ItemType.Normal ||
                    itemType == ItemType.EdgeColor ||
                    itemType == ItemType.FaceColor ||
                    itemType == ItemType.Cube)
                {
                    var childHasColor = itemType == ItemType.FaceColor ||
                                        itemType == ItemType.EdgeColor;

                    ECB.SetComponent(newItem, placementInput.Color);

                    if (childHasColor)
                    {
                        ECB.AddComponent(newItem, new AppendingColorEntity
                        {
                            Index = (int)itemType,
                            Value = placementInput.Color
                        });
                        // ColorFace : LinkedEntityGroup[2] with color mat
                        // ColorEdge : LinkedEntityGroup[1] with color mat
                    }
                }
            }
            input.Clear();
        }
    }
}