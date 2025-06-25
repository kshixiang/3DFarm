using System;
using System.Collections.Generic;
using UnityEngine;

// 定义所有可放置/创建的物品类型
public enum PlaceableType
{
    Land,
    River,
    Sand,

    // --- 新增农作物类型 ---
    Corn,
    Wheat
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    // 【新增】定义一个公开事件。当物品数量改变时，关心此事件的任何脚本都会收到通知。
    // Action<PlaceableType, int> 表示这个事件会传递两个参数：发生改变的物品类型，和该物品的最新数量。
    public event Action<PlaceableType, int> OnItemCountChanged;

    // 在Inspector中设置每种物品的初始数量
    [Serializable]
    public struct ItemStock
    {
        public PlaceableType type;
        public int initialCount;
    }

    public List<ItemStock> initialStocks;
    private readonly Dictionary<PlaceableType, int> itemCounts = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeStock();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 初始化库存
    private void InitializeStock()
    {
        foreach (var stock in initialStocks)
            if (!itemCounts.ContainsKey(stock.type))
                itemCounts.Add(stock.type, stock.initialCount);
    }

    /// <summary>
    ///     检查是否有足够的物品可供创建
    /// </summary>
    public bool CanCreate(PlaceableType type)
    {
        return itemCounts.ContainsKey(type) && itemCounts[type] > 0;
    }

    /// <summary>
    ///     使用/消耗一个物品
    /// </summary>
    public void UseItem(PlaceableType type)
    {
        if (CanCreate(type))
        {
            itemCounts[type]--;
            Debug.Log($"Used {type}. Remaining: {itemCounts[type]}");

            // 【修改】触发（或广播）事件，并把更新后的类型和数量传递出去
            // ?.Invoke 是一个安全调用，如果没有任何脚本订阅这个事件，它不会报错。
            OnItemCountChanged?.Invoke(type, itemCounts[type]);
        }
    }

    /// <summary>
    ///     回收/返还一个物品
    /// </summary>
    public void ReturnItem(PlaceableType type)
    {
        if (itemCounts.ContainsKey(type))
        {
            itemCounts[type]++;
            Debug.Log($"Returned {type}. Remaining: {itemCounts[type]}");

            // 【修改】同样，在返还物品后也触发事件
            OnItemCountChanged?.Invoke(type, itemCounts[type]);
        }
    }

    /// <summary>
    ///     获取物品的剩余数量 (可用于UI显示)
    /// </summary>
    public int GetItemCount(PlaceableType type)
    {
        if (itemCounts.ContainsKey(type)) return itemCounts[type];
        return 0;
    }
}