using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace XAGame
{
    public class Unit : MonoBehaviour
    {
        private NavMeshAgent nav;
        private Animator ani;
        private bool isSelected;
        public Player player;
        public GameObject destPrefab;
        public GameObject destObj;
        private Transform destParent;
        private List<GameObject> viewableObjs;
        private GameObject targetObj;
        private bool hasAttacked;
        public string unitType;
        public GameObject grid;
        public List<GameObject> aroundGrids;//detect which grid the unit is on,and aroundGrids are viewable areas
        public int maxHp;
        public int presentHp;
        public int attack;
        public int armor;
        public float attackDistance;
        public float viewDistance;
        public float speed;
        public bool isAttacking;
        public int cost;
        public Sprite avatar;
        private Vector3 targetPos;
        public Material[] materials;
        NavMeshObstacle obstacle;
        public List<GameObject> addBuildingGrids;
        // Start is called before the first frame update
        void Start()
        {
            nav = gameObject.transform.GetComponent<NavMeshAgent>();
            ani= gameObject.transform.GetComponent<Animator>();
            destParent = GameObject.Find("DestUnits").transform;
            isSelected = false;
            if(!player)
                player = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>();
            destParent = GameObject.FindGameObjectWithTag("DestUnits").transform;
            viewableObjs = new List<GameObject>();
            hasAttacked = false;
            isAttacking = false;
            viewDistance = gameObject.GetComponents<CapsuleCollider>()[1].radius;
            speed = gameObject.GetComponent<NavMeshAgent>().speed;
            aroundGrids = new List<GameObject>();
            obstacle = GetComponent<NavMeshObstacle>();
            targetPos = transform.position;
            addBuildingGrids = new List<GameObject>();
        }

        // Update is called once per frame
        void Update()
        {
            //detect which grid the unit is on
            if (grid != null)
            {
                float minGridDis = float.PositiveInfinity;
                GameObject minGrid=null;
                foreach(GameObject ag in aroundGrids)
                {
                    if (Vector3.Distance(transform.position, ag.transform.position) < minGridDis)
                    {
                        minGridDis = Vector3.Distance(transform.position, ag.transform.position);
                        minGrid = ag;
                    }
                }
                ExchangeGrid(minGrid);
            }

            //attack
            if (GameObject.Find("GameManager").GetComponent<GameManager>().gameStage == GameStage.executionAttack ||
                GameObject.Find("GameManager").GetComponent<GameManager>().gameStage == GameStage.executionMove)
            {
                AnimatorStateInfo aniInfo = ani.GetCurrentAnimatorStateInfo(0);
                if(aniInfo.IsName("Damage") && aniInfo.normalizedTime > 0.95f)
                {
                    if (nav.enabled && nav != null)
                    {
                        nav.isStopped = false;
                        nav.SetDestination(targetPos);
                    }
                }
                else if (aniInfo.IsName("Attack") && isAttacking)
                {
                    //give damage
                    if(aniInfo.normalizedTime > 0.3f && !hasAttacked)
                    {
                        if (targetObj != null)
                        {
                            if(targetObj.tag=="Unit")
                            {
                                Unit targetUnit = targetObj.GetComponent<Unit>();
                                targetUnit.Damage(attack);
                                hasAttacked = true;
                            }
                            else if(targetObj.tag == "Building")
                            {
                                Building targetBuilding = targetObj.GetComponent<Building>();
                                targetBuilding.Damage(attack);
                                hasAttacked = true;
                            }
                        }
                    }
                    //finish attack
                    if (aniInfo.normalizedTime > 0.95f)
                    {
                        isAttacking = false;
                        if(nav.enabled && nav != null)
                        {
                            nav.isStopped = false;
                            nav.SetDestination(targetPos);
                            //nav.destination = targetPos;
                        }
                    }
                }
                if (nav.enabled && nav != null)
                {
                    if (!nav.hasPath || isAttacking)
                    {
                        ani.SetBool("Walk", false);
                    }
                    else if (nav.hasPath && !isAttacking)
                    {
                        ani.SetBool("Walk", true);
                    }
                }
                
                if (nav.enabled && Vector3.Distance(transform.position, targetPos) < 0.1f && !nav.hasPath)
                {
                    ChangeNavMode(true); 
                } 
            }
            
			//Move
            if (isSelected)
            {
                if (Input.GetMouseButtonDown(1) && GameObject.Find("GameManager").GetComponent<GameManager>().gameStage == GameStage.decision)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Terrain")		))
                    {
                        if (destObj)
                        {
                            Destroy(destObj);
                            targetPos = transform.position;
                        }
                        GameObject Instance = Instantiate(destPrefab);
                        Instance.transform.position = hitInfo.point;
                        Instance.transform.SetParent(destParent);
                        Vector3 lookAt = Instance.transform.position - transform.position;
                        transform.LookAt(hitInfo.point);
                        Instance.transform.LookAt(hitInfo.point+ lookAt);
                        destObj = Instance;
                        SetMovedUI(true);
                        //设置木对的
                        targetPos = hitInfo.point;
                    }
                }
            }
        }

        public void IsSelected()
        {
            player.NewSelected(gameObject);
            var outline = gameObject.GetComponent<LineOut>();
            UiManager uiManager=GameObject.FindGameObjectWithTag("FinishRound").GetComponent<UiManager>();
            uiManager.addBtnWrapper[0].SetActive(false);
            uiManager.addBtnWrapper[1].SetActive(false);
            Transform inform = uiManager.addBtnWrapper[2].transform;
            inform.Find("Avatar").GetComponent<Image>().sprite = avatar;
            inform.Find("ObjName").GetComponent<Text>().text = unitType;
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

        public void SetMoveToTarget()
        {
            if(destObj!=null)
                Destroy(destObj);
            if (nav != null && nav.enabled)
            {
                nav.SetDestination(targetPos);
                //nav.destination = targetPos;
            }
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (player.playerType != GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>().playerType && !player.GetViewableArea().Contains(gameObject))
                return;
            IsSelected();
        }

        private void OnTriggerEnter(Collider other)
        {
			if (other.gameObject.tag == "Unit" || other.gameObject.tag == "Building")
			{
				viewableObjs.Add(other.gameObject);
			}
			else if (other.gameObject.tag == "Terrain")
			{
				aroundGrids.Add(other.gameObject);
				if (Vector3.Distance(transform.position, other.gameObject.transform.position) <= 2.5f
					&& (other.gameObject.GetComponent<Grid>().gridType == "Grass"
					|| other.gameObject.GetComponent<Grid>().gridType == "Plain"
					|| other.gameObject.GetComponent<Grid>().gridType == "Rock1"))
                {
                    addBuildingGrids.Add(other.gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Unit" || other.gameObject.tag == "Building")
            {
                viewableObjs.Remove(other.gameObject);
            }
            else if (other.gameObject.tag == "Terrain")
            {
                aroundGrids.Remove(other.gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {            
            float minDis=float.PositiveInfinity;
            foreach(GameObject gm in viewableObjs)
            {
                if (gm != null)
                {
                    if (gm.tag == "Unit")
                    {
                        if (gm.GetComponent<Unit>().player.playerType == player.playerType)
                            continue;
                    }
                    else if (gm.tag == "Building")
                    {
                        if (gm.GetComponent<Building>().player.playerType == player.playerType)
                            continue;
                    }
                    float tempDis=Vector3.Distance(transform.position, gm.transform.position);
                    if (tempDis < minDis && tempDis<=viewDistance)
                    {
                        minDis = tempDis;
                        targetObj = gm;
                    }
                }
               
            }
            if (targetObj!=null && !hasAttacked && GameObject.Find("GameManager").GetComponent<GameManager>().gameStage == GameStage.executionAttack && !isAttacking)
            {
                //attack
                if(Vector3.Distance(transform.position, targetObj.transform.position) < attackDistance)
                {
                    isAttacking = true;
                    if(nav.enabled && nav != null)
                        nav.isStopped = true;
                    ani.SetBool("Walk",false);
                    transform.LookAt(targetObj.transform.position);
                    ani.SetTrigger("Attack");
                }
                //move
                else
                {
                    if(nav != null && nav.enabled)
                    {
                        nav.SetDestination(targetObj.transform.position);
                        //nav.destination = targetObj.transform.position;
                    }
                }
            }

            //解决两个单位目的地相同碰撞
            if (nav != null)
            {
                if(nav.hasPath && !isAttacking && nav.remainingDistance<1f && other.tag == "Unit" && Vector3.Distance(transform.position,other.transform.position)<1f)
                {
                    if (Vector3.Distance(other.gameObject.GetComponent<Unit>().targetPos, targetPos) < 1f)
                    {
                        StopMove();
                        other.gameObject.GetComponent<Unit>().StopMove();
                    }
                }
            }
        }

        private void ExchangeGrid(GameObject minGrid)
        {
            if (!grid.Equals(minGrid) && minGrid!=null)
            {
                grid.GetComponent<Grid>().gridGameObjects.Remove(gameObject);
                grid = minGrid;
                grid.GetComponent<Grid>().gridGameObjects.Add(gameObject);
            }
        }

        public void FlashState()
        {
            StopMove();
            targetPos = transform.position;
            hasAttacked = false;
            isAttacking = false;
            ChangeNavMode(false);

            if (unitType == "Peasant")
            {
                //get add-building-grids
                for(int i= addBuildingGrids.Count-1; i>=0;i--)
                {
                    if (Vector3.Distance(transform.position, addBuildingGrids[i].transform.position) > 2.5f)
                    {
                        addBuildingGrids.Remove(addBuildingGrids[i]);
                    }
                }
                foreach (GameObject g in aroundGrids)
                {
                    if (!addBuildingGrids.Contains(g))
                    {
                        if (Vector3.Distance(transform.position, g.transform.position) <= 2.5f
							&&(g.GetComponent<Grid>().gridType == "Grass"
							|| g.GetComponent<Grid>().gridType == "Plain"
							|| g.GetComponent<Grid>().gridType == "Rock1"))
                        {
                            addBuildingGrids.Add(g);
                        }
                    }
                }
            }
        }

        public void StopMove()
        {
            if (nav != null && nav.enabled)
            {
                //nav.destination = transform.position;
                nav.SetDestination(transform.position);
            }
            ani.SetBool("Walk", false);
        }

        public void SetMovedUI(bool flag)
        {
            //flag=true:设置为已移动
            //设置ui为已选择移动
            MeshRenderer[] mrs = gameObject.GetComponentsInChildren<MeshRenderer>();
            SkinnedMeshRenderer[] smrs = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < mrs.Length; i++)
            {
                if(flag)
                    mrs[i].material = materials[1];
                else
                    mrs[i].material = materials[0];
            }
            for (int i = 0; i < smrs.Length; i++)
            {
                if(flag)
                    smrs[i].material = materials[1];
                else
                    smrs[i].material = materials[0];
            }
        }
        public void Dead()
        {
            player.GetComponent<Player>().unit.Remove(gameObject);
            grid.GetComponent<Grid>().gridGameObjects.Remove(gameObject);
            int t = Random.Range(1, 2);
            ani.SetTrigger("Die"+t.ToString());
            Invoke("DestroySelf",3f);
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }

        public bool HasPath()
        {
            if (!nav.enabled || nav == null)
                return false;
            return nav.hasPath;
        }

        public void ChangeNavMode(bool toObstacle)
        {
            if (nav.enabled && toObstacle)
            {
                nav.enabled = false;
                Invoke("ObstacleEnable", 0.1f);
            }
            else if(!nav.enabled && !toObstacle)
            {
                obstacle.enabled = false;
                Invoke("NavEnable", 0.1f);
            }
        }

        private void NavEnable()
        {
            if(!obstacle.enabled)
                nav.enabled = true;
        }

        private void ObstacleEnable()
        {
            if(!nav.enabled)
                obstacle.enabled = true;
        }

        public void Damage(int a)
        {
            //防止死前受伤
            if (presentHp <= 0)
                return;
            presentHp =presentHp - (a - armor);
            FloatingText.instance.InitializeScriptableText(0, transform.position, "-"+ (a - armor).ToString());
            //interrupt
            AnimatorStateInfo aniInfo = ani.GetCurrentAnimatorStateInfo(0);
            isAttacking = false;
            if(!aniInfo.IsName("Damage"))
                ani.SetTrigger("Damage");
            if (presentHp <= 0)
            {
                Dead();
            }
        }

        public List<GameObject> GetAddBuildingGrids(string type)
        {
            List<GameObject> temp = new List<GameObject>();
            if (type == "Farm")
            {
                foreach (GameObject g in addBuildingGrids)
                {
                    if (g.GetComponent<Grid>().hasGrain)
                    {
                        temp.Add(g);
                    }
                }
                return temp;
            }
            //else
            foreach (GameObject g in addBuildingGrids)
            {
                if (!g.GetComponent<Grid>().hasGrain)
                {
                    temp.Add(g);
                }
            }
			//建筑前置条件
			if (type == "Archery")
			{
				bool ArcheryFlag = false;
				foreach (GameObject bu in player.building)
				{
					if (bu.GetComponent<Building>().buildingType == "Barracks")
					{
						ArcheryFlag = true;
						break;
					}
				}
				if (ArcheryFlag)
					return temp;
				else
					return new List<GameObject>();
			}
			else if (type == "Tower")
			{
				bool ArcheryFlag = false;
				foreach (GameObject bu in player.building)
				{
					if (bu.GetComponent<Building>().buildingType == "Barracks")
					{
						ArcheryFlag = true;
						break;
					}
				}
				if (ArcheryFlag)
					return temp;
				else
					return new List<GameObject>();
			}
			else if (type == "MageTower")
			{
				bool ArcheryFlag = false;
				foreach (GameObject bu in player.building)
				{
					if (bu.GetComponent<Building>().buildingType == "Barracks")
					{
						ArcheryFlag = true;
						break;
					}
				}
				if (ArcheryFlag)
					return temp;
				else
					return new List<GameObject>();
			}
			else if (type == "Stables")
			{
				bool ArcheryFlag = false;
				foreach (GameObject bu in player.building)
				{
					if (bu.GetComponent<Building>().buildingType == "MageTower")
					{
						ArcheryFlag = true;
						break;
					}
				}
				if (ArcheryFlag)
					return temp;
				else
					return new List<GameObject>();
			}
			return temp;
        }
    }
}

