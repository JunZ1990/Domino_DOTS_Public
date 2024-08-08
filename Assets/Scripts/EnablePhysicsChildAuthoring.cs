using UnityEngine;
using Unity.Entities;

namespace Jun
{
    public class EnablePhysicsAuthoring : MonoBehaviour
    {

    }

    public class EnablePhysicsBaker : Baker<EnablePhysicsAuthoring>
    {
        public override void Bake(EnablePhysicsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnablePhysicsTag());
        }
    }

    public struct EnablePhysicsTag : IComponentData { }
}