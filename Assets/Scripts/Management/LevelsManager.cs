using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    public LevelData[] levels;
    public GameObject optionPrefab;

    LevelOption[] options;

    public static LevelsManager instance;

    void OnEnable()
    {
        instance = this;
        Invoke("Init", 0.45f);
    }

    void Init()
    {
        //Debug.Log("Started initializing level manager");
        int total = levels.Length;
        int oldUnlocked = PlayerPrefs.GetInt("Old Unlocked", 1);

        options = new LevelOption[total];

        var parent = MenuManager.instance.UI.Data.levelsParent;

        for(int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject); // remove all children
        
        for (int i = 0; i < total; i++)
        {
            var obj = Instantiate(optionPrefab, parent);
            var option = obj.GetComponent<LevelOption>();

            option.Init(levels[i], i < oldUnlocked);

            options[i] = option;
        }

        //StartCoroutine(ApplyUnlockAnimations());
        //Debug.Log("Done initializing level manager");
    }

    public void ApplyAnims()
    {
        StartCoroutine(ApplyUnlockAnimations());
    }

    IEnumerator ApplyUnlockAnimations()
    {
        int oldUnlocked = PlayerPrefs.GetInt("Old Unlocked", 1);
        int unlocked = PlayerPrefs.GetInt("Level", 1);

        //for (int i = 0; i < oldUnlocked; i++) options[i].anim.SetBool("unlocked", true);
        
        for(int i = oldUnlocked; i < unlocked; i++)
        {
            if (!options[i].isActiveAndEnabled) break;

            options[i].anim.SetTrigger("unlock");
            yield return new WaitForSeconds(1);
            //options[i].Unlock();
            //options[i].anim.SetBool("unlocked", true);
            PlayerPrefs.SetInt("Old Unlocked", ++oldUnlocked);
        }
    }

    public void Clear()
    {
        for (int i = 1; i < options.Length; i++) options[i].Lock();
    }

    [System.Serializable]
    public class LevelData
    {
        public int level;
        public Sprite lockedIcon;
        public Sprite icon;
    }
}
