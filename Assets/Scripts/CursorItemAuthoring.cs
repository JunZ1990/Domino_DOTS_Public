using UnityEngine;
using Unity.Entities;

namespace Jun
{
    public class CursorItemAuthoring : MonoBehaviour
    {
        public ItemType ItemType;
        public bool IsColorful;
    }

    public class CursorItemBaker : Baker<CursorItemAuthoring>
    {
        public override void Bake(CursorItemAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ItemInfoComponent
            {
                ItemType = authoring.ItemType
            });

            if (authoring.IsColorful)
            {
                AddComponent(entity, new ItemMaterialColor
                {
                    Value = Vector4.one
                });
            }
        }
    }
}