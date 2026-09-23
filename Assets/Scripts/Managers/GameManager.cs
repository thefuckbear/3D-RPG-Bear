using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

//GameManager类是一个单例类，用于管理游戏中的全局状态和数据。在这个类中，我们定义了一个CharacterStats类型的变量playerStats，用于存储玩家的角色属性信息。通过RegisterPlayer方法，我们可以将玩家的角色属性注册到GameManager中。
public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;

    private Cinemachine.CinemachineFreeLook followCamera;

    List<IEndGameObserve> endGameObservers = new List<IEndGameObserve>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    public void RegisterPlayer(CharacterStats player)
    {
        playerStats = player;

        //要在切换场景后，重新寻找相机
        followCamera = FindObjectOfType<CinemachineFreeLook>();
        if (followCamera != null)
        {
            followCamera.Follow = playerStats.transform.GetChild(2);
            followCamera.LookAt = playerStats.transform.GetChild(2);
        }
    }

    public void AddObserver(IEndGameObserve observer)
    {
        endGameObservers.Add(observer);
    }

    public void RemoveObserver(IEndGameObserve observer)
    {
        endGameObservers.Remove(observer);
    }

    public void NotifyObservers()
    {
        //通知观察者游戏结束
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }

    //获取传送门的位置
    public Transform GetEntrance()
    {
        foreach (var item in FindObjectsOfType<TransitionDestination>())
        {
            if (item.destinationTag == TransitionDestination.DestinationTag.ENTER)
            {
                return item.transform;
            }
        }
        return null;
    }
}
