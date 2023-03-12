using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Zenject;
using GameManagerSpace;

/// <summary>
/// メニュー画面
/// </summary>
public class MenuManager : MonoBehaviour
{
    //メニュー画面
    [Header("メニューボタンのパネル")]
    [SerializeField] private GameObject menuButtonPanel;
    //チュートリアル画面
    [Header("説明のパネル")]
    [SerializeField] private GameObject tutorialPanel;
    //ステータス画面
    [Header("ステータスのパネル")]
    [SerializeField] private GameObject statusPanel;
    //ステータス画面へのボタン
    [Header("ステータスボタン")]
    [SerializeField] private Button statusButton;
    //タイトルへのボタン
    [Header("タイトルへのボタン")]
    [SerializeField] private Button titleButton;
    //操作説明へのボタン
    [Header("操作説明のボタン")]
    [SerializeField] private Button tutorialButton;
    //メニューを開いている
    private bool openMenu = false;
    public bool OpenMenu { get { return openMenu; } set { openMenu = value; } }
    protected IGameManager gameManager = default;
    [Inject]
    public void Construct(IGameManager IgameManager)
    {
        gameManager = IgameManager;
    }
    // Start is called before the first frame update
    private void StatusPanel()
    {
        PanelActive(menuButtonPanel,false);
        statusPanel.SetActive(true);
    }
    private void TutorialPanel()
    {
        PanelActive(menuButtonPanel,false);
        tutorialPanel.SetActive(true);
    }
    void Start()
    {
        statusButton.onClick.AddListener(StatusPanel);
        tutorialButton.onClick.AddListener(TutorialPanel);
        gameManager.IsMenu
            .SkipLatestValueOnSubscribe()
            .Subscribe(isMenu => { openMenu = isMenu; Menu(); }).AddTo(this);
        statusPanel.SetActive(false);
        menuButtonPanel.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// 各パネルのアクティブ非アクティブ
    /// </summary>
    /// <param name="obj">表示するかしないかのオブジェクト</param>
    /// <param name="isActive">表示か非表示か</param>
    private void PanelActive(GameObject obj,bool isActive)
    {
        obj.SetActive(isActive);
    }
    /// <summary>
    /// メニューオンオフ
    /// </summary>
    public void Menu()
    {
        //メニューを開いていたら閉じる、閉じているなら開く
        menuButtonPanel.SetActive(!menuButtonPanel.activeSelf);
    }
    /// <summary>
    /// リターンボタンに付与する
    /// </summary>
    /// <param name="obj">非表示にするオブジェクト</param>
    public void MenuReturn(GameObject obj)
    {
        obj.SetActive(false);
        PanelActive(menuButtonPanel,true);
    }
}
