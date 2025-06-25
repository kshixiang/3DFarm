using UnityEngine;

/// <summary>
/// 一个简单的脚本，允许挂载此脚本的GameObject在按下Q或E键时，
/// 围绕世界的Y轴进行持续的自旋转。
/// </summary>
public class SelfRotator : MonoBehaviour
{
    [Tooltip("物体每秒旋转的角度")]
    public float rotationSpeed = 90.0f; // 每秒旋转90度

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q))
        {
            TileManager.Instance.ClearAll();
        }

        // 检查是否按下了 'E' 键
        if (Input.GetKey(KeyCode.E))
        {
            // 如果按下E，则围绕世界的Y轴（Vector3.up）进行正向旋转。
            // 乘以 Time.deltaTime 可以确保旋转速度是平滑的，并且与帧率无关。
            // Space.World 参数确保物体总是围绕垂直于地面的轴旋转，即使物体自身倾斜了。
            transform.Rotate(Vector3.up*rotationSpeed * Time.deltaTime);
        }
        // 检查是否按下了 'Q' 键
        else if (Input.GetKey(KeyCode.Q))
        {
            // 如果按下Q，则进行反向旋转。
            transform.Rotate(Vector3.up*-rotationSpeed * Time.deltaTime);
        }
        
        if (Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q) )
        {
            TileManager.Instance.Rotation();
        }
        
    }
}