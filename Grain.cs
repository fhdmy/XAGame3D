using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAGame
{
    public class Grain : MonoBehaviour
    {
        private int resources;
        // Start is called before the first frame update
        void Start()
        {
            resources = 1500;
        }

        // Update is called once per frame
        void FixedUpdate()
        {

        }

        public int GetResources()
        {
            return resources;
        }

        public void AddResources(int e)
        {
            resources += e;
        }

        public void ReduceResources(int e)
        {
            resources -= e;
        }

    }
}

