using System.Collections.Generic;
using UnityEngine;

public class KilledFigureVisual : MonoBehaviour
{
    public Transform fxParent;

    GameObject fx3D;
    GameObject fx2D;

    bool is3D;

    bool Showing => fx3D != null;

    public void Init(GameObject prefab3D, GameObject prefab2D, bool show3D)
    {
        Clear();

        fx3D = Instantiate(prefab3D, fxParent.position, fxParent.rotation);
        fx2D = Instantiate(prefab2D, fxParent.position, fxParent.rotation);

        fx3D.transform.parent = fx2D.transform.parent = fxParent;

        SetDimensions(show3D);
    }

    public void Clear()
    {
        if (fx3D != null) Destroy(fx3D);
        if (fx2D != null) Destroy(fx2D);
    }

    void SetDimensions(bool show3D)
    {
        is3D = !show3D;
        SwitchDimensions();
    }

    public void SwitchDimensions()
    {
        is3D = !is3D;

        if (!Showing) return;

        fx3D.SetActive(is3D);
        fx2D.SetActive(!is3D);
    }
}