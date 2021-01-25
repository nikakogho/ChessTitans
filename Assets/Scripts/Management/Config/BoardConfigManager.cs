using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Chess.UI;

namespace Chess.Config
{
    public class BoardConfigManager : MonoBehaviour
    {
        public GameObject prefab;
        public float cameraMoveSpeed = 10;
        public float deltaTime = 0.1f;

        public float boardDistance = 8;

        BoardConfigOptionUI[] options;

        public static BoardConfigManager instance;

        IEnumerator lookingRoutine;

        int index = 0;
        LoadManager loadManager;
        MenuManager menuManager;

        public BoardConfig Chosen => loadManager.boardConfigs[index];

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            loadManager = LoadManager.instance;
            menuManager = MenuManager.instance;
            InitBoards();
        }

        void InitBoards()
        {
            //Debug.Log("Init boards 0");
            options = new BoardConfigOptionUI[loadManager.boardConfigs.Length];

            //Debug.Log("Init boards 1");
            for (int i = 0; i < options.Length; i++)
            {
                Vector3 pos = new Vector3(i * boardDistance, 0, 0);
                Quaternion rot = Quaternion.identity;

                var obj = Instantiate(prefab, pos, rot, transform);
                var option = obj.GetComponent<BoardConfigOptionUI>();

                option.Init(loadManager.boardConfigs[i]);
                options[i] = option;
            }
            //Debug.Log("Init boards done");
        }

        public void Next()
        {
            if(++index == options.Length) index = 0;
            Select(index);
        }

        public void Previous()
        {
            if (--index < 0) index = options.Length - 1;
            Select(index);
        }

        void Select(int index)
        {
            this.index = index;
            // may do update loaded game UIs
            menuManager.UI.Data.chosenBoardText.text = Chosen.name + " Board";
            options[index].Shuffle();
            LookAtCurrentBoard();
        }

        IEnumerator LookingRoutine(Transform targetSpot)
        {
            var cam = Camera.main.transform;

            Vector3 dir = targetSpot.transform.position - cam.position;
            float dist = dir.magnitude;

            float stepMove = cameraMoveSpeed * deltaTime;
            int steps = Mathf.CeilToInt(dist / stepMove);

            var desiredRotation = Quaternion.LookRotation((options[index].transform.position - targetSpot.position).normalized);

            for(int i = 0; i < steps; i++)
            {
                yield return new WaitForSeconds(deltaTime);

                float fraction = (float)i / steps;
                cam.position = Vector3.Slerp(cam.position, targetSpot.position, fraction);
                cam.rotation = Quaternion.Slerp(cam.rotation, desiredRotation, fraction);
            }

            cam.position = targetSpot.position;
            cam.rotation = desiredRotation;
        }

        void LookAtCurrentBoard()
        {
            if (lookingRoutine != null) StopCoroutine(lookingRoutine);

            lookingRoutine = LookingRoutine(options[index].cameraSpot);
            StartCoroutine(lookingRoutine);
        }
    }
}