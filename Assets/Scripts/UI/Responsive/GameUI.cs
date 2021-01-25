using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace Chess.UI.Responsive
{
    public class GameUI : MonoBehaviour, ISizeUI<GameUI.UIData>
    {
        [SerializeField] Vector2Int _size;
        [SerializeField] GameObject _fx;
        [SerializeField] UIData _data;

        public Vector2Int Size => _size;

        public GameObject FX => _fx;

        public UIData Data => _data;

        #region Button Clicks
        public void ToggleAudio()
        {
            GameManager.instance.ToggleAudio();
        }

        public void ToggleDimensions()
        {
            GameManager.instance.Switch2D3D();
        }

        public void NextLevel(bool asWhite)
        {
            GameManager.instance.NextLevel(asWhite);
        }

        public void Undo()
        {
            GameManager.instance.Undo();
        }

        public void Restart()
        {
            GameManager.instance.ResetBoard();
        }

        public void Menu()
        {
            GameManager.instance.GoToMenu();
        }

        public void Exit()
        {
            GameManager.instance.Exit();
        }
        #endregion

        public void OnActivated(UIData oldData)
        {
            if(oldData == null)
            {
                Data.gameOverUI.SetActive(false);
                Data.audioAllowedImage.sprite = GameManager.instance.audioEnabledSprite;
                Data.saveUI.SetActive(false);
            }
            else
            {
                Data.whiteScoreText.text = oldData.whiteScoreText.text;
                Data.blackScoreText.text = oldData.blackScoreText.text;
                Data.gameOverUI.SetActive(oldData.gameOverUI.activeSelf);
                Data.gameOverText.text = oldData.gameOverText.text;
                Data.nextLevelUI.SetActive(oldData.nextLevelUI.activeSelf);
                Data.audioAllowedImage.sprite = oldData.audioAllowedImage.sprite;

                Data.saveUI.SetActive(oldData.saveUI.activeSelf);
                Data.saveText.text = oldData.saveText.text;
            }

            GameManager.instance.cam3D.usePhysicalProperties = Data.cam3DUsePhysicalProperties;
            GameManager.instance.cam3D.fieldOfView = Data.cam3DFieldOfView;
            GameManager.instance.cam2D.orthographicSize = Data.cam2DFieldOfView;
            FX.SetActive(true);
        }

        [System.Serializable]
        public class UIData
        {
            public Transform whitesCameraSpot, blacksCameraSpot;

            [Header("Camera")]
            public bool cam3DUsePhysicalProperties;
            public float cam3DFieldOfView;
            public float cam2DFieldOfView;

            [Header("UI")]
            public GameObject gameOverUI;
            public Text gameOverText;
            public Text whiteScoreText, blackScoreText;
            public GameObject nextLevelUI;
            public Image audioAllowedImage;

            [Header("Save")]
            public GameObject saveUI;
            public InputField saveText;
        }
    }
}