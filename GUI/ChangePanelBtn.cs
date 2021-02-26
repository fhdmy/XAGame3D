using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
    public class ChangePanelBtn : MonoBehaviour
    {
        private Player player;
        public GameObject[] AddBtn;
        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void OnMouseDown()
        {
            IsSelected();
        }

        private void IsSelected()
        {
            player.NewSelected(gameObject);
            if (AddBtn[0].active == true)
            {
                AddBtn[0].SetActive(false);
                AddBtn[1].SetActive(true);
            }
            else
            {
                AddBtn[0].SetActive(true);
                AddBtn[1].SetActive(false);
            }
        }
    }
}

