using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.UI
{
    using Config;
    using System.Linq;

    public class LoadedBoardUI : MonoBehaviour
    {
        public Transform figuresParent;
        public Text nameText;
        
        MenuManager manager;

        SavedGameData game;

        string Name => game.name;

        public void Init(SavedGameData game, MenuManager manager)
        {
            this.game = game;
            this.manager = manager;

            nameText.text = game.name;
            DrawBoard();
        }

        void DrawBoard()
        {
            var board = game.boards.Last();

            for(int x = 0; x < 8; x++) for(int y = 0; y < 8; y++)
            {
                    var figure = board[x, y];

                    var tileObj = Instantiate(manager.loadedBoardTilePrefab, figuresParent);
                    var img = tileObj.GetComponent<Image>();

                    img.enabled = false; // TODO fix positions at some point and bring back this feature

                    //if (figure == Figure.Empty) img.enabled = false;
                    //else img.sprite = BoardConfigManager.instance.Chosen.figuresFX[(int)figure].sprite2D;
            }
        }

        void OnValidate()
        {
            if (figuresParent == null) figuresParent = transform;
        }

        public void OnSelect()
        {
            manager.PlayOldLoad(Name);
        }

        public void OnDelete()
        {
            manager.RemoveLoadedBoard(Name);
            Destroy(gameObject);
        }
    }
}
