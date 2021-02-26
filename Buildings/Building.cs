using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace XAGame
{
    public class Building : MonoBehaviour
    {
        private int len;
        private float times;
        public GameObject[] c;
        public int[] buildSessions;
        public int[] addHp;//add hp when building
        public int presentSession;
        private bool isSelected;
        public Player player;
        public string buildingType;
        public GameObject grid;
        public List<GameObject> addUnitGrids;
        public int cost;
        public int maxHp;
        public int presentHp;
        public int armor;
        public int attack;
        public float attackDistance;
        public float viewDistance;
        public Sprite avatar;
        public List<GameObject> aroundGrids;
        public List<string> relativeUnits;//units which can add around
        public GameObject grain;
        public int perRoundAddCount;
        private int presentRoundAddCount;
        // Start is called before the first frame update
        void Start()
        {
            len = transform.childCount;
            times = 0;
            isSelected = false;
            player = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>();
            addUnitGrids = new List<GameObject>();
            aroundGrids = new List<GameObject>();
            presentRoundAddCount = perRoundAddCount;
        }

        // Update is called once per frame
        void Update()
        {
            if (presentSession <= buildSessions.Length)
            {
                if (times <= buildSessions[buildSessions.Length - 1])
                {
                    if (times <= buildSessions[presentSession])
                    {
                        for (int i = 0; i < len; i++)
                        {
                            if (i == presentSession)
                                c[i].SetActive(true);
                            else
                                c[i].SetActive(false);
                        }
                    }
                    else if (times > buildSessions[presentSession])
                    {
                        maxHp += addHp[presentSession];
                        presentHp+=addHp[presentSession];
                        presentSession++;
                    }
                }
            }
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (player.playerType != GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>().playerType && !player.GetViewableArea().Contains(gameObject))
                return;
            IsSelected();
        }

        public void IsSelected()
        {
            player.NewSelected(gameObject);
            var outline = gameObject.GetComponent<LineOut>();
            UiManager uiManager = GameObject.FindGameObjectWithTag("FinishRound").GetComponent<UiManager>();
            uiManager.addBtnWrapper[0].SetActive(false);
            uiManager.addBtnWrapper[1].SetActive(false);
            Transform inform = uiManager.addBtnWrapper[2].transform;
            inform.Find("Avatar").GetComponent<Image>().sprite = avatar;
            inform.Find("ObjName").GetComponent<Text>().text = buildingType;
            inform.Find("Information").Find("Hp").GetChild(1).GetComponent<Text>().text = presentHp.ToString() + "/" + maxHp.ToString();
            inform.Find("Information").Find("Attack").GetChild(1).GetComponent<Text>().text = attack.ToString();
            inform.Find("Information").Find("Armor").GetChild(1).GetComponent<Text>().text = armor.ToString();
            uiManager.addBtnWrapper[2].SetActive(true);
            outline.enabled = true;
            isSelected = true;
        }

        public void ExitSelected()
        {
            var outline = gameObject.GetComponent<LineOut>();
            UiManager uiManager = GameObject.FindGameObjectWithTag("FinishRound").GetComponent<UiManager>();
            uiManager.addBtnWrapper[2].SetActive(false);
            outline.enabled = false;
            isSelected = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag=="Terrain")
            {
                //viewablegrids
                aroundGrids.Add(other.gameObject);
                //addgrids
                if(!other.gameObject.Equals(grid) && Vector3.Distance(transform.position, other.gameObject.transform.position) <= 3
					&& (other.gameObject.GetComponent<Grid>().gridType == "Grass"
					|| other.gameObject.GetComponent<Grid>().gridType == "Plain"
					|| other.gameObject.GetComponent<Grid>().gridType == "Rock1"))
                {
                    addUnitGrids.Add(other.gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            aroundGrids.Remove(other.gameObject);
        }

        public List<GameObject> GetAddUnitGrids(string type)
        {
            if (relativeUnits.Contains(type) && presentRoundAddCount!=0)
            {
                return addUnitGrids;
            }
            return new List<GameObject>();
        }

        public void FlashState()
        {
            times+=1;
            presentRoundAddCount = perRoundAddCount;
        }

        public void Damage(int a)
        {
            presentHp = presentHp - (a - armor);
            FloatingText.instance.InitializeScriptableText(0, transform.position, "-" + (a - armor).ToString());
            if (presentHp <= 0)
            {
                Dead();
            }
        }

        private void Dead()
        {
            player.GetComponent<Player>().building.Remove(gameObject);
            grid.GetComponent<Grid>().gridGameObjects.Remove(gameObject);
            Destroy(gameObject);
        }

        public int GetPresentRoundAddCount()
        {
            return presentRoundAddCount;
        }

        public void ReducePresentRoundAddCount(int e)
        {
            presentRoundAddCount-=e;
        }
    }
}

