using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Jun
{
    public class CursorItemContainerAuthoring : MonoBehaviour
    {
        public GameObject[] Objs_CursorItems;
    }

    public class CursorItemContainerBaker : Baker<CursorItemContainerAuthoring>
    {
        public override void Bake(CursorItemContainerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var itemsBuffer = AddBuffer<CursorItemPrefabsBuffer>(entity);
            foreach(var obj in authoring.Objs_CursorItems)
            {
                itemsBuffer.Add(new CursorItemPrefabsBuffer
                {
                    Entity = GetEntity(obj, TransformUsageFlags.Dynamic)
                });
            }
            AddComponent(entity, new CursorItemState
            {
                CurrentType = ItemType.None,
                NewType = ItemType.None,
                NewColor = Color.white,
                CurrentColor = Color.white
            });
            AddBuffer<CursorItemContainerBuffer>(entity);
        }
    }

    public struct CursorItemPrefabsBuffer : IBufferElementData
    {
        public Entity Entity;
    }

    public struct CursorItemContainerBuffer : IBufferElementData
    {
        public Entity Entity;
    }

    public struct CursorItemState : IComponentData
    {
        public ItemType CurrentType;
        public ItemType NewType;

        public Color CurrentColor;
        public Color NewColor;
    }
}