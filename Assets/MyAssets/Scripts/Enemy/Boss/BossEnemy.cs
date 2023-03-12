using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EnemySpace;
using System;
using UniRx;
using Ability;
using PlayerSpace;
using UnityEngine.UI;
using Zenject;
using TMPro;
using GameManagerSpace;

namespace EnemySpace
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class BossEnemy : EnemyBase
    {
        //攻撃の種類
        public enum AttackVariation : int
        {
            Tail = 1,//しっぽ攻撃
            Scream = 2,//怯ませる(一定時間移動不能)
            FireBall,//火炎球発射(通常攻撃よりも1.5倍の威力)
        }
        [Header("必殺技のファイアボール")]
        //奥義(ファイアボール)
        [SerializeField] private GameObject fireballObject;
        //ファイアボ－ル発射場所
        [SerializeField] private Transform fireBallTransform;
        //各攻撃の音
        [SerializeField] private AudioClip[] attackSE = new AudioClip[Enum.GetValues(typeof(AttackVariation)).Length];
        //チャージの時間
        [Header("今どのくらいチャージされているか")]
        [SerializeField] private Slider chargeSlider;
        [Header("スタンになった時")]
        //スタンになった時の音
        [SerializeField] private AudioClip stanSE;
        [Header("スタン状態でダメージを負った")]
        //スタン中のダメージ音
        [SerializeField] private AudioClip stanDamageSE;
        //チャージに至る割合
        [Range(1,9),Header("チャージになる割合"),SerializeField] private int chargePercent = 9;        
        [Header("必殺技の時の索敵サイズ"), Range(20, 40), SerializeField] private float deathBlowSearchColliderSize = 40;
        //攻撃した
        private bool doAttack = false;
        public bool DoAttack { get { return doAttack; } set { doAttack = value; } }
        //　経過時間
        private float elapsedTime;
        //　到着フラグ
        private bool arrival;
        //　SetPositionスクリプト
        private SetPosition setPosition = null;
        //奥義のゲージをチャージする間隔
        private const float chargeSpan = 0.3f;
        //チャージの時間
        private float chargeSpanTime = 0.0f;
        //チャージ中か
        private bool doCharge = false;
        //チャージ数
        private float chargeCount = 0;
        //スタン開始
        private bool isStan = false;
        //チャージの最高到達点
        private const int chargeMax = 5;
        //歩いている状態の時間
        private float walkTime = 6.0f;
        //立ち止まる時間
        private const float waitTime = 4.0f;
        //攻撃後の硬直
        private const float freezeAttackTime = 3.0f;
        //まわる速さ
        private const float rotateSpeed = 5;
        //追跡時の速さ
        private float agentFindSpeed = 4.5f;
        private float agentSpeed = 0.0f;
        //スタン時間
        private float stanStateTime = 0;
        private const float maxStanTime = 5.0f;
        //攻撃する距離
        private const float attackDis = 7.0f;
        private int stanAnim = Animator.StringToHash("Stan");
        private int chargeAnim = Animator.StringToHash("Charge");
        private int deathBlowAnim = Animator.StringToHash("DeathBlow");
        private int attackNumAnim = Animator.StringToHash("AttackNum");
        //攻撃バリエーション
        private AttackVariation attackVariation = AttackVariation.Tail;
        //ファイアボールの速さ
        private const float fireBallSpeed = 15.0f;

        protected override void Awake()
        {
            base.Awake();
        }
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            bodyDamage = 20;
            agentSpeed = agent.speed;
            //ボスのレベルはプレイヤーのレベルより(最高はあるが)15高い
            curLevel = (playerLevel + plusLevel) > maxLevel ? maxLevel : playerLevel + plusLevel;
            StatusInitialize();
            setPosition = GetComponent<SetPosition>();
            //最初の目的地設定
            setPosition.CreateRandomPosition();
            agent.SetDestination(setPosition.GetDestination());
            chargeSlider.value = 0;
            SetState(CharacterState.Move);
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (!isDeath)
            {
                StateMove();
            }
            else
            {
                SetState(CharacterState.Death);
            }
        }
        public override void TargetFind(GameObject obj)
        {
            //　敵キャラクターが追いかけられる状態であれば追いかけるに変更
            if (enemyState != CharacterState.Chase && enemyState != CharacterState.Freeze && enemyState != CharacterState.Charge
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
        /// <summary>
        /// 状態設定
        /// </summary>
        /// <param name="tempState">状態</param>
        /// <param name="targetObj">目的地</param>
        public override void SetState(CharacterState tempState, Transform targetObj = null)
        {
            if (searchCharacter != null)
            {
                if (tempState == CharacterState.DeathBlow)
                {
                    searchCharacter.ScaleCollider(deathBlowSearchColliderSize);
                }
                else
                {
                    searchCharacter.ScaleCollider(deathBlowSearchColliderSize * 0.5f);
                }
            }
            enemyState = tempState;
            //navmeshagentが発動したとき 
            if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
            {
                //agentの操作
                if (tempState == CharacterState.Move)
                {
                    arrival = false;
                    agent.speed = agentSpeed;
                    setPosition.CreateRandomPosition();
                    agent.SetDestination(setPosition.GetDestination());
                    agent.isStopped = false;
                }
                else if (tempState == CharacterState.Chase)
                {
                    //待機状態からの遷移の可能性があるため、falseにしてagentを起動させる
                    agent.isStopped = false;
                    //　待機状態から追いかける場合もあるのでOff
                    arrival = false;
                    //　追いかける対象をセット
                    target = targetObj;
                    agent.speed = agentFindSpeed;
                    agent.SetDestination(target.position);
                }
                else if (tempState == CharacterState.Charge || tempState == CharacterState.Stan)
                {
                    if (tempState == CharacterState.Stan)
                    {
                        animator.SetBool(stanAnim, true);
                        chargeCount = 0;
                        chargeSpanTime = 0;
                    }
                    else if(tempState == CharacterState.Charge)
                    {
                        animator.SetTrigger(chargeAnim);
                        if(!doCharge)
                            doCharge = true;
                        elapsedTime = 0;
                    }
                    arrival = true;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }
                else if (tempState == CharacterState.DeathBlow)
                {
                    animator.SetTrigger(deathBlowAnim);
                    DeathBlowState();
                    arrival = true;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }
                else if (tempState == CharacterState.Wait)
                {
                    elapsedTime = 0f;
                    //到着した
                    arrival = true;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = false;
                    animator.SetFloat(animWalkSpeed, 0f);
                }
                else if (tempState == CharacterState.Attack)
                {
                    Attack();
                    if (!doAttack)
                    {
                        doAttack = true;
                        animator.SetInteger(attackNumAnim, (int)attackVariation);
                        animator.SetTrigger(attackAnim);
                    }
                    arrival = true;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }
                else if (tempState == CharacterState.Freeze)
                {
                    elapsedTime = 0f;
                    arrival = true;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                    animator.SetFloat(animWalkSpeed, 0f);
                }
                else if (tempState == CharacterState.Damage)
                {
                    animator.SetTrigger(damageAnim);
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }
                else if (tempState == CharacterState.Death)
                {
                    animator.SetTrigger(deathAnim);
                    agent.isStopped = true;
                    Destroy(gameObject);
                }
            }
        }
        /// <summary>
        /// ボス戦の時の戦闘
        /// </summary>
        protected override void StateMove()
        {
            //ボスを倒したことでクリア
            if (enemyState == CharacterState.Death) return;
            //スタン状態ではないとき
            if (!isStan)
            {
                //　見回りまたはキャラクターを追いかける状態
                if (enemyState == CharacterState.Move || enemyState == CharacterState.Chase)
                {
                    //　キャラクターを追いかける状態であればキャラクターの目的地を再設定
                    if (enemyState == CharacterState.Chase)
                    {
                        setPosition.SetDestination(target.position);
                        agent.SetDestination(setPosition.GetDestination());
                    }
                    //　エージェントの潜在的な速さを設定
                    animator.SetFloat(animWalkSpeed, agent.velocity.sqrMagnitude);

                    if (enemyState == CharacterState.Move)
                    {
                        walkTime -= Time.deltaTime;
                        //navmeshagentが発動したとき
                        if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
                        {
                            //NavMeshAgentの操作
                            //　目的地に到着したかどうかとある程度の時間、目的地にたどり着けていないと判断したとき
                            if (agent.remainingDistance < 0.1f || walkTime <= 0 && !arrival)
                            {
                                walkTime = 5f;
                                SetState(CharacterState.Wait);
                                animator.SetFloat(animWalkSpeed, 0f);
                            }
                        }
                    }
                    else if (enemyState == CharacterState.Chase)
                    {
                        //　攻撃する距離だったら攻撃
                        if (agent.remainingDistance < attackDis)
                        {
                            SetState(CharacterState.Attack);
                        }
                    }
                }
                //　到着していたら一定時間待つ
                else if (enemyState == CharacterState.Wait)
                {
                    elapsedTime += Time.deltaTime;
                    //　待ち時間を越えたら次の目的地を設定もしくは、奥義をチャージする
                    if (elapsedTime > waitTime || isTargetFind)
                    {
                        MoveState();
                    }
                }
                else if (enemyState == CharacterState.Freeze)
                {
                    //待機時間
                    elapsedTime += Time.deltaTime;
                    //フリーズの時間
                    if (elapsedTime >= freezeAttackTime)
                    {
                        elapsedTime = 0;
                        MoveState();
                    }
                }
                else if (enemyState == CharacterState.Attack || enemyState == CharacterState.DeathBlow)
                {
                    if(enemyState == CharacterState.DeathBlow)
                    {
                        if(target == null)
                        {
                            SetState(CharacterState.Freeze);
                        }
                    }
                    if (target != null)
                    {
                        //　プレイヤーの方向を取得
                        var playerDirection = new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position;
                        //　敵の向きをプレイヤーの方向に少しづつ変える
                        var dir = Vector3.RotateTowards(transform.forward, playerDirection, rotateSpeed * Time.deltaTime, 0f);
                        //　算出した方向の角度を敵の角度に設定
                        transform.rotation = Quaternion.LookRotation(dir);
                    }
                }
                else if (enemyState == CharacterState.Charge)
                {
                    //チャージ中
                    if (doCharge)
                    {
                        //ダメージを受けてないとき、チャージする。
                        if (!isDamage)
                        {
                            //チャージが満タンになってないとき
                            if (chargeMax > chargeCount)
                            {
                                chargeSpanTime += Time.deltaTime;
                                //徐々に溜める
                                if (chargeSpan <= chargeSpanTime)
                                {
                                    chargeCount += 0.25f;
                                    chargeSpanTime = 0f;
                                }
                            }
                            else
                            {
                                //満タンになった時
                                SetState(CharacterState.DeathBlow);
                                doCharge = false;
                                chargeCount = 0;
                            }
                            chargeSlider.value = chargeCount / chargeMax;
                        }
                    }
                }                
            }
            else
            {
                //スタン状態
                if (enemyState == CharacterState.Stan)
                {
                    //スタン中
                    if (isStan)
                    {
                        stanStateTime += Time.deltaTime;
                        //スタン終了
                        if (stanStateTime >= maxStanTime)
                        {
                            stanStateTime = 0.0f;
                            Debug.Log("スタン解除");
                            chargeCount = 0;
                            isStan = false;
                            animator.SetBool(stanAnim, false);
                            SetState(CharacterState.Move);
                        }
                    }
                }
            }
            
        }
        /// <summary>
        /// 待機状態から行く状態を決める
        /// </summary>
        void MoveState()
        {
            int ran = UnityEngine.Random.Range(0,11);
            if (isTargetFind)
            {
                //プレイヤーが範囲内にいたら、確率でチャージする。
                if (ran >= chargePercent)
                {
                    SetState(CharacterState.Charge);
                    return;
                }
                else
                {
                    SetState(CharacterState.Move);
                    return;
                }
            }
            SetState(CharacterState.Move);
            return;
        }
        /// <summary>
        /// 必殺技の機能
        /// </summary>
        private void DeathBlowState()
        {
            attackVariation = AttackVariation.FireBall;
        }
        /// <summary>
        /// 攻撃
        /// </summary>
        private void Attack()
        {
            int ranAttack = UnityEngine.Random.Range(1, 5);
            if (ranAttack <= 3)
            {
                attackVariation = AttackVariation.Tail;
            }
            else if (ranAttack < 5)
            {
                attackVariation = AttackVariation.Scream;
                doScream = true;
            }
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="attackPoint"></param>
        /// <param name="damage"></param>
        /// <param name="playDamageAnim"></param>
        public override void ReceiveDamage(int attackPoint, WeaponDamageStock damage, bool playDamageAnim = true)
        {
            //必殺技中はダメージ無効
            if (attackVariation != AttackVariation.FireBall)
            {
                //チャージ中にダメージを受けたらスタン状態に
                if (!doCharge)
                {
                    base.ReceiveDamage(attackPoint, damage, playDamageAnim);
                }
                else
                {
                    base.ReceiveDamage(attackPoint, damage, false);
                    audioSourceManager.PlaySE(stanSE);
                    //ダメージを受けた時スタン状態になり、食らうダメージが1.5倍になる
                    SetState(CharacterState.Stan);
                    //スタンしたので、チャージ中のカウントを初期化
                    doCharge = false;
                    chargeCount = 0;
                    chargeSlider.value = 0f;
                    isStan = true;
                }
            }
        }
        protected override void TakeDamage(int damage, bool playDamageAnim)
        {
            //スタン状態だと1.5倍になる。
            int damagePoint = enemyState == CharacterState.Stan ? Mathf.CeilToInt(damage * 1.5f) : damage;
            //チャージ中だとスタン状態に
            var state = doCharge ? CharacterState.Stan : CharacterState.Damage;
            SetState(state);
            base.TakeDamage(damagePoint, playDamageAnim);
        }
        
        protected override void DeathState()
        {
            base.DeathState();
            gameManager.SetGameMode(GameMode.BossDefeat);
        }
        //------------アニメーションイベント-----------------------
        /// <summary>
        /// ファイアボールを出現
        /// </summary>
        private void FireBallSpawn()
        {
            BossAttackSE();
            //ファイアボールの召喚
            var fireBall = Instantiate(fireballObject, fireBallTransform.position, transform.rotation);
            var attackPoint = fireBall.GetComponent<BossAttackPoint>();
            if(attackPoint != null)
            {
                attackPoint.Boss = this;
                if (target != null)
                {
                    attackPoint.FireBall(true, target, fireBallSpeed);
                }
            }
        }
        /// <summary>
        /// 攻撃音
        /// </summary>
        private void BossAttackSE()
        {
            audioSourceManager.PlaySE(attackSE[(int)attackVariation - 1]);
        }
        protected override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);
        }
    }
}