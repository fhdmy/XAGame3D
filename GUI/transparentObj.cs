using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAGame
{
    public class transparentObj : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  //摄像机需要设置MainCamera的Tag这里才能找到
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 200))
            {
                transform.position = hitInfo.point+new Vector3(0.1f,0,0.6f);
            }
        }
    }
}

