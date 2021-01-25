using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Config
{
    [CreateAssetMenu(fileName = "New Board Visuals", menuName = "Board/Visuals")]
    public class BoardConfig : ScriptableObject
    {
        new public string name;

        public GameObject boardPrefab;
        public FigureFX[] figuresFX = new FigureFX[Enum.GetNames(typeof(Figure)).Length];

        [Serializable]
        public class FigureFX
        {
            public GameObject prefab3D;
            public GameObject prefab2D;
            public Sprite sprite2D;
        }
    }
}
