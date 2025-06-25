using UnityEngine;

public class Crop : MonoBehaviour
{
    public PlaceableType cropType; // 在Prefab的Inspector中设置为例如 "Wheat"

    public enum GrowthState { Seedling, Mature }
    private GrowthState currentState;

    [Header("生长设置")]
    [Tooltip("生长到成熟所需的时间（秒）")]
    public float growthTime = 20f;
    private float timer;

    [Header("模型设置")]
    public GameObject seedlingModel; // 初始状态的模型
    public GameObject matureModel;   // 成熟状态的模型

    /// <summary>
    /// 价格
    /// </summary>
    public int Value = 0;
    
    private Tile3D parentTile;

    void Start()
    {
        // 初始状态为幼苗
        SetState(GrowthState.Seedling);
    }

    void Update()
    {
        // 如果还未成熟，则计时
        if (currentState == GrowthState.Seedling)
        {
            timer += Time.deltaTime;
            if (timer >= growthTime)
            {
                // 时间到，变为成熟状态
                SetState(GrowthState.Mature);
            }
        }
    }

    private void SetState(GrowthState newState)
    {
        currentState = newState;
        timer = 0f;

        // 根据状态切换显示的GameObject
        if (seedlingModel != null) seedlingModel.SetActive(currentState == GrowthState.Seedling);
        if (matureModel != null) matureModel.SetActive(currentState == GrowthState.Mature);
    }
    
    // 设置该作物所在的父地块
    public void SetParentTile(Tile3D tile)
    {
        parentTile = tile;
    }

    // 鼠标点击事件，用于收割
    private void OnMouseDown()
    {
        // 只有成熟的农作物可以被收割
        if (currentState == GrowthState.Mature)
        {
            Harvest();
        }
        else
        {
            Debug.Log("农作物还未成熟，不能收割！");
        }
    }

    /// <summary>
    /// 收割作物
    /// </summary>
    private void Harvest()
    {
        Debug.Log($"收割了 {cropType}!");

        // 通知父地块，它上面的作物已经被移除
        if (parentTile != null)
        {
            parentTile.ClearPlantedCrop();
        }
        
        CropCount.Instance.UpdateCount(this);
        
        // （可选）在这里可以增加玩家的资源，例如返还一个种子
        // ResourceManager.Instance.ReturnItem(cropType);

        // 销毁作物对象
        Destroy(gameObject);
    }
}