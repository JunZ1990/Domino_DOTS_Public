using System.Collections;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Jun
{
    [RequireComponent (typeof(PlayerManager))]
    public class PlayerInputManager : MonoBehaviour
    {        
        public PlayerManager PlayerManager;
        public Camera Camera;

        public float MovementSpeed;
        public float RotationSpeed;
        private float xRotation = 0f;

        private Transform trans_Player;

        //private Entity entity;
        private World world;

        private Entity cursorEntity;
        private CursorItemState cursorState;
        private Color newColor = Color.white;

        private DominoInputScript _inputScript;
        private InputAction _escAction => _inputScript.Player.EscAction;
        private InputAction _moveAction => _inputScript.Player.Move;
        private InputAction _moveUpAction => _inputScript.Player.MoveUp;
        private InputAction _moveDownAction => _inputScript.Player.MoveDown;
        private InputAction _lookAction => _inputScript.Player.Look;
        private InputAction _cameraRotateAction => _inputScript.Player.CameraRotate;
        private InputAction _openColorPieAction => _inputScript.Player.OpenColorPie;
        private InputAction _openItemPieAction => _inputScript.Player.OpenItemPie;

        private void Awake()
        {
            _inputScript = new DominoInputScript();
            trans_Player = GetComponent<Transform>();
            PlayerManager = GetComponent<PlayerManager>();
        }

        private void OnEnable()
        {
            _escAction.Enable();
            _moveAction.Enable();
            _moveUpAction.Enable();
            _moveDownAction.Enable();
            _lookAction.Enable();
            _cameraRotateAction.Enable();
            _openColorPieAction.Enable();
            _openItemPieAction.Enable();

            _escAction.performed += EscMethod;
            _openColorPieAction.performed += OpenColorPie;
            _openItemPieAction.performed += OpenItemPie;

            Camera = Camera == null ? Camera.main : Camera;

            world = World.DefaultGameObjectInjectionWorld;
        }

        private void OnDisable()
        {
            _escAction.Disable();
            _moveAction.Disable();
            _moveUpAction.Disable();
            _moveDownAction.Disable();
            _lookAction.Disable();
            _cameraRotateAction.Disable();
            _openColorPieAction.Disable();
            _openItemPieAction.Disable();

            _escAction.performed -= EscMethod;
            _openColorPieAction.performed -= OpenColorPie;
            _openItemPieAction.performed -= OpenItemPie;

            //if (world.IsCreated && world.EntityManager.Exists(entity))
            //{
            //    world.EntityManager.DestroyEntity(entity);
            //}
        }

        private void Start()
        {
            SetBtnsEvent();
        }

        private void FixedUpdate()
        {
            if (world.IsCreated && !world.EntityManager.Exists(cursorEntity))
            {
                //EntityQuery entityQuery = 
                //    world.EntityManager.CreateEntityQuery(typeof(ItemPrefabEntitiesBuffer));
                //entity = entityQuery.ToEntityArray(Allocator.Temp)[0];

                EntityQuery query2 = world.EntityManager.CreateEntityQuery(typeof(CursorItemState));
                if (query2 == null) return;
                if (query2.IsEmpty) return;
                cursorEntity = query2.ToEntityArray(Allocator.Temp)[0];
            }

            PlayerMovement();
            PlayerLook();
            
            if (PlayerManager.IsOnUI) return;

            // Updating color
            cursorState = world.EntityManager.GetComponentData<CursorItemState>(cursorEntity);
            if (newColor != cursorState.NewColor)
            {
                world.EntityManager.SetComponentData(cursorEntity, new CursorItemState
                {
                    CurrentType = cursorState.CurrentType,
                    NewType = cursorState.NewType,
                    CurrentColor = cursorState.CurrentColor,
                    NewColor = newColor
                });
            }
        }

        private void SetBtnsEvent()
        {
            foreach (Button btn in PlayerManager.Btns_UiItems)
            {
                btn.onClick.AddListener(() => ClickOnItemSelectionButtonListener(btn));
            }
            PlayerManager.Btn_ColorPie.onClick.AddListener(ClickOnColorPie);
        }

        private void ClickOnColorPie()
        {
            int x = (int)Input.mousePosition.x;
            int y = (int)Input.mousePosition.y;
            StartCoroutine(GetColor(x, y));
        }
        IEnumerator GetColor(int x, int y)
        {
            int width = Screen.width;
            int height = Screen.height;
            yield return new WaitForEndOfFrame();

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, true);

            newColor = tex.GetPixel(x, y);
            
            PlayerManager.CloseUIs();
        }

        private void ClickOnItemSelectionButtonListener(Button button)
        {
            var btnName = button.name;
            var lastString = btnName.Split(' ').Last();
            Debug.Log(btnName + " clicked.");

            if (int.TryParse(lastString, out int id))
            {
                world.EntityManager.SetComponentData(cursorEntity, new CursorItemState
                {
                    CurrentType = cursorState.NewType,
                    NewType = (ItemType)id,
                    CurrentColor = cursorState.CurrentColor,
                    NewColor = cursorState.NewColor
                });
            }
            else
            {
                Debug.LogWarning($"{btnName} clicked, cant convert to an int id, the last string is {lastString}.");
            }

            PlayerManager.CloseUIs();
        }

        private void EscMethod(InputAction.CallbackContext context)
        {
            world.EntityManager.SetComponentData(cursorEntity, new CursorItemState
            {
                CurrentType = cursorState.CurrentType,
                NewType = ItemType.None,
                CurrentColor = cursorState.CurrentColor,
                NewColor = cursorState.NewColor
            });

            if (PlayerManager.IsOnUI)
            {
                PlayerManager.CloseUIs();
            }
            else
            {
                PlayerManager.Canvas_Main.enabled = true;
                PlayerManager.PauseGame();
            }            
        }

        private void PlayerMovement()
        {
            Vector2 moveDir = _moveAction.ReadValue<Vector2>();
            Vector3 newTrans = new Vector3(moveDir.x, 0f, moveDir.y);
            trans_Player.Translate(newTrans * MovementSpeed * Time.deltaTime);

            if (_moveUpAction.IsPressed())
            {
                trans_Player.Translate(0, MovementSpeed * Time.deltaTime, 0, Space.World);
            }
            if (_moveDownAction.IsPressed())
            {
                trans_Player.Translate(0, -MovementSpeed * Time.deltaTime, 0, Space.World);
            }            
        }

        private void PlayerLook()
        {
            if (_cameraRotateAction.IsPressed())
            {
                Vector2 lookDir = _lookAction.ReadValue<Vector2>();

                var lookX = lookDir.x * RotationSpeed * Time.deltaTime;
                var lookY = lookDir.y * RotationSpeed * Time.deltaTime;
                xRotation -= lookY;
                xRotation = Mathf.Clamp(xRotation, -89f, 89f);
                Camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                trans_Player.Rotate(Vector3.up * lookX);
            }
        }

        private void OpenColorPie(InputAction.CallbackContext context)
        {
            PlayerManager.BtnOpenColorPieClicked();
        }

        private void OpenItemPie(InputAction.CallbackContext context)
        {
            PlayerManager.BtnOpenItemPieClicked();
        }
    }
}