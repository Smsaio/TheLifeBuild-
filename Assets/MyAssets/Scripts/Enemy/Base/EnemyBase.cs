using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PlayerSpace;
using Ability;
using System;
using TMPro;
using UniRx;
using GameManagerSpace;
using DG.Tweening;
using Zenject;

namespace EnemySpace
{
    /// <summary>
    /// ジェネリックを隠すために継承してしまう
    /// [System.Serializable]を書くのを忘れない
    /// 型設定したものを継承し、変数として使えるようにする
    /// </summary>
    [System.Serializable]
    public class MaterialTable : Serialize.TableBase<Renderer, Material[], MaterialData>
    {

    }

    /// <summary>
    /// ジェネリックを隠すために継承してしまう
    /// [System.Serializable]を書くのを忘れない
    /// キーと値の型設定
    /// </summary>
    [System.Serializable]
    public class MaterialData : Serialize.KeyAndValue<Renderer, Material[]>
    {
        public MaterialData(Renderer key, Material[] value) : base(key, value)
        {

        }
    }
    
    public class EnemyBase : Character, IDamageble, ITargetSearch
    {
        [Header("敵の基本設定")]
        //敵のステータス
        public EnemyStatus enemyStatus;
        //味方にできない
        [Header("味方にはできない")]
        public bool notCanFellow = false;
        #region Serialize


        [SerializeField] protected FindMark findMark;
        //敵の体力バー
        [SerializeField] protected HPGauge enemyGauge;
        //ダメージのUI
        [SerializeField] private GameObject damageUI;
        //体のレンダラー
        [Header("味方になった時に見た目を変えるためにメッシュと示し合わせる必要があるため")]
        [SerializeField] protected Renderer[] meshRenderer;
        //　接地確認の球のコライダの半径
        [Header("地面判定の半径")]
        [SerializeField] protected float groundColliderRadius = 0.29f;
        [Header("食らった時の音")]
        //ダメージの音
        [SerializeField] protected AudioClip damageSE;

        //敵を検知するクラス
        [SerializeField] protected SearchCharacter searchCharacter;
        [Header("ダメージのエフェクト")]
        //ダメージのパーティクル
        [SerializeField] private ParticleSystem damageParticle;
        //パーティクルを召喚する位置
        [SerializeField] private Transform damageTransform;
        //ダメージテキスト召喚位置
        [SerializeField] private Transform damageUITransform;
        //重力を与えて徐々に遅くなっているときのエフェクト
        [SerializeField] private GameObject refusalEffect;
        [Header("負の記憶をランダムにするかどうか")]
        [SerializeField] private bool isMemoryRandom = false;
        //負の感情の記憶テキスト
        [Header("負の記憶表示用とレベルのテキスト")]
        [SerializeField] private TextMeshProUGUI memoryText;
        //レベルテキスト
        [SerializeField] private TextMeshProUGUI levelText;
        //味方の時のマテリアル(メッシュレンダラー)このメッシュならこのマテリアルにするものを入れる
        [Header("このメッシュならこのマテリアルにというふうにする。味方になった時のマテリアル")]
        [SerializeField] private MaterialTable fellowMaterial;
        //点滅中のマテリアル
        [Header("ダメージ時の点滅のマテリアル")]
        [SerializeField] private Material flashMaterial;
        //点滅回数(奇数だと変化したまま、偶数だと変化前と同じになる
        [Header("点滅回数(奇数だと変化したまま、偶数だと変化前と同じになる")]
        [Range(4, 10), SerializeField] private int flashCount = 6;
        [Header("1に近づくほど強く減衰、-1に近づくほど強く増幅")]
        [Range(-1.0f, 1.0f), SerializeField] private float flashPeriod = 0.4f;
        //攻撃する距離
        [Header("攻撃する距離"),Range(1.0f, 10.0f), SerializeField] protected float attackDistance = 4.0f;

        //出現したレベルをプラスする
        [Header("プラスするレベル"), Range(5, 20), SerializeField] protected int plusLevel = 10;
        //このオブジェクトが司っている負の記憶の種類
        [SerializeField]
        private MemoryType.NegativeMemory memory = MemoryType.NegativeMemory.Hatred;
        [SerializeField] private CharacterType characterType;
        public CharacterType MyCharacterType { get { return characterType; } }
        //調べる対象のレイヤー
        [Header("障害物のレイヤー"), SerializeField] protected LayerMask obstacleLayer = 7;
        //死んだ後のラグドール
        [SerializeField] protected GameObject enemyRagdoll;
        public GameObject EnemyRagDoll { get { return enemyRagdoll; } }
        //スクリーム状態になってから解除までの時間
        [Header("スクリーム状態の有効時間"), SerializeField] private float screamTime = 3.0f;
        public float ScreamTime { get { return screamTime; } }
        #endregion

        //スクリームをする
        protected bool doScream = false;
        public bool DoScream { get { return doScream; } set { doScream = value; } }
        //体にプレイヤーが当たった場合のダメージ
        protected int bodyDamage = 10;
        public int BodyDamage { get { return bodyDamage; } }
        //引き寄せられる武器のプロパティ
        protected GameObject anuvis;
        public GameObject AnuvisObject { set { anuvis = value; } }

        //記憶転換の武器の範囲内であるかのプロパティ
        protected bool inAnuvisArea = false;
        public bool InAnuvisArea { set { inAnuvisArea = value; } }
        //敵がプレイヤーの味方になったかどうか
        protected bool onFellow = false;
        public bool OnFellow { set { onFellow = value; } get { return onFellow; } }

        //ダメージを食らったか
        protected bool isDamage = false;
        public bool IsDamage { set { isDamage = value; } get { return isDamage; } }

        //現在のレベル
        protected int curLevel = 1;
        public int CurrentLevel { get { return curLevel; } }
        //物理
        protected Rigidbody rigid;
        public Rigidbody Rb { get { return rigid; } }
        //重力が当たったか
        private bool isHitRefusal = false;
        public bool IsHitRefusal { get { return isHitRefusal; } set { isHitRefusal = value; } }
        //司っている負の記憶の分類
        private MemoryType.MemoryClassification memoryClassification = MemoryType.MemoryClassification.None;
        public MemoryType.MemoryClassification MemoryClassification { get { return memoryClassification; }  }
        
        //プレイヤーを見つけた
        protected bool isTargetFind = false;
        public bool IsTargetFind { set { isTargetFind = value; } get { return isTargetFind; } }

        protected Transform target;
        public Transform Target { get { return target; } }
        //アヌビスで引き寄せられる力 
        private float anuvisPower = 5;
        public float AnuvisPower { set { anuvisPower = value; } }

        protected Animator animator;
        public Animator EnemyAnimator { get { return animator; } }
        protected CharacterState enemyState = CharacterState.Wait;
        //navmeshの速さ、速度、回転速度格納変数
        protected float startAgentspeed, startAgentangularSpeed, startAgentacceleration;
        //最初のスピードを保持したか
        protected bool startAgentKeep = false;
        //敵の時のマテリアル(メッシュレンダラー)
        protected Dictionary<Renderer,Material[]> enemyMaterial = new Dictionary<Renderer, Material[]>();
        //敵の時のアイコンのマテリアル(アイコン)
        protected Material enemyIconMaterial;
        //死んだか
        protected bool isDeath = false;
        //味方になった場合のクラス
        protected FollowChara followChara;
        //万有引力定数の定義 正式名称が大文字のGのため、これは例外
        protected float G = 6.67259f;
        //引き寄せられるオブジェクトのrigidbody
        protected Rigidbody anuvis_rb;
        //スピードが減るタイミング
        private float speedChangeTime = 0.0f;
        private float speedChangeSpan = 0.3f;
        //死んだときのラグドール召喚y座標
        protected float deathSpawnY = 1.2f;
        //最高レベル
        protected const int maxLevel = 100;
        protected AnimatorStateInfo stateInfo;
        //地面判定
        protected bool isGrounded;
        protected NavMeshAgent agent;
        //ダメージの状態から抜け出す
        protected float damageSpanTime = 0.2f;
        //赤い点滅
        protected float damageColorTime = 0.2f;
        //プレイヤー参照
        protected PlayerSpecialityController specialityController;
        protected Player player;
        protected int playerLevel = 0;
        //地面判定の判定範囲
        protected Vector3 groundPositionOffset = new Vector3(0f, 0.02f, 0f);
        //animator高速化
        protected int animWalkSpeed = Animator.StringToHash("WalkSpeed");
        protected int damageAnim = Animator.StringToHash("Damage");
        protected int deathAnim = Animator.StringToHash("Death");
        protected int attackAnim = Animator.StringToHash("Attack");

        //実際の経験値
        private int exp;
        //相手の弱点であった時に倒された場合増やす
        private float mulWeekDefeat = 1.0f;
        //特技拒絶の遅くする速度の限界
        private float limitAgentspeed, limitAgentangularSpeed, limitAgentacceleration;
        //前のサイズ
        private Vector3 beforeScale;
        protected IAudioSourceManager audioSourceManager = default;
        public IAudioSourceManager AudioSourceManager { set { audioSourceManager = value; } get { return audioSourceManager; } }
        protected IGameManager gameManager = default;
        public IGameManager GameManager { set { gameManager = value; } }
        [Inject]
        public void Construct(IAudioSourceManager IaudioSourceManager,IGameManager IgameManager)
        {
            audioSourceManager = IaudioSourceManager;
            gameManager = IgameManager;
        }

        //スクリプトが非アクティブでも実行
        protected virtual void Awake()
        {
            
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            if (Mathf.Approximately(Time.timeScale, 0)) return;
            NecessaryMethod();
            PlayerWeekScale();
        }
        public CharacterState GetState()
        {
            return enemyState;
        }
        //目標を見つけている場合
        public virtual void TargetFind(GameObject obj)
        {            
            //　敵キャラクターが追いかけられる状態であれば追いかけるに変更
            if (enemyState != CharacterState.Chase && enemyState != CharacterState.Charge
                 && !Physics.Linecast(transform.position + Vector3.up, obj.transform.position + Vector3.up, obstacleLayer))
            {
                if (!isTargetFind)
                {
                    findMark.SetSize();
                }
                SetState(CharacterState.Chase, target);
                isTargetFind = true;
            }
        }
        //目標を見失った場合
        public virtual void TargetLost()
        {
            isTargetFind = false;
            SetState(CharacterState.Wait);
        }
        public void NonTargettingAttackReceive(WeaponDamageStock damage)
        {
            //進む先がプレイヤーの前か索敵した敵か
            if (damage != null)
            {
                //遠距離攻撃だった場合
                if (damage.isBullet)
                {
                    //遠距離攻撃の発射位置(発射したプレイヤーの位置)
                    isTargetFind = true;
                    SetState(CharacterState.Chase, target);
                    agent.isStopped = false;
                    Destroy(damage.gameObject);
                }
            }
        }
        /// <summary>
        /// 初期化を設定
        /// </summary>
        private void SetInitialize()
        {
            SetNegativeMemory();
            var level = UnityEngine.Random.Range(playerLevel, playerLevel + plusLevel);
            curLevel = level;
            //味方になる敵である場合
            if (!notCanFellow)
            {
                //味方になった時、マテリアルを変更するためマテリアルを連想配列に代入
                for (int i = 0; i < meshRenderer.Length; i++)
                {
                    try
                    {
                        //万が一代入されていたら困るので消してから初期化
                        enemyMaterial.Remove(meshRenderer[i]);
                        enemyMaterial.Add(meshRenderer[i], meshRenderer[i].materials);
                    }
                    catch
                    {
                        //既にある項目を追加しようとした
                    }
                }
            }
            StatusInitialize();
            //コンポーネントを取得
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            rigid = GetComponent<Rigidbody>();
            //記憶停滞で使う敵停止を追加
            gameObject.AddComponent<Pauser>();
            agent.speed = enemyStatus.agentWalkSpeed;
            refusalEffect.SetActive(false);
            //エージェント速さ格納
            startAgentspeed = agent.speed;
            startAgentangularSpeed = agent.angularSpeed;
            startAgentacceleration = agent.acceleration;
            var speciality = specialityController.SpecialityBases[(int)Ability.Attach.Speciality.Refusal];
            //速度が遅くなる限界
            limitAgentspeed = startAgentspeed * speciality.RefusalLimitSlowSpeed;
            limitAgentangularSpeed = startAgentangularSpeed * speciality.RefusalLimitSlowSpeed;
            limitAgentacceleration = startAgentacceleration * speciality.RefusalLimitSlowSpeed;
            beforeScale = transform.localScale;

        }

        //プレイヤー監視
        public override void ReactivePlayer(IRole role)
        {
            base.ReactivePlayer(role);
            if (role == null) return;
            role.CurrentPlayerSpController.Subscribe(spController => { specialityController = spController; }).AddTo(this);
            role.CurrentPlayer.Subscribe(currentPlayer => { player = currentPlayer; }).AddTo(this);
            role.CurrentPlayerTransform.Subscribe(currentTransform => { target = currentTransform; }).AddTo(this);
            role.CurrentPlayerLevel.Subscribe(level => { playerLevel = level; }).AddTo(this);
            SetInitialize();
        }
        /// <summary>
        ///負の記憶設定
        /// </summary>
        private void SetNegativeMemory()
        {
            //特殊な記憶が2個あるので - 2
            //負の記憶の種類設定
            if(isMemoryRandom)
                memory = (MemoryType.NegativeMemory)UnityEngine.Random.Range(0, Enum.GetValues(typeof(MemoryType.NegativeMemory)).Length - 2);
            //記憶の種類に応じてテキストの色を変える。
            //テキストを司っている記憶に変換
            switch (memory)
            {
                case MemoryType.NegativeMemory.Hatred:
                    memoryText.text = "憎悪";
                    memoryText.color = Color.red;
                    break;
                case MemoryType.NegativeMemory.Angry:
                    memoryText.text = "怒り";
                    memoryText.color = Color.red;
                    break;
                case MemoryType.NegativeMemory.Tragic:
                    memoryText.text = "悲壮感";
                    memoryText.color = Color.blue;
                    break;
                case MemoryType.NegativeMemory.Sorrowful:
                    memoryText.text = "悲哀";
                    memoryText.color = Color.blue;
                    break;
                case MemoryType.NegativeMemory.Despair:
                    memoryText.text = "絶望";
                    memoryText.color = Color.yellow;
                    break;
                case MemoryType.NegativeMemory.Uneasiness:
                    memoryText.text = "不安感";
                    memoryText.color = Color.yellow;
                    break;
                case MemoryType.NegativeMemory.Trauma:
                    memoryText.text = "トラウマ";
                    memoryText.color = Color.white;
                    break;
                case MemoryType.NegativeMemory.Darkhistory:
                    memoryText.text = "黒歴史";
                    memoryText.color = Color.black;
                    break;
                default:
                    Debug.Log("負の記憶が設定されていません。");
                    break;
            }
            //この敵オブジェクトは味方化可能か
            if (!notCanFellow)
            {
                followChara = GetComponent<FollowChara>();
                //記憶決め
                if (followChara != null)
                {
                    followChara.fellowGoodMemory = (MemoryType.GoodMemory)((int)memory);
                }
            }
            SetClassification();
        }
        /// <summary>
        /// 負の記憶の分類設定
        /// </summary>
        private void SetClassification()
        {
            //負の記憶の種類を分類に分ける
            switch (memory)
            {
                //憎しみと怒りを壮絶な記憶という分類に
                case MemoryType.NegativeMemory.Hatred:
                case MemoryType.NegativeMemory.Angry:
                    memoryClassification = MemoryType.MemoryClassification.Spectacular;
                    break;
                //悲壮感と悲哀を悲しみの記憶という分類に
                case MemoryType.NegativeMemory.Tragic:
                case MemoryType.NegativeMemory.Sorrowful:
                    memoryClassification = MemoryType.MemoryClassification.Sad;
                    break;
                //絶望と不安感をつらい生きおくという分類に変える
                case MemoryType.NegativeMemory.Despair:
                case MemoryType.NegativeMemory.Uneasiness:
                    memoryClassification = MemoryType.MemoryClassification.Painful;
                    break;
                //トラウマと黒歴史は特殊な記憶という分類に
                case MemoryType.NegativeMemory.Trauma:
                case MemoryType.NegativeMemory.Darkhistory:
                    memoryClassification = MemoryType.MemoryClassification.Special;
                    break;
                default:
                    Debug.Log("設定されていない、または間違えている");
                    break;
            }
        }
        ///<summary>
        ///敵を味方にする。敵の攻撃部位のタグも変える
        ///</summary>
        public void TargetChange()
        {
            //味方にできないならば返す
            if (notCanFellow) return;
            //味方になっていない
            if (!onFellow)
            {
                if (followChara != null)
                {
                    onFellow = true;
                    //味方になった時のマテリアル変換
                    ChangeMaterial();
                    //味方のクラスをオンに
                    followChara.enabled = true;
                    //味方にステータスを引き継ぎ
                    followChara.StatusInitialization(attackP, defenceP, currentHP, currentMaxHP);
                    //自分をオフにする
                    this.enabled = false;
                }
            }
        }
        //ダメージ時の点滅
        private void DamageFlash()
        {
            //メッシュのレンダラーをすべて検索
            for (int i = 0; i < meshRenderer.Length; i++)
            {
                //キーがあるか
                if (enemyMaterial.ContainsKey(meshRenderer[i]))
                {
                    //あったらdotweenで色を赤と元で点滅させる
                    for (int j = 0; j < meshRenderer[i].materials.Length; j++)
                    {
                        //dotweenで色をアニメーション
                        meshRenderer[i].materials[j].DOColor(Color.red, damageColorTime).SetEase(Ease.OutFlash, flashCount, flashPeriod);
                    }
                }
            }
        }
        ///<summary>
        ///プレイヤーの弱点であった場合
        ///</summary>
        private int WeekDefeatEXP()
        {
            var week = player.WeekMemory;
            //自分の記憶の分類とプレイヤーの弱点が同じであった場合
            if (week == memoryClassification)
            {
                return 2;
            }
            return 1;
        }
        /// <summary>
        /// ダメージを与えられた時の処理
        /// </summary>
        /// <param name="attackPoint">攻撃力</param>
        /// <param name="damage">ダメージを与えてきたオブジェクト</param>
        /// <param name="playDamageAnim">ダメージアニメーションをするかどうか</param>
        /// <returns></returns>
        public virtual void ReceiveDamage(int attackPoint, WeaponDamageStock damage, bool playDamageAnim = true)
        {
            int receiveDamageP = 0;
            //ダメージを食らっていない
            if (!isDamage)
            {
                var week = player.WeekMemory;
                //相手の弱点だったら0.5倍
                mulWeekDefeat = week == memoryClassification ? 0.5f : 1.0f;
                //ダメージ計算
                float attackPercent = UnityEngine.Random.Range(attackPoint / 3, attackPoint / 1.5f);
                float damagePoint = ((attackPoint + attackPercent) - defenceP) * mulWeekDefeat;
                int minDamage = UnityEngine.Random.Range(10, 20);
                receiveDamageP = damagePoint <= 0.0f ? minDamage : Mathf.FloorToInt(damagePoint);
                //ダメージ設定
                isDamage = true;
                NonTargettingAttackReceive(damage);
                TakeDamage(receiveDamageP, playDamageAnim);
                //ダメージテキスト位置をランダムに(単調にならないように)
                float randomX = UnityEngine.Random.Range(-0.25f, 0.25f);
                var obj = Instantiate(damageUI, damageUITransform.position + (Vector3.right * randomX), Quaternion.identity);
                obj.GetComponent<DamageUI>().Damage = receiveDamageP;
                //ダメージを受けている最中にダメージを負わないようにしたものを解除
                Observable.Timer(TimeSpan.FromSeconds(damageSpanTime), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                {
                    isDamage = false;
                }).AddTo(this);
            }
        }
        public void ReceiveDamage(int attackPoint, bool playDamageAnim = true,
            MemoryType.MemoryClassification memoryType = MemoryType.MemoryClassification.None,
        bool isBodyHit = false)
        {
            int receiveDamageP = 0;
            //ダメージを食らっていない
            if (!isDamage)
            {
                var week = player.WeekMemory;
                //相手の弱点だったら0.5倍
                mulWeekDefeat = week == memoryClassification ? 0.5f : 1.0f;
                //ダメージ計算
                float attackPercent = UnityEngine.Random.Range(attackPoint / 3, attackPoint / 1.5f);
                float damagePoint = ((attackPoint + attackPercent) - defenceP) * mulWeekDefeat;
                int minDamage = UnityEngine.Random.Range(10, 20);
                receiveDamageP = damagePoint <= 0.0f ? minDamage : Mathf.FloorToInt(damagePoint);
                //ダメージ設定
                isDamage = true;
                TakeDamage(receiveDamageP, playDamageAnim);
                //ダメージテキスト位置をランダムに(単調にならないように)
                float randomX = UnityEngine.Random.Range(-0.25f, 0.25f);
                var obj = Instantiate(damageUI, damageUITransform.position + (Vector3.right * randomX), Quaternion.identity);
                obj.GetComponent<DamageUI>().Damage = receiveDamageP;
                //ダメージを受けている最中にダメージを負わないようにしたものを解除
                Observable.Timer(TimeSpan.FromSeconds(damageSpanTime), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                {
                    isDamage = false;
                }).AddTo(this);
            }
        }
        /// <summary>
        /// agentの速さが変わっていた場合、10秒後に元に戻る。(実験段階の拒絶の2つ目の小技)
        /// </summary>
        private void AgentSpeed()
        {
            //重力のアビリティが当たった場合
            if (isHitRefusal)
            {
                refusalEffect.transform.position = transform.position;
                refusalEffect.SetActive(true);
                //スピード保持
                if (!startAgentKeep)
                {
                    //重力のアビリティを食らった場合、速さが半分に
                    agent.angularSpeed = startAgentangularSpeed / 2;
                    agent.acceleration = startAgentacceleration / 2;
                    agent.speed = startAgentspeed / 2;
                    startAgentKeep = true;
                }
                //限界まで遅くする
                if (agent.speed > limitAgentspeed || agent.angularSpeed > limitAgentangularSpeed || agent.acceleration > limitAgentacceleration)
                {
                    //speedChangeTimeごとにagentのスピードを減らす
                    speedChangeTime += Time.deltaTime;
                    if (speedChangeTime > speedChangeSpan)
                    {
                        speedChangeTime = 0;
                        agent.speed -= 0.1f;
                        agent.angularSpeed -= 0.1f;
                        agent.acceleration -= 0.1f;
                    }
                }
                //アビリティが終わった場合
                if (!specialityController.IsSpecialityValid[(int)Attach.Speciality.Refusal])
                {
                    //拒絶のエフェクトを非表示に
                    refusalEffect.SetActive(false);
                    //速度をもとに戻す
                    agent.speed = startAgentspeed;
                    agent.angularSpeed = startAgentangularSpeed;
                    agent.acceleration = startAgentacceleration;
                    isHitRefusal = false;
                    startAgentKeep = false;
                }
            }
        }

        /// <summary>
        /// 記憶転換を使った場合
        /// </summary>
        private void AnuvisGravity()
        {
            //が取得出来ている、アヌビスの有効範囲内にいる
            if (inAnuvisArea && anuvis != null)
            {
                Vector3 vec_direction = anuvis.transform.position - transform.position;
                if (anuvis_rb == null)
                    anuvis_rb = anuvis.GetComponent<Anuvis>().Rigid;
                //剣に引き寄せられる
                //万有引力の計算
                Vector3 Univ_gravity = (rigid.mass * anuvis_rb.mass) * G * vec_direction.normalized / Mathf.Pow(vec_direction.magnitude,3);
                //アヌビスに引き寄せられる感を出す。
                refusalEffect.SetActive(true);
                rigid.AddForce(Univ_gravity * anuvisPower);
            }
            //記憶転換の有効時間が終わった時
            if (specialityController.SpecialityValidTime[(int)Attach.Speciality.Convert] <= 0)
            {
                //アヌビスと範囲内にいるかどうかを初期化
                anuvis = null;
                inAnuvisArea = false;
                //拒絶エフェクトを非表示
                refusalEffect.SetActive(false);
            }
        }

        /// <summary>
        /// 必要なメソッド
        /// </summary>
        private void NecessaryMethod()
        {
            if (!isDeath)
            {
                AgentSpeed();
                AnuvisGravity();
            }
        }
        /// <summary>
        /// プレイヤーの弱点であれば大きくする
        /// </summary>
        /// <param name="mulScale">弱点を拡大</param>
        private void PlayerWeekScale(float mulScale = 1.2f)
        {
            //敵がプレイヤーの弱点であった場合、少し大きくなる
            if(memoryClassification == player.WeekMemory)
            {
                transform.localScale = beforeScale * mulScale;
            }
            else
            {
                //前のサイズに戻る
                transform.localScale = beforeScale;
            }
        }

        /// <summary>
        /// ダメージ処理メソッド
        /// </summary>
        /// <param name="damage">受けたダメージ量</param>
        /// <param name="playDamageAnim">ダメージのアニメーションを再生するか</param>
        protected virtual void TakeDamage(int damage,bool playDamageAnim)
        {
            //体力ゲージに反映
            enemyGauge.GaugeReduction(damage, currentHP, currentMaxHP);
            //体力を減少
            currentHP -= damage;
            //ダメージ
            audioSourceManager.PlaySE(damageSE);
            DamageParticlePlay(damageTransform.position);
            //点滅する
            DamageFlash();
            //ダメージを受けた時アニメーションするか
            if (playDamageAnim)
            {
                //ダメージのアニメーション
                animator.SetTrigger(damageAnim);
            }
            if (currentHP <= 0)
            {
                isDeath = true;
                DeathState();
            }
        }
        /// <summary>
        /// 死亡した時の処理
        /// </summary>
        protected virtual void DeathState()
        {
            //　敵のHPがなくなったらゲームオブジェクトの削除とラグドールを召喚し、飛ばす方向を設定
            if (isDeath)
            {
                animator.SetTrigger(deathAnim);
                //スポーンする場所
                var pos = new Vector3(transform.position.x, transform.position.y + deathSpawnY, transform.position.z);
                //　敵を倒した時にラグドールのプレハブをインスタンス化
                var ragdoll = Instantiate(enemyRagdoll, pos, transform.rotation);
                //メッシュを徐々に透明化するクラスを取得
                var meshFade = ragdoll.GetComponent<EnemyLifeBuildSpawn>();
                if (meshFade != null)
                {
                    //出てくる結晶体に割り当てる記憶の種類
                    meshFade.LifeBuildInMemory = memory;
                    //プレイヤーの弱点が自分の記憶の分類であった場合
                    int mul = WeekDefeatEXP();
                    //2倍になる
                    meshFade.KeepEXP = exp * mul;
                }
                Destroy(gameObject);
            }
        }
        /// <summary>
        /// 負の記憶の種類によって変化するステータス
        /// </summary>
        private void MemoryStatus()
        {
            //負の記憶の分類
            switch(memoryClassification)
            {
                //壮絶な記憶なら攻撃力
                case MemoryType.MemoryClassification.Spectacular:
                    attackP += 20;
                    break;
                //悲しみの記憶なら防御力
                case MemoryType.MemoryClassification.Sad:
                    attackP += 20;
                    break;
                //つらい記憶なら体力
                case MemoryType.MemoryClassification.Painful:
                    currentMaxHP += 50;
                    currentHP = currentMaxHP;
                    break;
                //特殊な記憶なら経験値
                case MemoryType.MemoryClassification.Special:
                    enemyStatus.startEXP += 100;
                    break;
                default:
                    Debug.Log("負の記憶の分類に想定していないものが代入されている");
                    break;
            }
        }
        
//----------------
        /// <summary>
        /// プレイヤーの味方になった時の色
        /// </summary>
        private void ChangeMaterial()
        {
            //変数を連想配列に
            var fellowmaterial = fellowMaterial.GetTable();
            //メッシュの配列
            for (int i = 0; i < meshRenderer.Length; i++)
            {
                //連想配列の中にキーと一致するものがあれば
                if (fellowmaterial.ContainsKey(meshRenderer[i]))
                {
                    //味方になった時のマテリアルに変更
                    meshRenderer[i].materials = fellowmaterial[meshRenderer[i]];
                }
            }
            
        }
        /// <summary>
        /// ステータス初期化
        /// </summary>
        protected virtual void StatusInitialize()
        {
            //付与される記憶の割合
            var divPercent = 0.5f;
            float currentLevel = curLevel < maxLevel ? curLevel / (float)maxLevel : 1;
            //攻撃力
            attackP = Mathf.CeilToInt(enemyStatus.attackP * currentLevel);
            //防御力
            defenceP = Mathf.CeilToInt(enemyStatus.defenceP * currentLevel);
            //体力
            currentMaxHP = Mathf.CeilToInt(enemyStatus.maxHP * currentLevel);
            //差分の半分を増やす
            var between = curLevel - playerLevel;
            exp = between == 0 ? enemyStatus.startEXP : enemyStatus.startEXP + Mathf.FloorToInt((enemyStatus.startEXP * between) / divPercent);
            currentHP = currentMaxHP;
            MemoryStatus();
            levelText.text = "LV. " + curLevel.ToString();
        }
        //ダメージパーティクル再生
        private void DamageParticlePlay(Vector3 relativePoint,float stopTime = 0.5f)
        {
            damageParticle.transform.position = relativePoint;
            if (damageParticle.isStopped)
            {
                damageParticle.Play();
            }
            Observable.Timer(TimeSpan.FromSeconds(stopTime), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
            {
                damageParticle.Stop();
            }).AddTo(this);
            
        }
        protected virtual void OnCollisionEnter(Collision other)
        {
           
        }
    }
}
