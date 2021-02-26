using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAGame
{
    public enum GameStage
    {
        decision,
        executionMove,
        executionAttack //can attack
    }
    public class GameManager : MonoBehaviour
    {
        public int round;
        public GameStage gameStage;
        public int gameTimer;
        private float t;
        public List<Player> player;
        private int decisionTime;
        private int executionTime;
        // Start is called before the first frame update
        void Start()
        {
            round = 1;
            gameStage = GameStage.decision;
            decisionTime = 15;
            executionTime = 8;
            gameTimer = decisionTime;
            t = 0;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            t += Time.deltaTime;
            if (t >= 1)
            {
                t = 0;
                if (gameTimer > 0)
                {
                    gameTimer -= 1;
                    GameObject.FindGameObjectWithTag("TimerText").GetComponent<Text>().text = gameTimer.ToString();
                    if(gameStage == GameStage.executionAttack)
                    {
                        //提前结束执行回合
                        bool flag = true;
                        foreach(Player pl in player)
                        {
                            foreach (GameObject u in pl.unit)
                            {
                                if (u.GetComponent<Unit>().HasPath())
                                    flag = false;
                                if(u.GetComponent<Unit>().isAttacking)
                                    flag = false;
                                //Debug.Log("path: " + u.GetComponent<Unit>().HasPath());
                                //Debug.Log("attack: " + u.GetComponent<Unit>().isAttacking);
                            }
                        }
                        if (flag)
                            NewRound();
                    }
                    //进入可进攻阶段
                    if (gameStage == GameStage.executionMove)
                    {
                        if (gameTimer <= executionTime - 1)
                        {
                            gameStage = GameStage.executionAttack;
                        }
                    }
                }
                else
                {
                    if (gameStage == GameStage.decision)
                    {
                        FinishRound();
                    }
                    else
                    {
                        NewRound();
                    }
                }
            }
            if (gameStage == GameStage.executionMove)
            {
                ExecutionStage();
            }
        }

        //finish decision stage
        public void FinishRound()
        {
            if(gameStage == GameStage.decision)
            {
                t = 0;
                gameTimer = executionTime;
                gameStage = GameStage.executionMove;
                GameObject.FindGameObjectWithTag("TimerText").GetComponent<Text>().text = gameTimer.ToString();
            }
        }

        private void ExecutionStage()
        {
            foreach(Player pl in player)
            {
                foreach(GameObject u in pl.unit)
                {
                    u.GetComponent<Unit>().SetMovedUI(false);
                    u.GetComponent<Unit>().SetMoveToTarget();
                }
            }
        }

        private void NewRound()
        {
            foreach (Player pl in player)
            {
                foreach (GameObject u in pl.unit)
                {
                    u.GetComponent<Unit>().FlashState();
                }
            }

            round++;
            gameTimer = decisionTime;
            //计算加钱
            foreach(Player pl in player)
            {
                pl.NewRound();
            }
            GameObject.FindGameObjectWithTag("TimerText").GetComponent<Text>().text = gameTimer.ToString();
            gameStage = GameStage.decision;
            GameObject.FindGameObjectWithTag("RoundText").GetComponent<Text>().text = "第" + round.ToString() + "回合";
        }
    }
}

