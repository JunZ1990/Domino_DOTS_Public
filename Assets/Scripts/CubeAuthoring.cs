using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Jun
{
    public class CubeAuthoring : MonoBehaviour
    {
        public ItemType ItemType;
    }

    public class CubeBaker : Baker<CubeAuthoring>
    {
        public override void Bake(CubeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ItemInfoComponent
            {
                ItemType = authoring.ItemType
            });
            AddComponent(entity, new SelectableObjectTag());
            AddComponent(entity, new RemovableItem());
            AddComponent(entity, new ItemMaterialColor
            {
                Value = new float4(1, 1, 1, 1)
            });
        }
    }
}