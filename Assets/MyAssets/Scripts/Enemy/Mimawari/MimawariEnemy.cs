using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using PlayerSpace;
namespace EnemySpace
{
    /// <summary>
    /// ステートベースAI(見回り版)
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MimawariEnemy : EnemyBase
    {
        //　待ち時間
        [SerializeField] private float waitTime = 3f;
        //　到着フラグ
        private bool arrival;
        //　SetPositionスクリプト
        private SetPosition setPosition;
        //　経過時間
        private float elapsedTime;
        // 敵の状態
        private CharacterState state;
        //攻撃の後の凍結時間
        private float freezeAttackTime = 0.0f;
        //歩いている時間
        private float walkTime = 0.0f;
        private float maxWalkTime = 20.0f;
        //まわる速さ
        private readonly float rotateSpeed = 5;
        //元の速度
        private float agentSpeed = 0.0f;
        //追跡時の速度
        private float agentFindSpeed = 4.5f;
        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            //初期化
            agentSpeed = agent.speed;
            setPosition = GetComponent<SetPosition>();
            setPosition.CreateRandomPosition();
            agent.SetDestination(setPosition.GetDestination());
            arrival = false;
            elapsedTime = 0f;
            freezeAttackTime = waitTime * 1.25f;
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
        /// <summary>
        /// 見回り用のステート制御
        /// </summary>
        protected override void StateMove()
        {
            base.StateMove();
            if (state == CharacterState.Death)
            {
                return;
            }
            //　見回りまたはキャラクターを追いかける状態
            if (state == CharacterState.Move || state == CharacterState.Chase)
            {
                //　キャラクターを追いかける状態であればキャラクターの目的地を再設定
                if (state == CharacterState.Chase)
                {
                    setPosition.SetDestination(target.position);
                    agent.SetDestination(setPosition.GetDestination());
                }
                //　エージェントの潜在的な速さを設定
                animator.SetFloat(animWalkSpeed, agent.velocity.sqrMagnitude);

                if (state == CharacterState.Move)
                {
                    walkTime += Time.deltaTime;
                    //navmeshagentが発動しているとき
                    if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
                    {
                        //　目的地に到着したかどうかとある程度の時間、目的地にたどり着けていないと判断したとき
                        if (agent.remainingDistance < 0.1f || walkTime >= maxWalkTime && !arrival)
                        {
                            walkTime = 0f;
                            SetState(CharacterState.Wait);
                            animator.SetFloat(animWalkSpeed, 0f);
                        }                        
                    }
                }
                else if (state == CharacterState.Chase)
                {
                    //　攻撃する距離だったら攻撃
                    if (agent.remainingDistance < attackDistance)
                    {
                        SetState(CharacterState.Attack);
                    }
                }
                //　到着していたら一定時間待つ
            }
            else if (state == CharacterState.Wait)
            {
                elapsedTime += Time.deltaTime;
                //　待ち時間を越えたら次の目的地を設定、プレイヤーを発見したら
                if (elapsedTime > waitTime || isTargetFind)
                {
                    if (isTargetFind)
                    {
                        elapsedTime = 0f;
                    }
                    SetState(CharacterState.Move);
                }
                //　攻撃後のフリーズ状態
            }
            else if (state == CharacterState.Freeze)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > freezeAttackTime)
                {
                    //歩行状態へ移行
                    SetState(CharacterState.Move);
                }
            }
            else if (state == CharacterState.Attack)
            {
                //　プレイヤーの方向を取得
                var playerDirection = new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position;
                //　敵の向きをプレイヤーの方向に少しづつ変える
                var dir = Vector3.RotateTowards(transform.forward, playerDirection, rotateSpeed * Time.deltaTime, 0f);
                //　算出した方向の角度を敵の角度に設定
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
        /// <summary>
        /// 敵キャラクターの状態変更メソッド
        /// </summary>
        /// <param name="tempState">敵の状態</param>
        /// <param name="targetObj">目的地</param>
        public override void SetState(CharacterState tempState, Transform targetObj = null)
        {
            state = tempState;
            if (tempState == CharacterState.Death)
            {
                return;
            }
            //navmeshagentが発動したとき 
            if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
            {
                //agentの操作
                if (tempState == CharacterState.Move)
                {
                    //歩行
                    arrival = false;
                    agent.isStopped = false;
                    setPosition.CreateRandomPosition();
                    agent.SetDestination(setPosition.GetDestination());
                }
                else if (tempState == CharacterState.Chase)
                {
                    //追跡
                    //　待機状態から追いかける場合もあるのでOff
                    arrival = false;
                    //　追いかける対象をセット
                    target = targetObj;
                    agent.speed = agentFindSpeed;
                    agent.SetDestination(target.position);
                    agent.isStopped = false;
                }
                else if (tempState == CharacterState.Wait)
                {
                    //待機
                    elapsedTime = 0f;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                    //到着した
                    arrival = true;
                    animator.SetFloat(animWalkSpeed, 0f);
                }
                else if (tempState == CharacterState.Attack)
                {
                    //攻撃
                    animator.SetTrigger(attackAnim);
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }
                else if (tempState == CharacterState.Freeze)
                {
                    //フリーズ中
                    elapsedTime = 0f;
                    agent.velocity = Vector3.zero;
                    animator.SetFloat(animWalkSpeed, 0f);
                }
                else if (tempState == CharacterState.Damage)
                {
                    //ダメージ
                    animator.SetTrigger(damageAnim);
                    agent.isStopped = true;
                }
            }
        }
        protected override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);
        }
    }
}
