using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
    public class AddBuildBtn : MonoBehaviour
    {
        private GameObject tObj;
        public GameObject[] relativeObj;
        public Transform parent;
        private bool isSelected;
        private Player player;
        public GameObject addHexagonModel;
        public List<GameObject> addHexagons;
		public Transform addParent;
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
                        if (hitInfo.collider.gameObject.tag == "Terrain" 
							&& player.GetViewableArea().Contains(hitInfo.collider.gameObject) 
							&& player.GetAddBuildingGrids(gameObject.name).Contains(hitInfo.collider.gameObject))
                        {
                            AddBuild(hitInfo.collider.gameObject, player.playerType);
                        }
                    }
                }
            }
        }

        public void IsSelected()
        {
            if (GameObject.Find("GameManager").GetComponent<GameManager>().gameStage == GameStage.executionAttack ||
                GameObject.Find("GameManager").GetComponent<GameManager>().gameStage == GameStage.executionMove)
                return;
            player.NewSelected(gameObject);
            foreach (GameObject gd in player.GetAddBuildingGrids(gameObject.name))
            {
                GameObject Instance = Instantiate(addHexagonModel);
                Instance.tag = "AbleToAdd";
                Instance.transform.position = gd.transform.position + new Vector3(0, 0.6f, 0);
				Instance.transform.parent = addParent;
				addHexagons.Add(Instance);
            }
            isSelected = true;
        }
        public void ExitSelected()
        {
            //Destroy(tObj);
            foreach (GameObject ah in addHexagons)
            {
                Destroy(ah);
            }
            addHexagons.Clear();
            isSelected = false;
        }


        public void AddBuild(GameObject grid, PlayerType playerType)
        {
            Vector3 targetPos = grid.transform.position;
            int n;
            if (playerType == PlayerType.playBlue)
                n = 0;
            else n = 1;
            if (player.gold < relativeObj[n].GetComponent<Building>().cost)
            {
                Debug.Log("money is not enough");
                return;
            }
            GameObject Instance = Instantiate(relativeObj[n]);
            Instance.tag = "Building";
            Instance.transform.parent = parent;
            Instance.transform.position =new Vector3(targetPos.x,targetPos.y+0.9f,targetPos.z);
            Instance.GetComponent<Building>().buildingType = gameObject.name;
            Instance.GetComponent<Building>().grid = grid;
            grid.GetComponent<Grid>().gridGameObjects.Add(Instance);
            if (gameObject.name == "Farm")
            {
                GameObject grain = null;
                foreach(GameObject fg in grid.GetComponent<Grid>().gridGameObjects)
                {
                    if (fg.tag == "Grain")
                    {
                        grain = fg;
                        break;
                    }
                }
                Instance.GetComponent<Building>().grain= grain;
            }
            player.GetComponent<Player>().building.Add(Instance);
            player.SetGold(player.gold - relativeObj[n].GetComponent<Building>().cost);
        }
    }
}
