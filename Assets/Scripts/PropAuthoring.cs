using UnityEngine;
using Unity.Entities;

namespace Jun
{
    public class PropAuthoring : MonoBehaviour
    {
        public ItemType ItemType;

        public int index_ChildWithPhysicsBody = 0;
        public Vector3 Offset_Position;
        public Vector3 Offset_Rotation;
    }

    public class PropBaker : Baker<PropAuthoring>
    {
        public override void Bake(PropAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new ItemInfoComponent
            {
                ItemType = authoring.ItemType,

            });
            AddComponent(entity, new SelectableObjectTag());
            AddComponent(entity, new RemovableItem());

            if (authoring.index_ChildWithPhysicsBody != 0)
            {
                AddComponent(entity, new ChildWithPhysicsBodyComponent
                {
                    Index = authoring.index_ChildWithPhysicsBody,
                    Offset_Position = authoring.Offset_Position,
                    Offset_Rotation = authoring.Offset_Rotation
                });
            }
        }
    }
}