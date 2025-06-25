using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIHandler : MonoBehaviour
{
    public static GameUIHandler Instance;
    [SerializeField]
    private GameObject opt;

    public bool Preview
    {
        get;
        set;
    }

    private void Awake()
    {
        Instance = this;
    }
    
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandlerOpt()
    {
        var optActiveSelf = opt.activeSelf;
        opt.SetActive(!optActiveSelf);
    }

    /// <summary>
    /// 预览模式
    /// </summary>
    /// <param name="status"></param>
    public void OnToogle(bool status)
    {
        Debug.Log("OnToogle");
        Preview = status;
    }
}
