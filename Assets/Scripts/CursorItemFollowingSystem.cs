using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jun
{
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    public partial class CursorItemFollowingSystem : SystemBase
    {
        //private Entity cursorContainerEntity;
        private DynamicBuffer<CursorItemContainerBuffer> cursorItemBuffer;
        private CursorItemState cursorItemState;

        private PhysicsWorldSingleton physicsWorldSingleton;
        private RaycastInput raycastInput;

        private DominoInputScript _inputScript;
        private InputAction _itemPlacementAction => _inputScript.Player.ItemPlacement;
        private InputAction _itemRemoveAction => _inputScript.Player.ItemRemove;
        private InputAction _itemRotateAction => _inputScript.Player.ItemRotate;

        protected override void OnCreate()
        {
            RequireForUpdate<CursorItemState>();
            RequireForUpdate<CursorItemContainerBuffer>();
            _inputScript = new DominoInputScript();
        }

        protected override void OnStartRunning()
        {
            _itemPlacementAction.Enable();
            _itemRemoveAction.Enable();
            _itemRotateAction.Enable();

            _itemPlacementAction.performed += OnPlaceItem;
        }

        protected override void OnStopRunning()
        {
            _itemPlacementAction.Disable();
            _itemRemoveAction.Disable();
            _itemRotateAction.Disable();

            _itemPlacementAction.performed -= OnPlaceItem;
        }

        protected override void OnUpdate()
        {
            physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var ecbSingleton = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);

            cursorItemBuffer = SystemAPI.GetSingletonBuffer<CursorItemContainerBuffer>(true);
            cursorItemState = SystemAPI.GetSingleton<CursorItemState>();

            if (cursorItemState.NewType == ItemType.None && cursorItemState.CurrentType == ItemType.None) return;

            UpdateCursorItemColor(ecb);
            UpdateCursorItemType(ecb);

            CursorItemFollowMethod(ecb, ref physicsWorldSingleton.PhysicsWorld.CollisionWorld);

            if (_itemRemoveAction.IsPressed())
            {
                OnRemoveItem(ecb, ref physicsWorldSingleton.PhysicsWorld.CollisionWorld);
            }
        }

        private void CursorItemFollowMethod(EntityCommandBuffer ecb, ref CollisionWorld collisionWorld)
        {
            if (cursorItemState.CurrentType == ItemType.None) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            raycastInput = new RaycastInput
            {
                Start = ray.origin,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3)
                },
                End = ray.GetPoint(10f)
            };

            if (collisionWorld.CastRay(raycastInput, out var hit))
            {
                var entity = cursorItemBuffer[(int)cursorItemState.CurrentType].Entity;
                var rotation = SystemAPI.GetComponentRO<LocalTransform>(entity).ValueRO.Rotation;
                // rotate
                if (_itemRotateAction.IsPressed())
                {
                    rotation *= OnRotateCursorItem(rotation);
                }
                var newPosition = hit.Position;

                // Offset for cubes
                if (cursorItemState.CurrentType == ItemType.Cube) newPosition += new float3(0, 0.05f, 0);

                ecb.SetComponent(entity, LocalTransform.FromPositionRotation(newPosition, rotation));
            }
        }

        private void UpdateCursorItemType(EntityCommandBuffer ecb)
        {            
            if (cursorItemState.NewType == ItemType.None)
            {
                //ecb.AddComponent<Disabled>(buffer.Entity);
                foreach (var buffer in cursorItemBuffer)
                {
                    var groupBuffer = SystemAPI.GetBuffer<LinkedEntityGroup>(buffer.Entity);
                    foreach (var child in groupBuffer)
                    {
                        ecb.AddComponent<Disabled>(child.Value);
                    }
                }
                return;
            }
            else
            {
                if (cursorItemState.CurrentType != cursorItemState.NewType)
                {
                    var newState = new CursorItemState
                    {
                        CurrentType = cursorItemState.NewType,
                        NewType = cursorItemState.NewType,
                        NewColor = cursorItemState.NewColor,
                        CurrentColor = cursorItemState.CurrentColor
                    };

                    Debug.Log($"NewState: {newState.CurrentType}, {newState.NewType}, {newState.CurrentColor}, {newState.NewColor}.");

                    SystemAPI.SetSingleton(newState);

                    var newIndex = (int)cursorItemState.NewType;
                    Debug.Log($"Changing cursor item from {cursorItemState.CurrentType} to {cursorItemState.NewType}");
                    for (var i = 0; i < cursorItemBuffer.Length; i++)
                    {
                        var entity = cursorItemBuffer[i].Entity;
                        if (i == newIndex)
                        {
                            if (SystemAPI.HasComponent<Disabled>(entity))
                            {
                                //ecb.RemoveComponent<Disabled>(entity);
                                var groupBuffer = SystemAPI.GetBuffer<LinkedEntityGroup>(entity);
                                foreach (var child in groupBuffer)
                                {
                                    ecb.RemoveComponent<Disabled>(child.Value);
                                }
                                //Debug.Log($"{entity} enabled.");
                            }
                        }
                        else
                        {
                            if (!SystemAPI.HasComponent<Disabled>(entity))
                            {
                                //ecb.AddComponent<Disabled>(entity);                                
                                var groupBuffer = SystemAPI.GetBuffer<LinkedEntityGroup>(entity);
                                foreach (var child in groupBuffer)
                                {
                                    ecb.AddComponent<Disabled>(child.Value);
                                }

                            }
                        }
                    }
                }
            }
        }

        private void UpdateCursorItemColor(EntityCommandBuffer ecb)
        {
            if (cursorItemState.NewColor != cursorItemState.CurrentColor)
            {
                Debug.Log($"Changing color from {cursorItemState.CurrentColor} to {cursorItemState.NewColor}");
                var newState = new CursorItemState
                {
                    CurrentType = cursorItemState.CurrentType,
                    NewType = cursorItemState.NewType,
                    NewColor = cursorItemState.NewColor,
                    CurrentColor = cursorItemState.NewColor
                };
                
                SystemAPI.SetSingleton(newState);
                var itemColor = new ItemMaterialColor
                {
                    Value = new float4(
                        cursorItemState.NewColor.linear.r,
                        cursorItemState.NewColor.linear.g, 
                        cursorItemState.NewColor.linear.b, 
                        cursorItemState.NewColor.linear.a)
                };

                // Update Color for all cursorItems, not only the current one.
                foreach(var buffer in cursorItemBuffer)
                {
                    var entity = buffer.Entity;
                    if (SystemAPI.HasComponent<ItemMaterialColor>(entity))
                    {
                        ecb.SetComponent(entity, itemColor);
                        var thisType = SystemAPI.GetComponent<ItemInfoComponent>(entity).ItemType;

                        if (thisType == ItemType.FaceColor || thisType == ItemType.EdgeColor)
                        {
                            ecb.AddComponent(entity, new AppendingColorEntity
                            {
                                Index = (int)thisType,
                                Value = itemColor
                            });
                        }
                    }
                }

                //var curType = cursorItemState.NewType;
                //var curEntity = cursorItemBuffer[(int)curType].Entity;                
                //if (SystemAPI.HasComponent<ItemMaterialColor>(curEntity))
                //{
                //    ecb.SetComponent(curEntity, itemColor);

                //    if (curType == ItemType.FaceColor || curType == ItemType.EdgeColor)
                //    {
                //        ecb.AddComponent(curEntity, new AppendingColorEntity
                //        {
                //            Index = (int)curType,
                //            Value = itemColor
                //        });
                //    }
                //}
            }
        }

        private void OnRemoveItem(EntityCommandBuffer ecb, ref CollisionWorld collisionWorld)
        {
            bool isOnUI = PlayerManager.Instance.IsOnUI;
            if (isOnUI) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            raycastInput = new RaycastInput
            {
                Start = ray.origin,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3)
                },
                End = ray.GetPoint(10f)
            };

            if (collisionWorld.CastRay(raycastInput, out var hit))
            {
                var entity = hit.Entity;
                if (SystemAPI.HasComponent<RemovableItem>(entity))
                {
                    ecb.DestroyEntity(entity);
                }
            }
        }

        private void OnPlaceItem(InputAction.CallbackContext context)
        {
            cursorItemState = SystemAPI.GetSingleton<CursorItemState>();
            bool isOnUI = PlayerManager.Instance.IsOnUI;
            if (isOnUI) return;
            if (cursorItemState.CurrentType == ItemType.None) return;

            var cursorEntity = cursorItemBuffer[(int)cursorItemState.CurrentType].Entity;
            var trans = SystemAPI.GetComponent<LocalTransform>(cursorEntity);

            var itemColor = new ItemMaterialColor();
            // Get the color if this entity's color can be changed.
            if (SystemAPI.HasComponent<ItemMaterialColor>(cursorEntity))
            {
                itemColor = SystemAPI.GetComponent<ItemMaterialColor>(cursorEntity);
            }

            SystemAPI.GetSingletonBuffer<ItemPlacementInput>().Add(new ItemPlacementInput
            {
                ItemType = cursorItemState.CurrentType,
                LocalTransform = trans,
                Color = itemColor
            });
        }

        private Quaternion OnRotateCursorItem(Quaternion q)
        {
            var y = 1f * SystemAPI.Time.DeltaTime;
            q = quaternion.RotateY(y);
            return q;
        }
    }
}