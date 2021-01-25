using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelOption : MonoBehaviour
{
    public Image image;
    public Button button;
    public Text text;

    public Animator anim;
    
    LevelsManager.LevelData data;
    bool unlocked;

    public void Init(LevelsManager.LevelData data, bool unlocked)
    {
        this.data = data;
        this.unlocked = unlocked;

        text.text = $"Level {data.level}";
        button.interactable = unlocked;
        image.sprite = unlocked ? data.icon : data.lockedIcon;
    }

    public void Unlock()
    {
        if (unlocked) return;

        unlocked = true;
        button.interactable = true;
        image.sprite = data.icon;
    }

    public void Lock()
    {
        if (!unlocked) return;

        unlocked = false;
        button.interactable = false;
        image.sprite = data.lockedIcon;
    }

    public void Play()
    {
        MenuManager.instance.Play(data.level);
    }
}
