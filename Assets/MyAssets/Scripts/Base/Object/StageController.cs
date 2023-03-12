using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayerSpace;
using System;
using Ability;
using TMPro;
using UnityEngine.UI;
using GameManagerSpace;
using Zenject;

/// <summary>
/// ステージの中で必要なものを司るクラス
/// </summary>
public class StageController : MonoBehaviour
{
    [Header("ゲームオーバー")] public GameObject gameOverObj;
    [Header("フェード")] public FadeImage fade;
    [Header("ステージクリア")] public GameObject stageClearObj;
    //タイトルボタン
    [SerializeField] private Button[] titleButton;
    //ローディング
    [SerializeField] private GameObject loadingUI;
    //ローディング経過時間
    [SerializeField] private Slider progressSlider;
    //ロードを開始
    private bool loadStart = false;
    public bool LoadingStart { get { return loadStart; } set { loadStart = value; } }
    //シーンの名前
    private string sceneName = "";
    //プレイヤー
    private Player player;
    //プレイヤーが操作しているオブジェクト
    private GameObject playerObj;
    //フェードを始める
    private bool startFade = false;
    private bool doGameOver = false;
    private bool doSceneChange = false;
    private bool doClear = false;
    IGameManager gameManager = default;

    [Inject]
    public void Construct(IGameManager IgameManager)
    {
        gameManager = IgameManager;
    }
    // Start is called before the first frame update
    void Start()
    {
        playerObj = GameObject.FindWithTag("Player");
        //初期化
        if (playerObj != null && gameOverObj != null && fade != null && stageClearObj != null)
        {
            gameOverObj.SetActive(false);
            stageClearObj.SetActive(false);
            player = playerObj.GetComponent<Player>();
            if (player == null)
            {
                Debug.Log("プレイヤーじゃない物がアタッチされているよ！");
            }
        }
        else
        {
            Debug.Log("設定が足りてないよ！");
        }
        //タイトルボタンの初期化
        for (int i = 0; i < titleButton.Length; i++)
        {
            titleButton[i].onClick.AddListener(Title);
            titleButton[i].gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーがやられた時の処理
        if (player != null)
        {
            GameOver();
        }
        //ゲームオーバーせずクリアしていて、クリアの挙動をしてない場合
        if (!doClear)
        {
            StageClear();
        }
        //ステージを切り替える
        if (fade != null && startFade && !doSceneChange)
        {
            if (fade.IsFadeOutComplete())
            {
                gameManager.StageNum++;
                doSceneChange = true;
            }
        }
    }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    public void GameOver()
    {
        //ゲームオーバーになったときとまだゲームオーバーの処理をしていないとき
        if (gameManager.IsGameOver && !doGameOver)
        {
            gameOverObj.SetActive(true);
            doGameOver = true;
            Time.timeScale = 0;
        }
    }
    /// <summary>
    /// タイトルに戻る
    /// </summary>
    public void Title()
    {
        //クリアやオーバーを解除
        gameManager.StateReset();
        for (int i = 0; i < titleButton.Length; i++)
        {
            titleButton[i].gameObject.SetActive(false);
        }
        //ゲームモードをタイトルに
        gameManager.SetGameMode(GameMode.Title);
        ChangeScene();
        Time.timeScale = 1;
    }

    /// <summary>
    /// ステージを切り替える。
    /// </summary>
    /// <param name="nextSceneName">シーン名</param>
    public void ChangeScene(string nextSceneName = "")
    {
        if (fade != null)
        {            
            loadStart = true;
            sceneName = nextSceneName;
            fade.StartFadeOut();
            startFade = true;
            StartCoroutine(GameLoadScene());
        }
    }

    /// <summary>
    /// シーンをロードする
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameLoadScene()
    {
        int nextSceneIndex = 0;
        if (loadStart)
        {
            loadingUI.SetActive(true);
            loadStart = false;
        }
        //ローディングが表示されている
        if (loadingUI.gameObject.activeSelf)
        {
            var currentScene = SceneManager.GetActiveScene();
            var sceneIndex = currentScene.buildIndex;
            nextSceneIndex = sceneIndex - 1;
            //シーン読み込み
            var async = SceneManager.LoadSceneAsync(nextSceneIndex);
            while (!async.isDone)
            {
                progressSlider.value = async.progress;
                yield return null;
            }
        }
    }
    /// <summary>
    /// ステージをクリアした
    /// </summary>
    public void StageClear()
    {
        if (gameManager.IsStageClear)
        {
            doClear = true;
            loadStart = true;
            Time.timeScale = 0;
            stageClearObj.SetActive(true);
        }
    }
}