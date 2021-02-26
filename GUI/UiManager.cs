using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAGame
{
    public class UiManager : MonoBehaviour
    {
        public GameObject[] addBtnWrapper;
        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void FinishRound()
        {
            GameObject.Find("GameManager").GetComponent<GameManager>().FinishRound();
        }

        public void PrepareToAddUnit()
        {
            if (!addBtnWrapper[0].activeSelf)
            {
                addBtnWrapper[2].SetActive(false);
                addBtnWrapper[1].SetActive(false);
                addBtnWrapper[0].SetActive(true);
            }
            else
            {
                addBtnWrapper[0].SetActive(false);
            }
        }

        public void PrepareToAddBuilding()
        {
            if (!addBtnWrapper[1].activeSelf)
            {
                addBtnWrapper[2].SetActive(false);
                addBtnWrapper[0].SetActive(false);
                addBtnWrapper[1].SetActive(true);
            }
            else
            {
                addBtnWrapper[1].SetActive(false);
            }
        }
    }
}

