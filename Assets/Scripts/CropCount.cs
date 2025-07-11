using TMPro;
using UnityEngine;

public class CropCount : MonoBehaviour
{
    public static CropCount Instance;

    public int Count;

    public TextMeshProUGUI m_text;

    private void Awake()
    {
        Instance = this;
    }


    // Start is called before the first frame update
    private void Start()
    {
        m_text.text = Count.ToString();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    /// <summary>
    ///     更新数量
    /// </summary>
    /// <param name="crop"></param>
    public void UpdateCount(Crop crop)
    {
        Count += crop.Value;
        m_text.text = Count.ToString();
    }
}