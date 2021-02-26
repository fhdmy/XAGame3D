using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
    public class AddUnitBtn : MonoBehaviour
    {
        private GameObject tObj;
        public GameObject[] relativeObj;
        public Transform parent;
        private bool isSelected;
        private Player player;
        public GameObject addHexagonModel;
        public List<GameObject> addHexagons;
        // Start is called before the first frame update
        void Start()
        {
            isSelected = false;
            player = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>();
            addHexagons = new List<GameObject>();
        }

        // Update is called once per frame
        void Update()
        {
            if (isSelected)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, 200))
                    {
                        if (hitInfo.collider.gameObject.tag == "Terrain" && player.GetAddUnitGrids(gameObject.name).Contains(hitInfo.collider.gameObject))
                        {
                            AddUnit(hitInfo.collider.gameObject, player.playerType);
                            foreach (GameObject ah in addHexagons)
                            {
                                Destroy(ah);
                            }
                            addHexagons.Clear();
                            foreach (GameObject gd in player.GetAddUnitGrids(gameObject.name))
                            {
                                GameObject Instance = Instantiate(addHexagonModel);
                                Instance.tag = "AbleToAdd";
                                Instance.transform.position = gd.transform.position + new Vector3(0, 0.6f, 0);
                                addHexagons.Add(Instance);
                            }
                        }
                    }
                }
            }
        }

        public void IsSelected()
        {
            if (GameObject.Find("GameManager").GetComponent<GameManager>().gameStage == GameStage.executionMove ||
                GameObject.Find("GameManager").GetComponent<GameManager>().gameStage == GameStage.executionAttack)
                return;
            player.NewSelected(gameObject);
            foreach (GameObject gd in player.GetAddUnitGrids(gameObject.name))
            {
                GameObject Instance = Instantiate(addHexagonModel);
                Instance.tag = "AbleToAdd";
                Instance.transform.position = gd.transform.position+new Vector3(0,0.6f,0);
                addHexagons.Add(Instance);
            }
            isSelected = true;
        }

        public void ExitSelected()
        {
            //Destroy(tObj);
            foreach(GameObject ah in addHexagons)
            {
                Destroy(ah);
            }
            addHexagons.Clear();
            isSelected = false;
        }

        public void AddUnit(GameObject grid,PlayerType playerType)
        {
            Vector3 targetPos = grid.transform.position;
            int n;
            if (playerType == PlayerType.playBlue)
                n = 0;
            else n = 1;
            if (player.gold<relativeObj[n].GetComponent<Unit>().cost)
            {
                Debug.Log("money is not enough");
                return;
            }
            GameObject Instance = Instantiate(relativeObj[n]);
            Instance.tag = "Unit";
            Instance.transform.parent = parent;
            Instance.transform.position = targetPos;
            Instance.GetComponent<Unit>().unitType = gameObject.name;
            Instance.GetComponent<Unit>().grid = grid;
            grid.GetComponent<Grid>().gridGameObjects.Add(Instance);
            player.GetComponent<Player>().unit.Add(Instance);
            player.SetGold(player.gold - relativeObj[n].GetComponent<Unit>().cost);
            //building can add -1
            foreach (GameObject bu in player.building)
            {
                if (bu.GetComponent<Building>().presentSession == bu.GetComponent<Building>().buildSessions.Length - 1)
                {
                    if (bu.GetComponent<Building>().GetAddUnitGrids(gameObject.name).Contains(grid)){
                        bu.GetComponent<Building>().ReducePresentRoundAddCount(1);
                        break;
                    }
                }
            }
        }
    }
}

