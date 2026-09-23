using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//[System.Serializable]
//public class EventVector3 : UnityEvent<Vector3> { }
public class MouseManager : Singleton<MouseManager>
{
    public Texture2D point, doorway, attack, target, arrow;

    RaycastHit hitInfo;

    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> OnEnemyClicked;

    int mouseRaycastMask; //用于存储射线检测的层遮罩，以便在射线检测时只检测特定的层，从而在点击玩家时穿过玩家到达敌人

    //也就是说，MouseManager类继承自Singleton<MouseManager>，并且在Awake方法中调用了base.Awake()，确保了单例模式的正确初始化。然后，在Update方法中，它会不断检查鼠标的位置，并根据鼠标悬停的对象类型来切换鼠标指针的贴图。同时，它还监听鼠标点击事件，如果点击的是地面或敌人，就会触发相应的事件。
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this); //确保在场景切换时不会销毁MouseManager实例

        //设置鼠标射线检测的层遮罩，排除玩家层
        mouseRaycastMask = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        SetCusorTexture();
        MouseControl();
    }

    void SetCusorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, mouseRaycastMask))
        {
            // 切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(arrow, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }
        }
    }

    //检测鼠标点击事件，如果点击的是地面或敌人或石头，就触发相应的事件。
    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                OnMouseClicked?.Invoke(hitInfo.point);
            }
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Portal"))
            {
                OnMouseClicked?.Invoke(hitInfo.point);
            }
        }
    }
}
