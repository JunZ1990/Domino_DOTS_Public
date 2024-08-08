using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Jun
{
    public struct SelectableObjectTag : IComponentData { }
    public struct RemovableItem : IComponentData { }

    public struct ItemInfoComponent : IComponentData
    {
        public ItemType ItemType;
    }

    public struct ChildWithPhysicsBodyComponent : IComponentData
    {
        public int Index;
        public float3 Offset_Position;
        public float3 Offset_Rotation;
    }

    [Serializable]
    [MaterialProperty("_BaseColor")]
    public struct ItemMaterialColor : IComponentData
    {
        /// <summary>
        /// The RGBA color value.
        /// </summary>
        public float4 Value;
    }

    public struct AppendingColorEntity : IComponentData
    {
        public int Index;
        public ItemMaterialColor Value;
    }

    public struct ItemPlacementInput : IBufferElementData
    {
        public ItemType ItemType;
        public LocalTransform LocalTransform;
        public ItemMaterialColor Color;
    }

    public struct ItemRemoveInput : IBufferElementData
    {
        public RaycastInput Input;
    }
}