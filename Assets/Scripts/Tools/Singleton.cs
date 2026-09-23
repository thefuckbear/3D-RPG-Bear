using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//当你想要创建一个单例类时，可以使用这个泛型单例类。它确保在整个应用程序中只有一个实例存在，并提供全局访问点。
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    
    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<T>();

            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = (T)this; //当前实例赋值给静态变量
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    //当你想要检查单例是否已经初始化时，可以使用这个属性。
    public static bool IsInitialized
    {
        get { return instance != null; }
    }

    //如果你想要在单例被销毁时执行一些清理操作，可以重写这个方法。
    protected virtual void OnDestroy()
    {
        if (ReferenceEquals(instance, this))
        {
            instance = null;
        }
    }
}
