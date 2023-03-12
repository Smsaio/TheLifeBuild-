using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UniRx;
using System.IO;
//自分が設定した名前空間
using EnemySpace;
using Ability;
using GameManagerSpace;
using Zenject;

/*
//performedは離れた時、startedは押された時、canceledは離れた時
interactionsというものを加えると上のではなく、機能独自のものになる。
*/
//[Conditional("UNITY_EDITOR")] エディター上でのみしたいものにつける
//blurのマテリアルがURPに適応出来ていないので、中止
namespace PlayerSpace
{
    /// <summary>
    /// プレイヤーの基底クラス
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerSpecialityController))]
    public class Player : Character,IDamageble
    {
        #region public・Serialize  

        #region public 
        [Header("プレイヤー基礎")]
        #endregion
        #region Serialize
        // 地面判定に使うレイヤー
        [Header("地面判定関係"),SerializeField] protected LayerMask groundLayers;
        //壁で重力技が敵に利かないようにする時のレイヤー
        [SerializeField] protected int wallLayer = 12;
        
        [Header("レベルや体力系")]
        // 最初の経験値(これが重要で成長の速さを決める。)
        [SerializeField, Header("最初の経験値")] protected int startExp = 10;
        //最大のレベル
        [SerializeField] protected int maxLevel = 100;
        //プレイヤーの動きに関する管理基底クラス
        [SerializeField] private PlayerMove playerMove;
        [Header("エフェクト")]
        //ステータスが上がってる時のエフェクト
        [SerializeField] private GameObject statusUPEffect;
        //ステータスアップ音
        [SerializeField] private AudioClip statusUPSE;
        //ヒットエフェクト
        [SerializeField] protected ParticleSystem hitEffect;
        //ヒットエフェクトが出る位置
        [SerializeField] protected Transform damageTransform;
        //スクリーム状態で出るパーティクル
        [SerializeField] private GameObject screamParticle;
        //ライフビルド入手エフェクト
        [SerializeField] private ParticleSystem lifeBuildGetEffect;
        //体力ゲージのクラス
        [SerializeField] private HPGauge playerGauge;
        //自分の弱点の記憶分類
        [SerializeField] private MemoryType.MemoryClassification weekMemory = MemoryType.MemoryClassification.Painful;
        public MemoryType.MemoryClassification WeekMemory { get { return weekMemory; } }

        #endregion
        #endregion
        #region プロパティ

        //ライフビルドで得られる攻撃力
        protected int plusAttackP = 0;
        public int PlusAttackP { get { return plusAttackP; } set { plusAttackP = value; } }
        //ライフビルドで得られる防御力
        protected int plusDefenceP = 0;
        public int PlusDefenceP { get { return plusDefenceP; } set { plusDefenceP = value; } }

        //ライフビルドで得られる体力
        protected int plusHP = 0;
        public int PlusHP { get { return plusHP; } set { plusHP = value; } }

        //スクリーム状態
        private bool isScream = false;
        public bool IsScream { set { isScream = value; } get { return isScream; } }
        //ダメージを食らってない
        protected bool isDamage = false;
        public bool IsDamage { get { return isDamage; } }

        //リバースを使った場合
        protected bool isReverce = false;
        public bool IsReverce { get { return isReverce; } }

        //レベルアップしているとき
        private bool isLevelUP = false;
        public bool IsLevelUP { get { return isLevelUP; } }

        //現在のレベル
        protected int curLevel = 1;
        public int CurrentLevel { get { return curLevel; } }

        //回避している時の無敵期間
        private bool isInvinsibility = false;
        public bool IsInvinsibility { get { return isInvinsibility; } set { isInvinsibility = value; } }
        protected Animator anim;
        public Animator Animator { get { return anim; } }

        //2レベル以上レベルが上がるとき
        protected bool isLevelUPContinue = false;
        public bool IsLevelUPContinue { get { return isLevelUPContinue; } }

        //死亡した
        protected bool isDown = false;
        public bool IsDown { get { return isDown; } }

        //カメラを回転するかどうか
        private bool isCameraRot = false;
        public bool IsCameraRot { get { return isCameraRot; } set { isCameraRot = value; } }

        //今の経験値
        protected int curExp;
        public int CurrentEXP { get { return curExp; } }
        #endregion
        #region protected

        //　接地確認
        protected bool isGround = false;
        protected Rigidbody rb;
        //ジャンプした
        protected bool isJump = false;
        //コンティニューした場合
        protected bool isContinue = false;
        //まだ死んだアニメーションをしていない
        protected bool nonDownAnim = false;

        //　HPを一度減らしてからの経過時間
        protected float countTime = 0f;
        //攻撃を食らった後の少しの無敵時間
        protected float invinsibilityTime = 0.2f;

        //移動するための速度
        protected Vector3 velocity;
        // 最高速度
        protected float moveMaxSpeed = 35f;

        // 元の回転
        protected Quaternion beforeRot;
        //前フレームの位置
        protected Vector3 beforePos;
        //プレイヤーのインプットシステム使用
        protected PlayerInputAction playerInputs;
        //アイテムの中の逆時間の有効時間
        protected float reverceTime = 0;
        //逆時間の最高時間
        protected float maxReverceTime = 10f;
        // レベルごとの経験値
        protected int[] totalEXP = new int[100];
        //レベルアップ前のレベル
        protected int prevLevel = 0;
        //獲得した経験値
        protected int getExp;
        // 前のレベルに必要だった経験値
        protected int prevNeedExp = 0;
        // 次のレベルに必要な経験値
        protected int needExp = 0;
        public int NeedEXP { get { return needExp; } }
        //次のレベルに至るよりも多く経験値をもらっていた場合
        protected int overExp = 0;
        //累計経験値
        protected int totalExp = 0;
        //　次に経験値を増やすまでの時間
        protected float nextCountTime = 0.1f;

        //アニメーションの名前ハッシュ
        protected int deathAnimName = Animator.StringToHash("Death") ;
        protected int pickUPAnimName = Animator.StringToHash("PickUP");
        #endregion
        #region private

        //完全に経験値を獲得したか
        private bool isGetEXP = false;
        //ゲームマネージャー制御クラス
        protected IGameManager gameManager = default;
        //音制御クラス
        protected IAudioSourceManager audioSourceManager = default;
        [Inject]
        public void Construct(IGameManager IgameManager,IAudioSourceManager IaudioSourceManager,IRole Irole)
        {
            gameManager = IgameManager;
            audioSourceManager = IaudioSourceManager;
            role = Irole;
        }
        #endregion
        private void OnEnable()
        {
            
        }
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Update()
        {
            if (gameManager.IsStageClear || gameManager.IsGameOver || Mathf.Approximately(Time.timeScale, 0)) return;            
            GetExp();
            ReverceTime();
        }
        //ダメージを体力に反映
        public void DamageHPGauge(int damagePoint = 0)
        {
            playerGauge.GaugeReduction(damagePoint, currentHP, currentMaxHP);
        }
        /// <summary>
        /// 経験値表
        /// </summary>
        public virtual void EXPInitialize(StreamWriter sw)
        {
            var exp = startExp;
            for (int i = 0; i < maxLevel; ++i)
            {
                /*
                経験値A＝前のレベルでレベルアップに必要だった経験値×1.1
                経験値B＝今のレベル×15
                レベルアップに必要な経験値＝(経験値A＋経験値B)÷2
                */
                sw.WriteLine(exp.ToString());
                totalEXP[i] = Mathf.CeilToInt(exp);
                exp = Mathf.CeilToInt(exp * 1.1f);
                /*var secondEXP = exp * 3;
                exp = (exp + secondEXP) / 2;*/
            }
            sw.Flush();
            sw.Close();
        }

        
        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialization()
        {
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            if(playerMove == null)
            {
                playerMove = GetComponent<PlayerMove>();
            }
            StreamWriter sw;
            //レベル経験値一覧表があるとき
            if (File.Exists("LevelEXP.txt"))
            {
                File.Delete("./Assets/MyAssets/Scripts/File/LevelEXP.txt");
            }
            //ない時
            else if (!File.Exists("LevelEXP.txt"))
            {
                sw = new StreamWriter("./Assets/MyAssets/Scripts/File/LevelEXP.txt", false);
                EXPInitialize(sw);
            }
            needExp = totalEXP[0];
            screamParticle.gameObject.SetActive(false);
            lifeBuildGetEffect.gameObject.SetActive(false);
        }
        ///<summary>
        ///ステータス初期化
        ///</summary>
        public void StatusInitialization(PlayerParamater paramater)
        {
            if (curLevel <= maxLevel)
            {
                float currentLevel = curLevel < maxLevel ? curLevel / (float)maxLevel : 1;
                //攻撃力
                attackP = Mathf.CeilToInt(paramater.maxAttackPoint * currentLevel) + plusAttackP;
                //防御力
                defenceP = Mathf.CeilToInt(paramater.maxDefencePoint * currentLevel) + plusDefenceP;
                //体力初期化
                //体力
                currentMaxHP = Mathf.CeilToInt(paramater.maxHP * currentLevel) + plusHP;
                currentHP = currentMaxHP;
                playerMove.MoveSpeed = paramater.moveSpeed;
                //現在の体力を適応
                DamageHPGauge(0);
            }
        }

        /// <summary>
        /// バフは、ダメージを減らされたら戻る
        /// </summary>
        private void StatusReturn()
        {
            //攻撃力、防御力が上昇していて、かつ体力が減らされた
            if (plusAttackP > 0 || plusDefenceP > 0)
            {
                //増えたステータスをもとに戻す
                plusAttackP = 0;
                plusDefenceP = 0;
            }
        }

        ///<summary>
        ///レベルが変化した場合、ゲームマネージャーに入れる。
        /// </summary>
        private void LevelSet()
        {
            //役割に割り当てられた番号に応じて各役割のレベルを配列に入れる
            role.CurrentPlayerLevel.Value = curLevel;
        }

        ///<summary>
        ///経験値設定
        ///</summary>
        public void SetExp(int SetExp)
        {
            float banishTime = 1.5f;
            if (curLevel <= maxLevel)
            {
                //取得したアニメーション
                anim.SetTrigger(pickUPAnimName);
                lifeBuildGetEffect.gameObject.SetActive(true);
                lifeBuildGetEffect.Play();
                Observable.Timer(TimeSpan.FromSeconds(banishTime), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                {
                    lifeBuildGetEffect.Stop();
                    lifeBuildGetEffect.gameObject.SetActive(false);
                }).AddTo(this);
                //経験値取得
                getExp += SetExp;
                //次のレベルへの経験値の残りをオーバーしている経験値を保持する(オーバーしていなかったらそのまま)
                overExp = 0;
                //数えるまでの時間
                countTime = 0f;
                //経験値を取得した
                isGetEXP = true;
            }
        }

        ///<summary>
        ///経験値獲得処理
        ///</summary>
        private void GetExp()
        {
            //獲得した経験値が0ではないとき
            if (getExp > 0)
            {
                countTime += Time.deltaTime;
                //　次に変化する時間がきたら
                if (countTime >= nextCountTime)
                {
                    countTime = 0f;
                    //　ダメージ量を10で割った商を増やす
                    var tempExp = getExp / 10;
                    //　商が0になったら余りを増やす
                    if (tempExp == 0)
                    {
                        tempExp = getExp % 10;
                    }
                    curExp += tempExp;
                    if (!isLevelUPContinue)
                        overExp += tempExp;
                    getExp -= tempExp;
                    if (curExp >= needExp)
                    {
                        prevLevel = curLevel;
                        // レベルアップする
                        curLevel++;
                        Debug.Log("レベルが上がりました。");
                        //音を鳴らす
                        audioSourceManager.PlaySE(statusUPSE);
                        //ステータス対応
                        StatusInitialization(role.PlayerParamaters[role.RoleNumber]);
                        //レベルを保持する
                        LevelSet();
                        //レベルアップ中
                        isLevelUP = true;
                        //経験値初期化
                        curExp = 0;
                        countTime = 0f;
                        //現在レベルまでに得た累計経験値
                        prevNeedExp += needExp;
                        // 次のレベルアップに必要な経験値を計算する
                        needExp = totalEXP[curLevel - 1];
                    }
                }
            }
            else if (getExp <= 0)
            {
                totalExp = overExp;
                if (isGetEXP)
                {
                    StatusInitialization(role.PlayerParamaters[role.RoleNumber]);
                }
                //まだレベルアップの余地があるかどうかを調べるもの
                int over = needExp - curExp;
                if (over < 0)
                {
                    Debug.Log("経験値" + -(needExp - curExp) + "オーバー");
                    isLevelUPContinue = true;
                    getExp -= over;
                }
                else
                {
                    isLevelUP = false;
                    isLevelUPContinue = false;
                    isGetEXP = false;
                }
            }
        }

//-----------特徴------------
        private void StatusUP()
        {
            if(!isLevelUP)
                audioSourceManager.PlaySE(statusUPSE);
            statusUPEffect.SetActive(true);
            Observable.Timer(TimeSpan.FromSeconds(2.0f), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
            {
                statusUPEffect.SetActive(false);
            }).AddTo(this);
        }
        public void Heal(int healPoint)
        {
            currentHP += healPoint;
            StatusUP();
        }
        /// <summary>
        /// 攻撃力が上がる
        /// </summary>
        public void PointUP(int plus, ItemType.ITEMTYPE itemType)
        {
            switch (itemType) 
            {
                case ItemType.ITEMTYPE.PowerUP:
                    plusAttackP += plus;
                    StatusUP();
                    break;
                case ItemType.ITEMTYPE.DefenceUP:
                    plusDefenceP += plus;
                    StatusUP();
                    break;
                case ItemType.ITEMTYPE.Reverce:
                    UseReverce();
                    StatusUP();
                    break;
                case ItemType.ITEMTYPE.Heal:
                    Heal(plus);
                    break;
            }
        }

        /// <summary>
        /// ダメージが回復になる
        /// </summary>
        private void ReverceTime()
        {
            //逆時間の特技を使用したか、逆時間出来るか
            if (isReverce)
            {
                //リバースを使った場合使用回数が最大に達していないとき
                reverceTime += Time.deltaTime;
                if (reverceTime >= maxReverceTime)
                {
                    reverceTime = 0;
                    isReverce = false;
                }
            }
        }
        //逆時間を使用
        public void UseReverce()
        {
            isReverce = true;
        }
        
        //-----コントローラー系------
        
        //---------状態系--------

        /// <summary>
        /// やられた時の処理 
        /// </summary>
        /// <param name="attackPoint">食らったダメージ量</param>
        /// <param name="memoryType">食らった敵の記憶の分類</param>
        /// <param name="isBodyHit">敵に接触したとき</param>
        public virtual void ReceiveDamage(int attackPoint,  bool playDamageAnim = true,
            MemoryType.MemoryClassification memoryType = MemoryType.MemoryClassification.None,bool isBodyHit = false)
        {
            int damagePoint;
            //ダウン中や無敵中、ダメージを負った後などはダメージを受けない
            if (isDown || isInvinsibility || isDamage || gameManager.IsGameOver)
            {
                return;
            }
            else
            {
                //攻撃でダメージを与えられた
                if (!isBodyHit)
                {
                    //防御力にランダムにプラスする
                    float defencePercent = UnityEngine.Random.Range(defenceP / 5, defenceP / 4);
                    float damage = attackPoint - (defencePercent + defenceP);
                    int minDamage = UnityEngine.Random.Range(7, 20);
                    damagePoint = damage <= 0.0f ? minDamage : Mathf.FloorToInt(damage);
                }
                else
                {
                    //接触ダメージのときは敵に設定した接触ダメージがそのまま与えられる
                    damagePoint = attackPoint;
                }
                //逆時間中のダメージも考慮
                damagePoint = isReverce ? -Mathf.CeilToInt(damagePoint  / 3) : damagePoint;
                isDamage = true;
                if (isDamage)
                {
                    //攻撃中ではないか
                    if (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                    {
                        //体力はまだあるか
                        if (currentHP > 0)
                        {
                            //ダメージエフェクト
                            var damageEffect = Instantiate(hitEffect.gameObject,damageTransform.position,Quaternion.identity); 
                            Destroy(damageEffect,0.3f);
                            StatusReturn();
                            playerMove.DamageAnimation();
                            
                            //敵の分類が弱点だった場合、2倍になる
                            int weekMul = 1;
                            if (weekMemory == memoryType)
                            {
                                weekMul = 2;
                            }
                            damagePoint *= weekMul;
                            //ダメージを受けた時は止まる
                            rb.velocity = Vector3.zero;
                            //体力バーへの反映体力を減らした後にさらに減らす形になってしまうため、
                            //必ず体力を減らす前に体力ゲージに反映させなければならない
                            DamageHPGauge(damagePoint);
                            //体力が最大
                            if (currentHP > 0 && currentHP <= currentMaxHP)
                            {
                                currentHP -= damagePoint;
                            }
                            //最大以上であれば最大に
                            if (currentHP > currentMaxHP)
                            {
                                currentHP = currentMaxHP;
                            }
                            //0以下であれば0に
                            if(currentHP <= 0)
                            {
                                currentHP = 0;
                            }
                            Observable.Timer(TimeSpan.FromSeconds(invinsibilityTime), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                            {
                                isDamage = false;
                            }).AddTo(this);
                        }
                    }
                    //体力が無くなったら
                    if (currentHP <= 0)
                    {
                        isDamage = false;
                        if (!isDown)
                        {
                            anim.SetTrigger(deathAnimName);
                            isDown = true;
                        }
                        //死亡処理とゲームオーバーへの移行
                        gameManager.SetGameMode(GameMode.GameOver);                        
                        plusHP = 0;
                    }
                }
                else
                {
                    nonDownAnim = true;
                }
            }
        }
        public void ReceiveDamage(int attackPoint, WeaponDamageStock damage, bool playDamageAnim = true)
        {

        }
        /// <summary>
        /// ボスの叫び攻撃を食らった場合
        /// </summary>
        /// <param name="screamTime">スクリーム状態の有効時間</param>
        public void HitScream(float screamTime = 3.0f)
        {
            isScream = true;
            rb.velocity = Vector3.zero;
            //スクリーム状態を表すパーティクルをつける
            screamParticle.gameObject.SetActive(true);
            Ray ray = new Ray(screamParticle.transform.position, -transform.up);
            //パーティクルを地面に
            if(Physics.Raycast(ray,out RaycastHit hit, groundLayers))
            {
                screamParticle.transform.position = hit.point;
            }
            //一定時間後に非表示
            Observable.Timer(TimeSpan.FromSeconds(screamTime), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
            {
                //スクリーム状態から脱出
                isScream = false;
                //スクリーム状態を表すパーティクルを消す
                screamParticle.gameObject.SetActive(false);
            }).AddTo(this);
        }
        /// <summary>
        /// コンティニュー待機状態か
        /// </summary>
        /// <returns></returns>
        public bool IsContinueWaiting()
        {
            //ゲームオーバーしているか
            if (gameManager.IsGameOver)
            {
                return false;
            }
            else
            {
                //死亡アニメーションが完了しているか
                return IsDownAnimEnd() || nonDownAnim;
            }
        }
        /// <summary>
        /// ダウンアニメーションが完了しているかどうか
        /// </summary>
        private bool IsDownAnimEnd()
        {
            if (isDown && anim != null)
            {
                //現在のアニメーションが死亡アニメs－本である場合
                AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
                if (currentState.IsName("Death"))
                {
                    //死亡アニメーションが終わった場合
                    if (currentState.normalizedTime >= 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// コンティニューする、コンティニューボタンが押された場合
        /// </summary>
        public void ContinuePlayer()
        {
            isDown = false;
            isJump = false;
            velocity = Vector3.zero;
            isDamage = false;
            isContinue = true;
            nonDownAnim = false;
        }


        //-----コリジョン系----
        private void OnCollisionEnter(Collision col)
        {            
            //ポーズしているときはダメージを受けない
            if (!Pauser.isPause)
            {
                //敵に接触したら接触ダメージを食らう
                if (col.collider.CompareTag("Enemy") || col.collider.CompareTag("Boss"))
                {
                    var enemyBase = col.collider.GetComponent<EnemyBase>();
                    if (enemyBase != null)
                    {
                        int enemyBodyDamage = enemyBase.BodyDamage;
                        //逆時間中ではないか、レベルアップしている時ではないか
                        if (!isReverce && !isLevelUP)
                        {
                            //敵に接触した接触ダメージ
                            ReceiveDamage(enemyBodyDamage,true, enemyBase.MemoryClassification);
                        }
                    }
                }
            }
        }
    }
}