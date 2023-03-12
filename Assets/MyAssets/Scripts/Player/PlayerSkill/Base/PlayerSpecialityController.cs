using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ability;
using PlayerSpace;
using System;
using EnemySpace;
using GameManagerSpace;
using Zenject;

public class PlayerSpecialityController : MonoBehaviour
{
    [SerializeField] private SpecialityBase[] specialityBases = new SpecialityBase[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public SpecialityBase[] SpecialityBases { get { return specialityBases; } }
    ////特技のクールタイム設定
    //[Header("記憶転換で召喚される武器"), SerializeField]
    //private GameObject anuvisWeapon;

    ////味方になる最大数
    //[Header("味方になる最大数"), Range(1, 5), SerializeField] private int maxFollowCharaCount = 3;
    //public int MaxFollowCharaCount { get { return maxFollowCharaCount; } }    
    [NamedArray(new string[] { "記憶停滞使用可能までの回数", "記憶拒絶使用可能までの回数", "記憶転換使用可能までの回数" })]
    [Header("負の記憶を受け入れた最高回数"), Range(10, 20), SerializeField] private int[] negativeMemoryMaxCount = new int[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public int[] NegativeMemoryMaxCount { get { return negativeMemoryMaxCount; } }
    //特技使用可能かどうかの配列
    [NamedArray(new string[] { "記憶停滞", "記憶拒絶", "記憶転換" })]
    [Header("各特技使用可能かどうか"), SerializeField] private bool[] canSP = new bool[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public bool[] CanSP { get { return canSP; } }
     [NamedArray(new string[] { "記憶停滞", "記憶拒絶", "記憶転換" })]
    [Header("特技の最高有効時間"), SerializeField, Range(0, 30)] private float[] specialityValidMaxTime = new float[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public float[] SpecialityValidMaxTime { get { return specialityValidMaxTime; } }
    //特技のクールタイムの時間
    private float[] specialityCoolMaxTime = new float[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public float[] SpecialityCoolMaxTime { get { return specialityCoolMaxTime; } }
    //負の記憶の分類ごとの数
    protected int[] negativeMemoryCount = new int[Enum.GetValues(typeof(MemoryType.MemoryClassification)).Length];
    public int[] NegativeMemoryCount { get { return negativeMemoryCount; } set { negativeMemoryCount = value; } }
    //特技のクールタイム中 
    protected bool[] isSpecialityCool = new bool[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public bool[] IsSpecialityCool { get { return isSpecialityCool; } }

    //各特技の有効時間
    protected float[] specialityValidTime = new float[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public float[] SpecialityValidTime { get { return specialityValidTime; } }
    private bool[] eachSpecialityValid = new bool[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public bool[] IsSpecialityValid { get { return eachSpecialityValid; } }
    // 特技中か
    private bool isSpeciality = false;
    public bool IsSpeciality { get { return isSpeciality; } set { isSpeciality = value; } }

    //クールタイムの時間
    private float[] specialityCoolTime = new float[Enum.GetValues(typeof(Attach.Speciality)).Length];
    public float[] SpecialityCoolTime { get { return specialityCoolTime; } }
    //最初の特技
    private int currentSpeciality = 0;
    public int CurrentSpeciality { get { return currentSpeciality; } set { currentSpeciality = value; } }
    
    //次に使える特技の種類
    private int nextSpeciality = 0;
    public int NextSpeciality { get { return nextSpeciality; }  }
    private Attach.Speciality attachSpeciality = Attach.Speciality.Stagnation;
    public Attach.Speciality AttachSpeciality { get { return attachSpeciality; } }
    //次の特技を探すか
    private bool searchNextSpeciality = false;
    //止めてる間にダメージを与えたゲームオブジェクト
    protected List<GameObject> stopDamageObject = new List<GameObject>();
    //始まってからまだ特技を使用していない
    private bool playSpeciality = false;
    private IGameManager gameManager = default;
    [Inject]
    public void Construct(IGameManager IgameManager)
    {
        gameManager = IgameManager;
    }
    // Start is called before the first frame update
    void Start()
    {
        currentSpeciality = (int)attachSpeciality;   
        //特技の最高発動時間の適用
        for (int i = 0; i < specialityValidMaxTime.Length; i++)
        {
            if (specialityValidMaxTime[i] > 0)
            {
                specialityValidTime[i] = specialityValidMaxTime[i];
            }
        }
        //クールタイムは特技発動時間の倍
        for (int i = 0; i < specialityCoolMaxTime.Length; i++)
        {
            if (specialityCoolMaxTime[i] >= 0)
            {
                specialityCoolMaxTime[i] = specialityValidMaxTime[i] * 2;
                specialityCoolTime[i] = specialityCoolMaxTime[i];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.CurrentGameMode == GameMode.Game)
        {
            UpdateSpeciality();
            SearchNextSpeciality();
            AbilityCool();
            SpecialityDetail();
        }
    }
    //負の記憶受け入れた回数
    private void SpecialityDetail()
    {
        int length = Enum.GetValues(typeof(Attach.Speciality)).Length;
        for (int i = 0; i < length; i++)
        {
            //特技がまだ使用できない
            if (!canSP[i])
            {
                //負の記憶の合計が最大になったらクールタイム用のスライダーになる
                if (negativeMemoryCount[i] >= negativeMemoryMaxCount[i])
                {
                    negativeMemoryCount[i] = negativeMemoryMaxCount[i];
                    //特技が使用可能になる
                    canSP[i] = true;
                }
            }
        }
    }
    /// <summary>
    /// 次の特技表示
    /// </summary>
    private void SearchNextSpeciality()
    {
        //特技を探さず、自分で指定している場合
        if (!searchNextSpeciality) return;
        //特技を初めて使用し終えていない場合は現在使用できる物を用意する
        if (!playSpeciality)
        {
            CanSearch();
            return;
        }
        //特技を初めて使用し終えている
        int length = Enum.GetValues(typeof(Attach.Speciality)).Length;
        int next =  currentSpeciality + 1 < length ? currentSpeciality + 1 : 0;
        //次に使用する特技が使用できなければ
        if (!canSP[next])
        {
            CanSearch();   
        }
        else
        {
            nextSpeciality = next;
        }
    }
    /// <summary>
    /// 使う特技を指定する
    /// </summary>
    /// <param name="inputMove">移動キーで特技を動かす</param>
    /// <param name="isChange">操作によって変えるか</param>
    public void PlaySpecialityControll(Vector3 inputMove,bool isChange)
    {
        if (!isChange) return;
        searchNextSpeciality = false;
        int searchSpeciality;
        //左にさしている
        if (inputMove.x > 0)
        {
            searchSpeciality = (int)Attach.Speciality.Convert;
            if (canSP[searchSpeciality])
            {
                nextSpeciality = searchSpeciality;
            }
        }
        //右にさしている
        else if (inputMove.x < 0)
        {
            searchSpeciality = (int)Attach.Speciality.Stagnation;
            if (canSP[searchSpeciality])
            {
                nextSpeciality = searchSpeciality;
            }
        }
        //上をさしている
        if (inputMove.z > 0)
        {
            searchSpeciality = (int)Attach.Speciality.Refusal;
            if (canSP[searchSpeciality])
            {
                nextSpeciality = searchSpeciality;
            }
        }
        else if (inputMove.z < 0 || inputMove == Vector3.zero)
        {
            searchNextSpeciality = true;
        }
    }
    //次に出来る物を探す
    private void CanSearch()
    {
        //全体から探す
        for (int i = 0; i < canSP.Length; i++)
        {
            //特技を使用可能で
            if (canSP[i])
            {
                //要素番号を取得
                nextSpeciality = i;
                return;
            }
        }
        //見つけられなかったら次の特技を最初の特技にする
        nextSpeciality = 0;
    }
    public void UseSpeciality()
    {
        if (isSpecialityCool[(int)attachSpeciality]) return;
        int currentSpecialityInt;
        //特技を初めて使用し終えていない
        if (!playSpeciality)
        {
            playSpeciality = true;
        }
        SetSpeciality();
        //特技発動
        if (isSpeciality)
        {
            //奥義によって能力が違う
            switch (attachSpeciality)
            {
                case Attach.Speciality.Stagnation:
                    Debug.Log("記憶停滞");
                    currentSpecialityInt = (int)Attach.Speciality.Stagnation;
                    //停滞によって止まった
                    eachSpecialityValid[currentSpecialityInt] = true;
                    specialityBases[currentSpecialityInt].UseSpeciality();
                    break;
                case Attach.Speciality.Refusal:
                    currentSpecialityInt = (int)Attach.Speciality.Refusal;
                    eachSpecialityValid[currentSpecialityInt] = true;
                    specialityBases[currentSpecialityInt].UseSpeciality();
                    Debug.Log("記憶拒絶");
                    break;
                case Attach.Speciality.Convert:
                    currentSpecialityInt = (int)Attach.Speciality.Convert;
                    //転換の特技を使った
                    eachSpecialityValid[currentSpecialityInt] = true;
                    specialityBases[currentSpecialityInt].UseSpeciality();
                    Debug.Log("記憶転換");                          
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// 特技中の処理
    /// </summary>
    private void UpdateSpeciality()
    {
        int length = Enum.GetValues(typeof(Attach.Speciality)).Length;
        for (int i = 0; i < length; i++)
        {
            Attach.Speciality specialityType = (Attach.Speciality)i;
            //発動した特技が設定したものと同じだった場合
            if (attachSpeciality == specialityType)
            {
                //特技中
                if (isSpeciality && specialityValidTime[i] <= specialityValidMaxTime[i])
                {
                    //特技制限時間
                    specialityValidTime[i] -= Time.deltaTime;
                    //終了
                    if (specialityValidTime[i] <= 0)
                    {
                        //特技の有効時間を初期化
                        specialityValidTime[i] = specialityValidMaxTime[i];
                        isSpecialityCool[i] = true;
                        DoneSpeciality();
                    }
                }
            }
        }
    }
    /// <summary>
    /// 特技終了
    /// </summary>
    public void DoneSpeciality()
    {
        for(int i=0; i < specialityBases.Length; i++)
        {
            if (specialityBases[i] != null)
            {
                specialityBases[i].DoneSpeciality();
            }
        }
        for (int i = 0; i < specialityValidTime.Length; i++)
        {
            specialityValidTime[i] = specialityValidMaxTime[i];
        }
        for (int i = 0; i < eachSpecialityValid.Length; i++)
        {
            eachSpecialityValid[i] = false;
        }
    }
    /// <summary>
    /// static変数に特技を設定
    /// </summary>
    public void SetSpeciality()
    {
        isSpeciality = true;
        currentSpeciality = nextSpeciality;
        attachSpeciality = (Attach.Speciality)currentSpeciality;
    }

    /// <summary>
    /// アビリティのクール時間(abilityCoolTime,abilityMaxCoolTime)
    /// </summary>
    private void AbilityCool()
    {
        int length = Enum.GetValues(typeof(Attach.Speciality)).Length;
        for (int i = 0; i < length; i++)
        {
            //アビリティを発動した後の効果時間が終了と同時にisSpecialityCoolがtrueになる。
            if (isSpecialityCool[i] && specialityCoolTime[i] <= specialityCoolMaxTime[i])
            {
                specialityCoolTime[i] -= Time.deltaTime;
                //特技を使用し終えた
                isSpeciality = false;
                if (specialityCoolTime[i] <= 0)
                {
                    specialityCoolTime[i] = specialityCoolMaxTime[i];
                    //クールタイムが終わった。
                    isSpecialityCool[i] = false;
                }
            }
        }
    }
}
