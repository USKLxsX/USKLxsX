using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer0Manager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayerInfoManager.Instance.infos.SetActive(false);
        PlayerInfoManager.Instance.skilltips.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
