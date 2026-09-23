using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

//名为SceneController，实为SceneManager，这样起名是为了避免与Unity自带的SceneManager冲突
public class SceneController : Singleton<SceneController>, IEndGameObserve
{
    public GameObject playerPrefab;

    public SceneFader sceneFaderPrefab;//场景切换的淡入淡出效果

    bool fadeFinished;

    GameObject player;

    NavMeshAgent playerAgent;

    //这样可以做是为了让SceneController在场景切换时不被销毁，并且只有一个实例
    //在Awake中调用base.Awake()，确保单例模式生效
    //然后调用DontDestroyOnLoad(this)，确保场景切换时不被销毁
    //这样SceneController就可以在场景切换时保持存在，并且可以访问到它
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        GameManager.Instance.AddObserver(this);
        fadeFinished = true;
    }

    //传送方法，由TransitionPoint调用
    //transitionPoint：传送点，包含目标场景名、目标传送点标签、传送类型
    //传送类型：同场景传送、跨场景传送
    //同场景传送：直接切换位置
    //跨场景传送：加载目标场景，然后切换位置

    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                //同场景下，直接切换位置
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;
        }
    }

    //传送(协程：在传送前等待一帧，确保场景加载完成)
    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        //TODO: 保存数据
        SaveManager.Instance.SavePlayerData();

        //如果当前场景不是目标场景，则加载目标场景
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            //TODO: 可加入fader
            yield return SceneManager.LoadSceneAsync(sceneName); //等待异步加载场景完毕
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation); //等待玩家实例化
            
            SaveManager.Instance.LoadPlayerData(); //加载玩家数据，同步两个场景中的玩家数据

            yield break;
        }
        else
        {
            //如果player为空，则获取玩家(在场景加载完成后)
            player = GameManager.Instance.playerStats.gameObject;

            //将玩家Agent禁用，防止寻路干扰传送，传送完再变回来
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;

            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);

            playerAgent.enabled = true;
            yield return null;
        }
    }

    //找到目标场景中的传送点
    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag)
    {
        //返回所有TransitionDestination组件
        //找到与destinationTag匹配的组件
        //如果没有找到，返回null
        //如果找到，返回该组件
        //注意：这里使用了FindObjectsOfType，所以会返回所有场景中的TransitionDestination组件的数组
        var entrances = FindObjectsOfType<TransitionDestination>();
        for (int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].destinationTag == destinationTag)
                return entrances[i];
        }
        return null;
    }

    public void TransitionMain()
    {
        StartCoroutine(LoadMain());
    }

    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }

    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Game"));
    }

    IEnumerator LoadLevel(string scene)
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);
        if (scene != "")
        {
            yield return StartCoroutine(fade.FadeOut(1f));
        
            yield return SceneManager.LoadSceneAsync(scene);
            yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);
        
            SaveManager.Instance.SavePlayerData();

            yield return StartCoroutine(fade.FadeIn(1f));
            yield break;
        }
    }

    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);
        yield return StartCoroutine(fade.FadeOut(1f));
        yield return SceneManager.LoadSceneAsync("Main");
        yield return StartCoroutine(fade.FadeIn(1f));
        yield break;
    }

    public void EndNotify()
    {
        if (fadeFinished)
        {
            fadeFinished = false;
            StartCoroutine(LoadMain());
        }
    }
}
