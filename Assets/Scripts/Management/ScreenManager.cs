using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager instance;

    public OnScreenSizeChanged onScreenSizeChanged;

    Vector2Int size;

    bool startChecking = false;

    void Awake()
    {
        instance = this;
        onScreenSizeChanged = null;
    }

    void Start()
    {
        Invoke("CheckChange", 0.4f);
    }

    void CheckChange()
    {
        startChecking = true;
        var newSize = new Vector2Int(Screen.width, Screen.height);

        if(newSize != size)
        {
            //Debug.Log($"Resolution changed from {size} to {newSize}");
            onScreenSizeChanged?.Invoke(size, newSize);
            size = newSize;
            //Debug.Log("Done calling screen size change events");
        }
    }

    void FixedUpdate()
    {
        if (startChecking) CheckChange();        
    }
}

public delegate void OnScreenSizeChanged(Vector2Int oldSize, Vector2Int newSize);