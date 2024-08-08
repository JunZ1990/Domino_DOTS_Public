using System;
using UnityEngine;
using UnityEngine.UI;

namespace Jun
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        public Button[] Btns_UiItems;
        public Button Btn_ColorPie;

        [Tooltip("The game is paused if is true.")]
        public bool IsOnUI;

        public Canvas Canvas_Main;
        public Button Btn_Save, Btn_Load;

        public GameObject Obj_ColorPie;
        public GameObject Obj_ItemPie;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }

            CloseUIs();
        }

        public void CloseUIs()
        {
            if (Canvas_Main.enabled) Canvas_Main.enabled = false;
            if (Obj_ColorPie.activeSelf) Obj_ColorPie.SetActive(false);
            if (Obj_ItemPie.activeSelf) Obj_ItemPie.SetActive(false);
            IsOnUI = false;
            Time.timeScale = 1f;
        }

        public void PauseGame()
        {
            IsOnUI = true;
            Time.timeScale = 0;
        }

        public void BtnOpenItemPieClicked()
        {
            if (IsOnUI)
            {
                if (Obj_ItemPie.activeSelf)
                {
                    CloseUIs();
                }
            }
            else
            {
                Obj_ItemPie.SetActive(true);
                PauseGame();
            }
        }

        public void BtnOpenColorPieClicked()
        {
            if (IsOnUI)
            {
                if (Obj_ColorPie.activeSelf)
                {
                    CloseUIs();
                }
            }
            else
            {
                Obj_ColorPie.SetActive(true);
                Obj_ColorPie.transform.position = Input.mousePosition;
                PauseGame();
            }
        }
    }
}