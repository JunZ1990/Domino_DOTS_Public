using Unity.Entities;
// From https://forum.unity.com/threads/i-want-to-create-an-entity-and-store-it-in-a-dynamic-buffer.1022968/
// Lieene-Guo: "The entity does not exist before the ECB is playedback,
// but it could exist in the current frame. If you Run a system in
// InitializationSystemGroup and create entity with EndInitializationCommandBufferSystem.
// the entity exist in Simulation and presentation of the current frame."
namespace Jun
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class InitialCursorItemSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<CursorItemPrefabsBuffer>();
            RequireForUpdate<CursorItemContainerBuffer>();
        }

        protected override void OnUpdate()
        {
            Enabled = false;

            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().
                CreateCommandBuffer(World.Unmanaged);
            var prefabsBuffer = SystemAPI.GetSingletonBuffer<CursorItemPrefabsBuffer>(true);

            var cursorItemBuffer = SystemAPI.GetSingletonBuffer<CursorItemContainerBuffer>();
            var containerEntity = SystemAPI.GetSingletonEntity<CursorItemContainerBuffer>();

            foreach (var buffer in prefabsBuffer)
            {
                var entity = ecb.Instantiate(buffer.Entity);
                
                ecb.AppendToBuffer(containerEntity, new CursorItemContainerBuffer
                {
                    Entity = entity
                });
            }
        }
    }
}