using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chess.Config;

namespace Chess.UI
{
    public class BoardConfigOptionUI : MonoBehaviour
    {
        public Transform cameraSpot;
        public Transform boardParent;
        Transform board;
        BoardConfig config;

        public Vector3 startPos;
        public Vector2 delta;

        [ContextMenu("Init With Editor Time Config")]
        void InitWithCurrentConfig()
        {
            if (board != null) DestroyImmediate(board.gameObject);
            Init(config);
        }

        public void Init(BoardConfig config)
        {
            this.config = config;
            Shuffle();
        }

        int Rand(int max)
        {
            return Random.Range(0, max + 1);
        }

        Dictionary<Figure, int> GetFigureAmounts()
        {
            var amounts = new Dictionary<Figure, int>
            {
                [Figure.PawnBlack] = Rand(8),
                [Figure.PawnWhite] = Rand(8),
                [Figure.HorseBlack] = Rand(2),
                [Figure.HorseWhite] = Rand(2),
                [Figure.BishopBlack] = Rand(2),
                [Figure.BishopWhite] = Rand(2),
                [Figure.CastleBlack] = Rand(2),
                [Figure.CastleWhite] = Rand(2),
                [Figure.QueenBlack] = Rand(1),
                [Figure.QueenWhite] = Rand(1),
                [Figure.KingBlack] = 1,
                [Figure.KingWhite] = 1
            };

            return amounts;
        }

        Vector3 GetPosFromIndex(int i)
        {
            int x = i % 8;
            int y = i / 8;

            return startPos + new Vector3(delta.x * x, 0, delta.y * y);
        }

        public void Shuffle()
        {
            if(board != null) Destroy(board.gameObject);

            board = Instantiate(config.boardPrefab, boardParent).transform;

            //Debug.Log("Shuffling");

            var amounts = GetFigureAmounts();
            
            var keys = new List<int>();
            for (int i = 0; i < 64; i++) keys.Add(i);

            foreach(var pair in amounts)
            {
                var figure = pair.Key;
                int amount = pair.Value;

                for (int i = 0; i < amount; i++)
                {
                    int keyIndex = Random.Range(0, keys.Count);
                    int key = keys[keyIndex];

                    keys.RemoveAt(keyIndex);

                    var pos = GetPosFromIndex(key);

                    var obj = Instantiate(config.figuresFX[(int)figure].prefab3D, board);
                    obj.transform.localPosition = pos;
                }
            }

            //Debug.Log("Shuffled");
        }
    }
}
