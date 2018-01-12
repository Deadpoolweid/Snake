﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Data;

public class Initialize : MonoBehaviour
{

    public GameObject GrassPrefab;

    // Use this for initialization
    void Start()
    {
        for (int i = (int)borderLeft.transform.position.x + 1; i < borderRight.transform.position.x; i++)
        {
            for (int j = (int)borderBottom.position.y + 1; j < borderTop.transform.position.y; j++)
            {
                Instantiate(GrassPrefab, new Vector3(i, j,10), Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
