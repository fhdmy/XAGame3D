using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAGame
{
    public enum PlayerType
    {
        playBlue,
        playRed
    }
    public class Player : MonoBehaviour
    {
        private GameObject selected;
        public PlayerType playerType;
        private float t0;
        private float t1;
        public List<GameObject> building;
        public List<GameObject> unit;
        public int gold;
        public int gem;
        private int perRoundGain;
        private int perGrainGain;

        // Start is called before the first frame update
        void Start()
        {
            unit = new List<GameObject>();
            gold = 100;
            gem = 100;
            perRoundGain = 20;
            perGrainGain =20;
            GameObject.FindGameObjectWithTag("GoldText").GetComponent<Text>().text = gold.ToString();
            GameObject.FindGameObjectWithTag("GemText").GetComponent<Text>().text = gem.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            //update viewableGrids
            GameObject[] allGrids= GameObject.FindGameObjectsWithTag("Terrain");
            List<GameObject> ag = GetViewableArea();
            for (int i=0;i<allGrids.Length;i++)
            {
                if (!ag.Contains(allGrids[i]))
                {
                    //if(allGrids[i].transform.childCount==0)
                    //Debug.Log(allGrids[i].name);
                    allGrids[i].transform.GetChild(0).gameObject.SetActive(true);
                    foreach (GameObject obj in allGrids[i].GetComponent<Grid>().gridGameObjects)
                    {
                        if (!unit.Contains(obj) && !building.Contains(obj))
                        {
                            SkinnedMeshRenderer[] marr = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                            foreach (SkinnedMeshRenderer m in marr)
                            {
                                m.enabled = false;
                            }
                            MeshRenderer[] mrr = obj.GetComponentsInChildren<MeshRenderer>(true);
                            foreach (MeshRenderer m in mrr)
                            {
                                m.enabled = false;
                            }
                        }
                    }
                }
                else
                {
                    allGrids[i].transform.GetChild(0).gameObject.SetActive(false);
                    foreach (GameObject obj in allGrids[i].GetComponent<Grid>().gridGameObjects)
                    {
                        SkinnedMeshRenderer[] marr = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
                        foreach (SkinnedMeshRenderer m in marr)
                        {
                            m.enabled = true;
                        }
                        MeshRenderer[] mrr = obj.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer m in mrr)
                        {
                            m.enabled = true;
                        }
                    }
                }
            }
            //input
            if (Input.GetMouseButtonDown(0))
            {
                t0 = Time.time;
            }
            else if (Input.GetMouseButton(0))
            {
                float tend = Time.time;
                if (tend - t0 > 0.1f)
                {
                    int speed = 30;
                    Camera.main.transform.position -= Camera.main.transform.right * Time.deltaTime * Input.GetAxis("Mouse X") * speed;
                    Camera.main.transform.position -= Camera.main.transform.up * Time.deltaTime * Input.GetAxis("Mouse Y") * speed;
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, 200))
                    {
						Debug.Log(hitInfo.collider.gameObject.name);
						if (hitInfo.collider.gameObject.tag == "Terrain")
                        {
                            NewSelected(null);
                        }
                    }
                }
            }
            //else if (Input.GetMouseButtonDown(1))
            //{
            //    t1 = Time.time;
            //}
            //else if (Input.GetMouseButton(1))
            //{
            //    float tend = Time.time;
            //    if (tend - t1 > 0.1f)
            //    {
            //        var angles = Camera.main.transform.eulerAngles;
            //        float x = angles.y;
            //        float y = angles.x;
            //        float speed = 0.8f;
            //        x += Input.GetAxis("Mouse X") * speed;
            //        y -= Input.GetAxis("Mouse Y") * speed;
            //        if (y > 90)
            //            y = 90;
            //        //返回一个四元数 绕某个轴旋转某个角度
            //        var rotation = Quaternion.Euler(y, x, 0);
            //        Camera.main.transform.rotation = rotation;
            //    }
            //}
            else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                //if(Camera.main.orthographicSize<40)
                //    Camera.main.orthographicSize += 1;
                if (Camera.main.fieldOfView < 40)
                    Camera.main.fieldOfView += 1;
            }
            //滚轮向上  显示全局
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                //if (Camera.main.orthographicSize >2)
                //    Camera.main.orthographicSize -= 1;
                if (Camera.main.fieldOfView >3)
                    Camera.main.fieldOfView -= 1;
            }
        }

        public void NewSelected(GameObject newObj)
        {
            if (selected != null)
            {
                if (selected.tag == "Unit")
                    selected.GetComponent<Unit>().ExitSelected();
                else if(selected.tag == "Building")
                    selected.GetComponent<Building>().ExitSelected();
                else if (selected.tag == "AddUnitBtn")
                    selected.GetComponent<AddUnitBtn>().ExitSelected();
                else if (selected.tag == "AddBuildBtn")
                    selected.GetComponent<AddBuildBtn>().ExitSelected();
            }
            selected = newObj;
        }

        public List<GameObject> GetAddUnitGrids(string type)
        {
            List<GameObject> addgrid = new List<GameObject>();
            foreach (GameObject bu in building)
            {
                if(bu.GetComponent<Building>().presentSession == bu.GetComponent<Building>().buildSessions.Length-1)
                {
                    foreach(GameObject gd in bu.GetComponent<Building>().GetAddUnitGrids(type))
                    {
                        //finally judge whether can add on grid
                        if(!addgrid.Contains(gd) &&
                        (gd.GetComponent<Grid>().gridGameObjects.Count==0 || (gd.GetComponent<Grid>().hasGrain && gd.GetComponent<Grid>().gridGameObjects.Count == 1))) {
                            addgrid.Add(gd);
                        }
                    }
                }
            }
            return addgrid;
        }

        public List<GameObject> GetAddBuildingGrids(string type)
        {
            List<GameObject> addgrid = new List<GameObject>();
            foreach (GameObject u in unit)
            {
                if (u.GetComponent<Unit>().unitType == "Peasant")
                {
                    foreach (GameObject g in u.GetComponent<Unit>().GetAddBuildingGrids(type))
                    {
                        if (!addgrid.Contains(g) &&
                    (g.GetComponent<Grid>().gridGameObjects.Count == 0 || (g.GetComponent<Grid>().hasGrain && g.GetComponent<Grid>().gridGameObjects.Count == 1)))
                        {
                                addgrid.Add(g);
                        }
                    }
                }
            }
            return addgrid;
        }

        public List<GameObject> GetViewableArea()
        {
            List<GameObject> temp = new List<GameObject>();
            foreach(GameObject u in unit)
            {
                foreach(GameObject ag in u.GetComponent<Unit>().aroundGrids)
                {
                    if(!temp.Contains(ag))
                        temp.Add(ag);
                }
            }
            foreach (GameObject b in building)
            {
                if(b.GetComponent<Building>().presentSession == b.GetComponent<Building>().buildSessions.Length -1 )
                {
                    foreach (GameObject bag in b.GetComponent<Building>().aroundGrids)
                    {
                        if (!temp.Contains(bag))
                            temp.Add(bag);
                    }
                }
                else
                {
                    temp.Add(b.GetComponent<Building>().grid);
                }
            }
            return temp;
        }

        public void SetGold(int val)
        {
            gold = val;
            GameObject.FindGameObjectWithTag("GoldText").GetComponent<Text>().text = gold.ToString();
        }

        public void SetGem(int val)
        {
            gem = val;
            GameObject.FindGameObjectWithTag("GemText").GetComponent<Text>().text = gem.ToString();
        }

        public void NewRound()
        {
            gold += perRoundGain;
            //grain gain
            //todo: Farm's resources on ui
            foreach(GameObject bu in building)
            {
                if (bu.GetComponent<Building>().buildingType == "Farm" && bu.GetComponent<Building>().presentSession == bu.GetComponent<Building>().buildSessions.Length -1)
                {
                    GameObject gr = bu.GetComponent<Building>().grain;
                    if (gr == null)
                        continue;
                    int res = gr.GetComponent<Grain>().GetResources();
                    if (res >= perGrainGain)
                    {
                        gr.GetComponent<Grain>().ReduceResources(perGrainGain);
                        gold += perGrainGain;
                    }
                    else
                    {
                        gr.GetComponent<Grain>().ReduceResources(res);
                        Destroy(gr);
                        bu.GetComponent<Building>().grain=null;
                        gold += res;
                    }
                }
            }
            //update ui
            if(gameObject == GameObject.FindGameObjectWithTag("MainCamera"))
                GameObject.FindGameObjectWithTag("GoldText").GetComponent<Text>().text = gold.ToString();
            //new unit state
            foreach (GameObject u in unit)
            {
                u.GetComponent<Unit>().FlashState();
            }
            foreach (GameObject bu in building)
            {
                bu.GetComponent<Building>().FlashState();
            }
        }
    }
}

