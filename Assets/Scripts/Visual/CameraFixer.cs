using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFixer : MonoBehaviour
{
    public MeshRenderer board;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;

        InvokeRepeating("SnapCamera", 0.1f, 0.5f);
    }

    void SnapCamera()
    {
        if (Camera.main == null) return;

        const float margin = 1.6f;
        float maxExtent = board.bounds.extents.magnitude;
        float minDistance = (maxExtent * margin) / Mathf.Sin(Mathf.Deg2Rad * cam.fieldOfView / 2.0f);

        Vector3 back = (-board.transform.position + cam.transform.position).normalized;

        Camera.main.transform.position = back * minDistance;
        Camera.main.transform.LookAt(board.transform);
    }
}
