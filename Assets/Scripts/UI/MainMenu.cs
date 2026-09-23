using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{

    public Button newGameBtn;
    public Button continueBtn;
    public Button exitBtn;

    PlayableDirector director; 

    private void Awake()
    {
        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueBtn = transform.GetChild(2).GetComponent<Button>();
        exitBtn = transform.GetChild(3).GetComponent<Button>();

        continueBtn.interactable = PlayerPrefs.HasKey("level");

        //添加监听事件
        newGameBtn.onClick.AddListener(PlayTimeline);
        continueBtn.onClick.AddListener(ContinueGame);
        exitBtn.onClick.AddListener(ExitGame);

        director = FindAnyObjectByType<PlayableDirector>();
        director.stopped += NewGame;
    }

    void PlayTimeline()
    {
        director.Play();
    }

    private void NewGame(PlayableDirector obj)
    {
        PlayerPrefs.DeleteAll();
        //场景转换
        SceneController.Instance.TransitionToFirstLevel();
    }
    private void ContinueGame()
    {
        //转换场景，读取进度
        SceneController.Instance.TransitionToLoadGame();
    }

    private void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }
}
