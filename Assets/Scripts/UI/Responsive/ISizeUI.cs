using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.UI.Responsive
{
    public interface ISizeUI<T>
    {
        Vector2Int Size { get; }
        GameObject FX { get; }
        T Data { get; }

        void OnActivated(T oldData);
    }
}