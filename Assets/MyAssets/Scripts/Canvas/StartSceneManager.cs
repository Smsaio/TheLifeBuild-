using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Ability;
using UnityEngine.InputSystem;
using GameManagerSpace;
using Zenject;

/// <summary>
/// スタート画面専用の管理クラス
/// </summary>
public class StartSceneManager : MonoBehaviour
{
    //職業ボタン
    [Header("役割選択ボタン")]
    [SerializeField] private Button swordButton;
    [SerializeField] private Button gunnerButton;
    [SerializeField] private Button standButton;
    //メニュー選択時のスタートボタン
    [Header("各メニューへのボタン")]
    [SerializeField] private Button menuStartButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button tutorialButton;
    //役割選択時のスタートボタン
    [Header("始めた時の音")]
    [SerializeField] private Button roleToStartButton;
    //始まるときの音
    [SerializeField] private AudioClip startSE;
    //各メニュー画面のパネル
    [Header("各メニューへのパネル")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject tutorialPanel;
    //現在選んでいる役割
    //役割表示
    [SerializeField] private Text roleText;
    //ローディング
    [Header("ローディング画面")]
    [SerializeField] private GameObject loadingUI;
    //経過時間表示
    [SerializeField] private Slider progressSlider;
    //例としてのプレイヤーの画像
    [Header("選択しているプレイヤー"),NamedArray(new string[] {"剣士","銃士","悪魔憑き"})]
    [SerializeField] private GameObject[] examplePlayerList;
    //スタンドの画像
    [SerializeField] private GameObject demonImage;
    [SerializeField] private RoleScriptable roleScriptable;
    //スタートボタンを押した
    private bool onStart = false;
    //スタートの時のボタン制御
    private PlayerInputAction inputActions;
    IGameManager gameManager = default;
    IAudioSourceManager audioSourceManager = default;
    [Inject]
    public void Construct(IGameManager IgameManager,IAudioSourceManager IaudioSourceManager)
    {
        gameManager = IgameManager;
        audioSourceManager = IaudioSourceManager;
    }
    private void Awake()
    {
        inputActions = new PlayerInputAction();        
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        //ボタンに機能を追加する
        swordButton.onClick.AddListener(SwordButton);
        gunnerButton.onClick.AddListener(GunnerButton);
        standButton.onClick.AddListener(StandButton);
        roleToStartButton.onClick.AddListener(StartButton);
        //メニュー選択時のスタート
        menuStartButton.onClick.AddListener(StartButton);
        selectButton.onClick.AddListener(RoleSelectButton);
        tutorialButton.onClick.AddListener(TutorialButton);
        inputActions.Start.Any.performed += OnStart;
        selectPanel.SetActive(false);
        titlePanel.SetActive(true);
        loadingUI.SetActive(false);
        demonImage.SetActive(false);
        for(int i = 0; i < examplePlayerList.Length; i++)
        {
            examplePlayerList[i].SetActive(false);
        }
        demonImage.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// スタート画面の何かのボタンを押した場合
    /// </summary>
    /// <param name="context"></param>
    private void OnStart(InputAction.CallbackContext context)
    {
        titlePanel.SetActive(false);
        menuPanel.SetActive(true);
        //GameManager.Instance.PlaySE(startSE);
    }
    private void RoleSelectButton()
    {
        selectPanel.SetActive(true);
        NonActive(menuPanel);
    }
    private void TutorialButton()
    {
        tutorialPanel.SetActive(true);
        NonActive(menuPanel);
    }
    private void NonActive( GameObject obj)
    {
        obj.SetActive(false);
    }
    private void SwordButton()
    {
        demonImage.SetActive(false);
        examplePlayerList[(int)roleScriptable.role].SetActive(false);
        roleScriptable.role = Attach.Role.Swordman;
        examplePlayerList[(int)roleScriptable.role].SetActive(true);
        roleText.text = "役割: 剣士";
    }
    private void GunnerButton()
    {
        demonImage.SetActive(false);
        examplePlayerList[(int)roleScriptable.role].SetActive(false);
        roleScriptable.role = Attach.Role.Gunner;       
        examplePlayerList[(int)roleScriptable.role].SetActive(true);
        roleText.text = "役割: 銃士";
    }
    private void StandButton()
    {
        demonImage.SetActive(true);
        examplePlayerList[(int)roleScriptable.role].SetActive(false);
        roleScriptable.role = Attach.Role.DemonPos;
        examplePlayerList[(int)roleScriptable.role].SetActive(true);
        roleText.text = "役割: 悪魔憑き";
    }
    public void StartButton()
    {
        Attach.role = roleScriptable.role;
        //ゲームモードをゲームに
        gameManager.SetGameMode(GameMode.Game);
        onStart = true;
        NonActive(menuPanel);
        audioSourceManager.BGMChange();
        StartCoroutine(LoadScene());
    }
    /// <summary>
    /// シーンをロードする
    /// </summary>
    IEnumerator LoadScene()
    {
        if (onStart)
        {
            loadingUI.SetActive(true);
            onStart = false;
        }
        //ローディングが表示されている
        if (loadingUI.gameObject.activeSelf)
        {
            var currentScene = SceneManager.GetActiveScene();
            var sceneIndex = currentScene.buildIndex;
            var nextSceneIndex = sceneIndex + 1;
            //シーン読み込み
            var async = SceneManager.LoadSceneAsync(nextSceneIndex);
            //シーン読み込み
            //var async = SceneManager.LoadSceneAsync(gameManager.SceneNames[(int)gameManager.CurrentGameMode] + gameManager.StageNum.ToString());
            //読み込みが完了していない
            while (!async.isDone)
            {
                progressSlider.value = async.progress;
                yield return null;
            }
        }
    }
    /// <summary>
    /// シーンの中のリターンボタンに付与する
    /// </summary>
    /// <param name="obj">非表示にするオブジェクト</param>
    public void MenuReturn(GameObject obj)
    {
        obj.SetActive(false);
        menuPanel.SetActive(true);
    }
}
