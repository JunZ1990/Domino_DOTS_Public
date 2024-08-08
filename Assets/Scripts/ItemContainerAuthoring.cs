using UnityEngine;
using Unity.Entities;

namespace Jun
{
    public class ItemContainerAuthoring : MonoBehaviour
    {
        public GameObject[] Items;
    }

    public class ItemPrefabBaker : Baker<ItemContainerAuthoring>
    {
        public override void Bake(ItemContainerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var itemsbuffer = AddBuffer<ItemPrefabEntitiesBuffer>(entity);
            for(int i = 0; i< authoring.Items.Length; i++)
            {
                itemsbuffer.Add(new ItemPrefabEntitiesBuffer
                {
                    ItemEntity = GetEntity(authoring.Items[i], TransformUsageFlags.Dynamic)
                });
            }
            AddBuffer<ItemPlacementInput>(entity);
            AddBuffer<ItemRemoveInput>(entity);
        }
    }

    public struct ItemPrefabEntitiesBuffer : IBufferElementData
    {
        public Entity ItemEntity;
    }
}