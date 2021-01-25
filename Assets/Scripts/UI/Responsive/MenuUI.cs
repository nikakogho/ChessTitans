using Chess.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace Chess.UI.Responsive
{
    public class MenuUI : MonoBehaviour, ISizeUI<MenuUI.UIData>
    {
        [SerializeField] Vector2Int _size;
        [SerializeField] GameObject _fx;
        [SerializeField] UIData _data;

        public Vector2Int Size => _size;

        public GameObject FX => _fx;

        public UIData Data => _data;

        #region Button Clicks
        public void DisplayLevels()
        {
            LevelsManager.instance.ApplyAnims();
        }

        public void Reset()
        {
            MenuManager.instance.ResetStats();
        }

        public void Exit()
        {
            MenuManager.instance.Exit();
        }

        public void SetPlayerSide(bool white)
        {
            MenuManager.instance.OnPlayerSideChanged(white);
        }

        public void PreviousBoardVisual()
        {
            BoardConfigManager.instance.Previous();
        }

        public void NextBoardVisual()
        {
            BoardConfigManager.instance.Next();
        }

        public void New2Player()
        {
            MenuManager.instance.Play2Players();
        }
        #endregion

        public void OnActivated(UIData oldData)
        {
            //Debug.Log("Started checking activated");
            if(oldData == null)
            {
                Data.optionsUI.SetActive(true);
                Data.newGameUI.SetActive(false);
                Data.backButtonUI.SetActive(false);
                Data.loadGameUI.SetActive(false);
                Data._1PlayerUI.SetActive(false);
                Data._2PlayersUI.SetActive(false);

                Data.chosenBoardText.text = "Marble Board";

                //Data.whiteSideImage.sprite = oldData.whiteSideImage.sprite;
                //Data.blackSideImage.sprite = oldData.blackSideImage.sprite;

                Data.somethingToLoadUI.SetActive(true);
                Data.nothingToLoadUI.SetActive(false);
            }
            else
            {
                Data.optionsUI.SetActive(oldData.optionsUI.activeSelf);
                Data.newGameUI.SetActive(oldData.newGameUI.activeSelf);
                Data.backButtonUI.SetActive(oldData.backButtonUI.activeSelf);
                Data.loadGameUI.SetActive(oldData.loadGameUI.activeSelf);
                Data._1PlayerUI.SetActive(oldData._1PlayerUI.activeSelf);
                Data._2PlayersUI.SetActive(oldData._2PlayersUI.activeSelf);

                Data.chosenBoardText.text = oldData.chosenBoardText.text;

                Data.whiteSideImage.sprite = oldData.whiteSideImage.sprite;
                Data.blackSideImage.sprite = oldData.blackSideImage.sprite;

                Data.somethingToLoadUI.SetActive(oldData.somethingToLoadUI.activeSelf);
                Data.nothingToLoadUI.SetActive(oldData.nothingToLoadUI.activeSelf);

                while (oldData.loadedBoardsParent.childCount > 0)
                {
                    oldData.loadedBoardsParent.GetChild(0).SetParent(Data.loadedBoardsParent, false);
                }

                while (oldData.levelsParent.childCount > 0)
                {
                    oldData.levelsParent.GetChild(0).SetParent(Data.levelsParent, false);
                }
            }
            
            FX.SetActive(true);
            //Debug.Log("Done checking activated");
        }

        [System.Serializable]
        public class UIData
        {
            public Image whiteSideImage, blackSideImage;
            public Text chosenBoardText;

            [Header("Options")]
            public GameObject optionsUI;
            public GameObject newGameUI;
            public GameObject backButtonUI;
            public GameObject loadGameUI;
            public GameObject _1PlayerUI;
            public GameObject _2PlayersUI;

            [Header("Loaded Board UI")]
            public GameObject somethingToLoadUI;
            public GameObject nothingToLoadUI;
            public Transform loadedBoardsParent;
            public Transform levelsParent;
        }
    }
}