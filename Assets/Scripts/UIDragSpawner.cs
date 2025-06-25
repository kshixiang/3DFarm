using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIDragSpawner : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // --- 【修改】使用 PlaceableType 来定义要生成的物品 ---
    public PlaceableType itemToSpawn;
    public GameObject itemPrefab; // 通用预制件引用
    public TextMeshProUGUI m_count;
    private GameObject currentPreviewInstance;
    private float gridSize;
    private Transform gridRoot;
    private Plane dragPlane;

    void Start()
    {
        if (TileManager.Instance != null)
        {
            gridSize = TileManager.Instance.PlanarSize;
            gridRoot = TileManager.Instance.transform;
            dragPlane = new Plane(gridRoot.up, gridRoot.position);
        }
        else
        {
            Debug.LogError("UIDragSpawner无法找到TileManager实例！");
        }

        ShowCount();
        ResourceManager.Instance.OnItemCountChanged += (arg, arg2) =>
        {
            ShowCount();
        };

    }

    private Tile3D m_tile3D;

    public void OnPointerDown(PointerEventData eventData)
    {
        // --- 【修改】创建前检查资源数量 ---
        if (!ResourceManager.Instance.CanCreate(itemToSpawn))
        {
            Debug.Log($"{itemToSpawn} 数量不足，无法创建！");
            return;
        }

        if (itemPrefab == null || gridSize <= 0 || gridRoot == null || GameUIHandler.Instance.Preview) return;

        currentPreviewInstance = Instantiate(itemPrefab);
        currentPreviewInstance.transform.rotation = gridRoot.rotation;
        m_tile3D = currentPreviewInstance.GetComponent<Tile3D>();

        UpdatePreviewPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentPreviewInstance != null)
        {
            UpdatePreviewPosition();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentPreviewInstance == null) return;

        // --- 【修改】根据物品类型执行不同放置逻辑 ---
        bool success = false;

        // 检查物品是否是农作物
        Crop cropComponent = itemPrefab.GetComponent<Crop>();
        if (cropComponent != null)
        {
            // 尝试种植作物
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Tile3D targetTile = hit.collider.GetComponent<Tile3D>();
                if (targetTile != null)
                {
                    // 在目标地块上尝试种植
                    if (targetTile.TryPlantCrop(itemPrefab))
                    {
                        success = true;
                    }
                }
            }
        }
        else // 否则，视为地块
        {
            // 使用地块管理器放置地块
            if (m_tile3D != null && TileManager.Instance.PlaceTile(m_tile3D))
            {
                // 如果放置成功，让 currentPreviewInstance 变为 null，防止它被销毁
                currentPreviewInstance = null;
                success = true;
            }
        }

        // --- 统一处理 ---
        if (success)
        {
            // 如果放置成功，消耗一个资源
            ResourceManager.Instance.UseItem(itemToSpawn);
            ShowCount();
        }

        // 如果 currentPreviewInstance 仍然存在（意味着放置失败或它是作物预览），则销毁它
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
        }

        currentPreviewInstance = null;
    }

    public void ShowCount()
    {
        if (m_count != null)
        {
            m_count.text = ResourceManager.Instance.GetItemCount(itemToSpawn).ToString();    
        }
    }
    
    private void UpdatePreviewPosition()
    {
        // ... 此方法保持不变 ...
        dragPlane.SetNormalAndPosition(gridRoot.up, gridRoot.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPosition = ray.GetPoint(distance);

            Vector3 mouseLocalPosition = gridRoot.InverseTransformPoint(mouseWorldPosition);

            float snappedLocalX = Mathf.Floor(mouseLocalPosition.x / gridSize) * gridSize + (gridSize / 2f);
            float snappedLocalZ = Mathf.Floor(mouseLocalPosition.z / gridSize) * gridSize + (gridSize / 2f);

            Vector3 finalLocalPosition = new Vector3(snappedLocalX, 0.1f, snappedLocalZ);

            currentPreviewInstance.transform.position = gridRoot.TransformPoint(finalLocalPosition);
            currentPreviewInstance.transform.rotation = gridRoot.rotation;
        }
    }
}