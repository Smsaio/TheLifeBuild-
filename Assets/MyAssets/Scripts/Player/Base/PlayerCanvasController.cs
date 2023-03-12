using Ability;
using PlayerSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
/// <summary>
/// プレイヤー関係のキャンバス制御
/// </summary>
public class PlayerCanvasController : MonoBehaviour,IReactiveProperty
{
    [Header("ステータス表示テキスト")]
    //使用可能な特技テキスト
    [NamedArray(new string[] { "停滞", "拒絶", "転換" })]
    [Header("使用可能判別"), SerializeField] private TextMeshProUGUI[] canSpecialityTexts;
    [Header("特技を使用可能になった場合、波紋を表示するための画像"),
        NamedArray(new string[] { "記憶停滞", "記憶拒絶", "記憶転換" }), SerializeField]
    private Image[] canSpecialityImage = new Image[Enum.GetValues(typeof(Attach.Speciality)).Length];
    //使用した特技の種類
    [Header("使用した特技の把握"), SerializeField] private TextMeshProUGUI playSpecialityText;
    //特技の次に使えるまでの時間を視覚化
    [NamedArray(new string[] { "記憶停滞", "記憶拒絶", "記憶転換" })]
    [Header("特技のクールタイムを表すスライダー"), SerializeField] private Slider specialityValidSlider;

    //各特技のクールタイム表示(使えるようになったら薄くなる)
    [NamedArray(new string[] { "記憶停滞", "記憶拒絶", "記憶転換" })]
    [Header("特技の有効時間を表すスライダー"), SerializeField] private Slider[] specialityCoolSliders = new Slider[Enum.GetValues(typeof(Attach.Speciality)).Length];
    [NamedArray(new string[] { "記憶停滞", "記憶拒絶", "記憶転換" })]
    [Header("特技の有効かどうかを表すスライダーの画像(fill)"), SerializeField] private Image[] specialityCoolSlidersFill = new Image[Enum.GetValues(typeof(Attach.Speciality)).Length];

    //役割レベルテキスト
    [Header("レベル"), SerializeField] private TextMeshProUGUI statusLevelText;
    //役割攻撃力テキスト
    [Header("攻撃力"), SerializeField] private TextMeshProUGUI statusAttackPointText;
    //役割防御力テキスト
    [Header("防御力"), SerializeField] private TextMeshProUGUI statusDefencePointText;
    //役割体力テキスト
    [Header("体力テキスト"), SerializeField] private TextMeshProUGUI statusHPText;
    //役割ステータステキスト
    [Header("体力テキスト"), SerializeField] private TextMeshProUGUI statusRoleText;
    //経験値バー
    [Header("現在の経験値設定"),SerializeField] private Slider expSlider;
    //経験値テキスト
    [Header("経験値テキスト"), SerializeField] private TextMeshProUGUI expText;
    //最大経験値テキスト
    [Header("最大経験値テキスト"), SerializeField] private TextMeshProUGUI expMaxText;
    //レベル表示
    [Header("現在のレベル表示"), SerializeField] private TextMeshProUGUI currentLevelText;
    //最大の体力表示
    [Header("現在の最大体力"), SerializeField] private TextMeshProUGUI currentMaxHPText;
    //現在の体力表示
    [Header("現在の体力"), SerializeField] private TextMeshProUGUI currentHPText;
    //ライフビルドでプラスされた体力
    [Header("増幅された体力を表示する"), SerializeField] private TextMeshProUGUI plusHPText;
    //ステータスアップ
    [NamedArray(new string[] { "攻撃力", "防御力", "逆時間" }),SerializeField]
    private TextMeshProUGUI[] statusUPText = new TextMeshProUGUI[Enum.GetValues(typeof(ItemType.ITEMTYPE)).Length - 1];
    //役割テキスト
    [Header("役割表示"), SerializeField] private TextMeshProUGUI roleText;
    //敵停止
    [Header("敵静止パネル"), SerializeField] private GameObject stagnationPanel;
    //必殺技クールタイムゲージ
    [Header("必殺技のクールタイム"), SerializeField] private Slider deathBlowSlider;
    public Slider DeathBlowSlider { get { return deathBlowSlider; }  } 
    //何が上がったかを示す
    private string[] statusUPType = { "攻撃力", "防御力", "逆時間発動中" };
    private PlayerSpecialityController specialityController;
    private PlayerMove playerMove;
    private Player currentPlayer;
    private int currentPlayerLevel;
    IRole role = default;
    [Inject]
    public void Construct( IRole Irole)
    {
        role = Irole;
    }
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i < canSpecialityImage.Length; i++)
        {
            canSpecialityImage[i].gameObject.SetActive(false); 
        }
        for(int i=0; i < statusUPText.Length; i++)
        {
            statusUPText[i].gameObject.SetActive(false);
        }
        stagnationPanel.SetActive(false);
        ReactivePlayer(role);
    }

    // Update is called once per frame
    void Update()
    {
        StatusText();
        SpecialityDetailDisplay();
        UseSpecialityText();
        CanSpecialityText();
        StateCheck();
        WriteStatusUPText();
    }
    public void ReactivePlayer(IRole role)
    {
        if (role == null) return;
        role.CurrentPlayerSpController.Subscribe(playerSp => { specialityController = playerSp; }).AddTo(this);
        role.CurrentPlayerMove.Subscribe(move => { playerMove = move; }).AddTo(this);
        role.CurrentPlayerLevel.Subscribe(level => { currentPlayerLevel = level; }).AddTo(this);
        role.CurrentPlayer.Subscribe(player => { currentPlayer = player; }).AddTo(this);
    }
    /// <summary>
    /// 使用した特技表示
    /// </summary>
    private void UseSpecialityText()
    {
        var specialityType = (Attach.Speciality)specialityController.NextSpeciality;
        switch (specialityType)
        {
            case Attach.Speciality.Stagnation:
                playSpecialityText.text = "記憶停滞";
                break;
            case Attach.Speciality.Refusal:
                playSpecialityText.text = "記憶拒絶";
                break;
            case Attach.Speciality.Convert:
                playSpecialityText.text = "記憶転換";
                break;
        }
    }

    /// <summary>
    /// 負の記憶受け入れた回数スライダー表示
    /// </summary>
    private void SpecialityDetailDisplay()
    {
        float validValue;
        int index = (int)specialityController.AttachSpeciality;
        int specialityLength = Enum.GetValues(typeof(Attach.Speciality)).Length;
        for (int i = 0; i < specialityLength; i++)
        {
            //特技がまだ使用できない
            if (!specialityController.CanSP[i])
            {
                //負の記憶の合計が最大になったらクールタイム用のスライダーになる
                if (specialityController.NegativeMemoryCount[i] >= specialityController.NegativeMemoryMaxCount[i])
                {
                    //波紋を画面に表示して使用可能を表現する
                    canSpecialityImage[i].gameObject.SetActive(true);
                }
                else
                {
                    validValue = specialityController.NegativeMemoryMaxCount[i] - specialityController.NegativeMemoryCount[i];                    
                    //負の記憶の合計が最大になっていなかったら使用可能までの残りの数のスライダー
                    //使用可能になったら文字が出てくる
                    specialityCoolSliders[i].value = validValue / specialityController.NegativeMemoryMaxCount[i];
                }
                specialityCoolSlidersFill[i].color = new Color32(255, 255, 255, 255);
            }
            else
            {
                //薄くして使用可能に見せる
                specialityCoolSlidersFill[i].color = new Color32(255, 255, 255, 100);            
            }
        }
        for (int i = 0; i < specialityLength; i++)
        {
            bool specialityCool = specialityController.IsSpecialityCool[i];
            //特技使用可能であり、クールタイム中か
            if (specialityController.CanSP[index])
            {
                float value = specialityController.SpecialityCoolTime[i] / specialityController.SpecialityCoolMaxTime[i];
                //クール時間反映
                specialityCoolSliders[i].value = specialityCool ? value : 1;
            }
        }
        //有効時間反映(attach.specialityでやるのは危険な気がする)
        specialityValidSlider.value = specialityController.SpecialityValidTime[index] / specialityController.SpecialityValidMaxTime[index];

        stagnationPanel.SetActive(specialityController.IsSpecialityValid[(int)Attach.Speciality.Stagnation]);
        //必殺技のクールタイム表示
        deathBlowSlider.value = playerMove.DeathBlowCoolTime / playerMove.DeathBlowCoolMaxTime;
    }
    /// <summary>
    /// ステータスメニューでの特技使用判断表示
    /// </summary>
    private void CanSpecialityText()
    {
        string speciality;
        //特技使用可能テキスト
        for (int i = 0; i < canSpecialityTexts.Length; i++)
        {
            string can = specialityController.CanSP[i] ? "可能" : "不能";
            //特技設定
            switch ((Attach.Speciality)i)
            {
                //記憶停滞
                case Attach.Speciality.Stagnation:
                    speciality = "停滞";
                    break;
                //記憶拒絶
                case Attach.Speciality.Refusal:
                    speciality = "拒絶";
                    break;
                //記憶転換
                case Attach.Speciality.Convert:
                    speciality = "転換";
                    break;
                //不具合があった場合
                default:
                    speciality = "無し";
                    break;
            }
            canSpecialityTexts[i].text = speciality +":" + can;
        }
    }
    /// <summary>
    /// 追加されたステータスの書き出し
    /// </summary>
    public void WriteStatusUPText()
    {
        for (int i = 0; i < Enum.GetValues(typeof(ItemType.ITEMTYPE)).Length; i++)
        {
            //アイテムの種類の中で何かがアップするのはこの二つ
            switch ((ItemType.ITEMTYPE)i)
            {
                //攻撃力が上がったらそれに合わせてプレイヤーの攻撃力にプラスする
                case ItemType.ITEMTYPE.PowerUP:
                    bool attackPositive = currentPlayer.PlusAttackP > 0;
                    //攻撃力が0以上であればテキストを表示
                    //ステータスアップテキスト反映
                    statusUPText[i].text = statusUPType[i] + currentPlayer.PlusAttackP.ToString() + "UP";
                    statusUPText[i].gameObject.SetActive(attackPositive);
                    break;
                //防御力が上がったらそれに合わせてプレイヤーの防御力にプラスする
                case ItemType.ITEMTYPE.DefenceUP:
                    //現在のプレイヤー参照
                    bool defencePositive = currentPlayer.PlusDefenceP > 0;
                    //防御力が0以上であればテキストを表示
                    statusUPText[i].text = statusUPType[i] + currentPlayer.PlusDefenceP.ToString() + "UP";
                    statusUPText[i].gameObject.SetActive(defencePositive);
                    break;
                //逆時間のアイテムを入手した
                case ItemType.ITEMTYPE.Reverce:
                    //逆時間が発動し終えたかを監視
                    bool isReverce = currentPlayer.IsReverce;
                    statusUPText[i].gameObject.SetActive(isReverce);
                    break;
            }
        }
    }
    /// <summary>
    /// 役割をテキストに反映
    /// </summary>
    /// <param name="role">役割</param>
    /// <returns></returns>
    public void RoleToText(Attach.Role role)
    {
        string text;
        switch (role)
        {
            case Attach.Role.Swordman:
                text = "剣士";
                break;
            case Attach.Role.Gunner:
                text = "銃士";
                break;
            case Attach.Role.DemonPos:
                text = "悪魔憑き";
                break;
            default:
                text = "役割無し";
                break;
        }
        roleText.text = "役割: " + text;
        statusRoleText.text = roleText.text;
    }

    /// <summary>
    ///ステータス表示
    /// </summary>
    private void StatusText()
    {        
        //ステータス表示用テキスト
        RoleToText(Attach.role);
        statusLevelText.text = "Lv. " + currentPlayerLevel.ToString();
        statusAttackPointText.text = "攻撃力:   " + currentPlayer.AttackP.ToString() + " プラス攻撃力: " + currentPlayer.PlusAttackP;
        statusDefencePointText.text = "防御力:   " + currentPlayer.DefenceP.ToString() + "プラス防御力: " + currentPlayer.PlusDefenceP;
        statusHPText.text = "体力:   " + currentPlayer.CurrentHP.ToString() + "/" + (currentPlayer.CurrentMaxHP + currentPlayer.PlusHP);        
    }
    public void EXPSliderInitialize()
    {
        //体力・最大体力表示
        int curExp = currentPlayer.CurrentEXP;
        int needExp = currentPlayer.NeedEXP;
        //経験値のバー
        float expValue = curExp / (float)needExp;
        if (curExp != 0 && needExp != 0)
        {
            if (expValue != float.NaN)
            {
                expSlider.value = expValue;
            }
        }
    }
    /// <summary>
    /// 状態を確認
    /// </summary>
    public void StateCheck()
    {
        //体力・最大体力表示
        int curExp = currentPlayer.CurrentEXP;
        int needExp = currentPlayer.NeedEXP;
        int curLevel = currentPlayer.CurrentLevel;
        //役割変更中に途切れるので、変更していないときだけ反映
        if (!role.IsRoleChange)
        {
            EXPSliderInitialize();
        }
        currentHPText.text = currentPlayer.CurrentHP.ToString();
        //体力表示
        if (currentPlayer.CurrentHP > currentPlayer.CurrentMaxHP)
        {
            currentHPText.color = Color.yellow;
        }
        currentMaxHPText.text = " / " + (currentPlayer.CurrentMaxHP - currentPlayer.PlusHP).ToString();
        //経験値表示
        expText.text = curExp.ToString();
        //最大経験値表示
        expMaxText.text = needExp.ToString();
        currentLevelText.text = curLevel.ToString();
        //ライフビルドで得られる体力増加分を表示
        if (currentPlayer.PlusHP > 0)
        {
            plusHPText.text = "+" + currentPlayer.PlusHP.ToString();
            plusHPText.color = Color.yellow;
        }
    }
    /*Button _mainButton;

    private void Awake()
    {
        _mainButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        Debug.Log("ボタンが押されました");
    }

    [Inject]
    public void Constructor(CanvasObject injectMainButton)
    {
        // Awakeより先に呼ばれてコンポーネントを注入する
        _mainButton = injectMainButton.GetComponent<Button>();
    }*/
}
