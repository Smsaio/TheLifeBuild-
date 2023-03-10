using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EnemySpace;
using PlayerSpace;
using TMPro;
using Zenject;
using UniRx;
using GameManagerSpace;

/// <summary>
/// 味方になった時の管理クラス
/// </summary>
public class FollowChara : Character,IDamageble,ITargetSearch
{
    //ダメージ処理
    [SerializeField] private Transform damageTransform;
    [SerializeField] private GameObject damageUI;
    //記憶のテキスト変換
    [SerializeField] private TextMeshProUGUI memoryText;
    //攻撃のポイント
    [SerializeField] private WeaponDamageStock attackPoint;
    //味方の体力ゲージ
    [SerializeField] private HPGauge followGauge;
    //キャラクターの型
    [SerializeField] private CharacterType characterType = CharacterType.Fellow;
    [SerializeField] private SearchCharacter searchCharacter;
    //ミニマップののアイコン
    [Header("ミニマップ用のアイコン(味方になったら色を変える)"), SerializeField] private Renderer minimapIcon;

    //味方になった時のアイコン(アイコン)
    [Header("味方になった時のアイコン")]
    public Material fellowIconMaterial;
    //攻撃を行う部位
    [Header("味方になった時に攻撃部位のタグを変える")] public Transform[] attackTransform;
    public CharacterType MyCharacterType { get { return characterType; } }
    //攻撃をする範囲
    [Range(1.0f, 6.0f)] public float attackDistanceThreshold = 4.0f;
    //負の記憶の反対(いい記憶)
    private MemoryType.GoodMemory goodMemory = MemoryType.GoodMemory.Hope;
    public MemoryType.GoodMemory fellowGoodMemory { get { return goodMemory; } set { goodMemory = value; } }
    //味方の要素番号(集合した時に要素番号を参照するため)
    private int fellowCount = 0;
    public int MyFellowCount { set { fellowCount = value; } get { return fellowCount; } }

    private bool targetFind = false;
    public bool IsTargetFind { get { return targetFind; } set { targetFind = value; } }
    //自分の敵だったクラス
    private EnemyBase enemyBase;

    private EnemyBase targetEnemy;
    public EnemyBase Enemy { get { return targetEnemy; } set { targetEnemy = value; } }
    //AIのターゲット
    private Transform target;
    public Transform Target { get { return target; } set { target = value; } }
    //プレイヤーからの距離
    private float followDistance = 1.5f;
    //死んだときに吹っ飛ばされる力
    private float powerToMove = 0.1f;
    private NavMeshAgent agent;
    private Animator animator;
    //食らったダメージ
    private int damageP = 0;
    private bool isDeath = false;
    //死んだときの正面
    private Vector3 dieDirection;
    //プレイヤー参照
    private PlayerSpecialityController specialityController;
    private Transform playerTransform;
    //死んだ後のラグドール
    private GameObject enemyPrefab;
    private int animWalkSpeedHash = Animator.StringToHash("WalkSpeed");
    private int animDamageHash = Animator.StringToHash("Damage");
    private int animDeathHash = Animator.StringToHash("Death");
    private int animAttackHash = Animator.StringToHash("Attack");
    //敵発見時の速さ
    private float agentFindSpeed = 5.0f;
    //ラグドールオブジェクト
    private GameObject ragdoll;    
    //元の速度保管
    private float agentSpeed = 0.0f;
    //ダメージを受けた
    private bool isDamage = false;
    //プレイヤーのカプセル
    private float targetCollisionRadius = 0.0f;
    //自分のコリジョンの半径
    private float myCollisionRadius = 0.0f;
    void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        enemyPrefab = enemyBase.EnemyRagDoll;
        this.enabled = false;
    }

    public void Start()
    {        
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        attackPoint.enabled = true;
        myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        agentSpeed = agent.speed;
        //攻撃箇所
        for (int i = 0; i < attackTransform.Length; i++)
        {
            attackTransform[i].tag = "PlayerAttack";
        }
        //アイコンのマテリアルを変える
        minimapIcon.material = fellowIconMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDeath)
        {
            TargetToWalk();
            Damage();
        }
    }
    public void TargetFind(GameObject obj)
    {
        //索敵した敵をターゲットに設定
        SetTarget(obj);
    }
    public void TargetLost()
    {
        targetFind = false;
    }
    /// <summary>
    /// 敵のステータス設定
    /// </summary>
    /// <param name="attackPoint">攻撃力</param>
    /// <param name="defencePoint">防御力</param>
    /// <param name="hp">体力</param>
    public void StatusInitialization(int attackPoint,int defencePoint,int hp,int maxHP,IGameManager IgameManager)
    {
        //攻撃力
        attackP = attackPoint;
        //防御力
        defenceP = defencePoint;
        //体力
        currentHP = hp;
        //最高体力
        currentMaxHP = maxHP;
        //ゲームマネージャー引継ぎ
        gameManager = IgameManager;
        int index = (int)Ability.Attach.Speciality.Convert;
        specialityController.SpecialityBases[index].AddFellow(gameObject);
        if (searchCharacter != null)
        {
            //索敵変更
            searchCharacter.TargetSearch = this;
        }
        //体力ゲージに設定
        followGauge.GaugeReduction(0, currentHP, currentMaxHP);
    }   
    /// <summary>
    /// ターゲット設定
    /// </summary>
    public void TargetToWalk()
    {
        if (animator != null)
        {
            if (!agent.isStopped)
            {
                animator.SetFloat(animWalkSpeedHash, agent.desiredVelocity.magnitude);
            }
            else
            {
                animator.SetFloat(animWalkSpeedHash, 0f);
            }
        }
        //敵を見つけていないとき
        if (!targetFind)
        {
            int index = (int)Ability.Attach.Speciality.Convert;
            //目的地を最初であればプレイヤーそれ以外では自分より一個前の味方を追う
            if(fellowCount == 0)
            {
                target = playerTransform;
            }
            else
            {
                //自分の前に味方はいるか
                if(specialityController.SpecialityBases[index].FollowCharaList[fellowCount - 1] != null)
                {
                    target = specialityController.SpecialityBases[index].FollowCharaList[fellowCount - 1].transform;
                }
            }
            agent.speed = agentSpeed;
            //自分がagentが動かなくなる範囲内にいないか

            agent.isStopped = agent.remainingDistance < followDistance;
            TargetSettings();
        }
        AttackMagnitude();
    }
    /// <summary>
    /// ダメージ処理
    /// </summary>
    private void Damage()
    {
        if (isDamage)
        {
            //ダメージテキスト召喚
            var obj = Instantiate(damageUI, damageTransform.position, Quaternion.identity);
            obj.GetComponent<DamageUI>().Damage = damageP;
            isDamage = false;
        }
    }
    /// <summary>
    /// 攻撃する距離になったら
    /// </summary>
    private void AttackMagnitude(float magnitude=1.6f)
    {
        //敵を発見しているとき
        //　攻撃する距離だったら攻撃
        if (targetFind)
        {
            //攻撃できる距離にいたら停止して攻撃
            if (agent.remainingDistance <= magnitude)
            {
                agent.velocity = Vector3.zero;
                agent.isStopped = true;
                animator.SetTrigger(animAttackHash);
                Debug.Log("攻撃");
            }
            else
            {
                agent.isStopped = false;
                agent.speed = agentFindSpeed;
            }
            if (targetEnemy != null)
            {
                //目標の敵が味方に変わったもしくは、死んだ
                if (targetEnemy.CurrentHP <= 0 || targetEnemy.OnFellow)
                {
                    //目標を見失った
                    targetFind = false;
                }
            }
            Debug.Log("敵追跡");
        }
    }
    /// <summary>
    /// 目的地設定
    /// </summary>
    public void TargetSettings()
    {
        if (target != null)
        {
            if (targetCollisionRadius == 0f)
                targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
            // 方向を求める
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            // directionToTarget * (自分の半径+ターゲットの半径)で、自分とターゲットの半径の長さ分の向きベクトルが求められる。
            Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
            if (agent != null)
            {
                agent.SetDestination(targetPosition);
            }
        }
    }
    /// <summary>
    /// 敵の索敵
    /// </summary>
    /// <param name="obj">目的のオブジェクト</param>
    public void SetTarget(GameObject obj)
    {
        //起動しているとき
        if (this.enabled)
        {
            //敵が索敵範囲に入ったかと敵をすでに見つけていないか
            if (obj.CompareTag("Enemy"))
            {
                targetEnemy = obj.GetComponent<EnemyBase>();
                //敵を取得出来た
                if (targetEnemy != null)
                {
                    //敵の体力がまだあるとき
                    if (targetEnemy.CurrentHP > 0)
                    {
                        //その敵が味方に変わっていないとき
                        if (!targetEnemy.OnFellow)
                        {
                            //敵を発見した
                            targetFind = true;
                            target = obj.transform;
                            Debug.Log("敵検知" + target);
                            Debug.Log(targetFind);
                            TargetSettings();
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// プレイヤーの方向に向く
    /// </summary>
    void OnAnimatorIK()
    {
        if (target != null)
        {
            var weight = Vector3.Dot(transform.forward, target.position - transform.position);

            if (weight < 0)
            {
                weight = 0;
            }
            animator.SetLookAtWeight(weight, 0.3f, 1f, 0f, 0.5f);
            animator.SetLookAtPosition(target.position + Vector3.up * 1.5f);
        }
    }
    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="attackPoint"></param>
    /// <param name="playDamageAnim"></param>
    public void ReceiveDamage(int attackPoint, WeaponDamageStock damage, bool playDamageAnim = true)
    {
        //ダメージ計算
        float attackPercent = UnityEngine.Random.Range(attackPoint / 3, attackPoint / 1.5f);
        float damagePoint = (attackPoint + attackPercent) - defenceP;
        int minDamage = UnityEngine.Random.Range(10, 20);
        damageP = damagePoint <= 0.0f ? minDamage : Mathf.FloorToInt(damagePoint);
        isDamage = true;
        TakeDamage(damageP);
    }
    public override void ReactivePlayer(IRole role)
    {
        base.ReactivePlayer(role);
        if (role == null) return;
        role.CurrentPlayerSpController.Subscribe(value => { specialityController = value; }).AddTo(this);
        role.CurrentPlayerTransform.Subscribe(value => { playerTransform = value; }).AddTo(this);
    }
    public void ReceiveDamage(int attackPoint,
        bool playDamageAnim = true, MemoryType.MemoryClassification memoryType = MemoryType.MemoryClassification.None,
        bool isBodyHit = false)
    {

    }
    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="damage">ダメージ</param>
    private void TakeDamage(int damage)
    {
        if (!isDeath)
        {
            followGauge.GaugeReduction(damage, currentHP, currentMaxHP);
            currentHP -= damage;
            animator.SetTrigger(animDamageHash);
        }
        //　敵のHPがなくなったらゲームオブジェクトの削除と飛ばす方向を設定
        if (currentHP <= 0)
        {
            int index = (int)Ability.Attach.Speciality.Convert;
            //味方消去
            specialityController.SpecialityBases[index].RemoveFellow(this);
            animator.SetTrigger(animDeathHash);
            //正面取得
            dieDirection = transform.root.forward;
            isDeath = true;
            //　敵を倒した時にラグドールのプレハブをインスタンス化
            ragdoll = Instantiate(enemyPrefab, transform.position, transform.rotation);
            //後ろに吹っ飛ばす
            ragdoll.GetComponent<Rigidbody>().AddForce(dieDirection * -powerToMove, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
}

