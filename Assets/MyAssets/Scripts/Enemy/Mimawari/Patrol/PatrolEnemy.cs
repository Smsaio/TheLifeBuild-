using UnityEngine;
using UnityEngine.AI;
using PlayerSpace;
using System;
using UniRx;

namespace EnemySpace
{    
    /// <summary>
    /// 決まった位置に移動する敵(設定された目的地をパトロールする)
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(DestinationController))]
    public class PatrolEnemy : EnemyBase
    {
        [ Header("巡回の敵の設定")]
        [SerializeField] private DestinationController destinationController;
        //　到着フラグ
        private bool arrival;
        //　待ち時間
        [SerializeField] private float waitTime = 3f;
        //　経過時間
        private float elapsedTime = 0;
        // 敵の状態
        private CharacterState state;
        //攻撃の後の凍結時間
        private float freezeTime = 2.0f;        
        //まわる速さ
        private float rotateSpeed = 5;
        //移動する時間
        private float walkTime = 0.0f;
        private int maxWalkTime = 40;
        //元の速度
        private float agentSpeed = 0.0f;
        //発見時の速度
        private float agentFindSpeed = 5.5f;


        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            agentSpeed = agent.speed;
            destinationController = GetComponent<DestinationController>();
            arrival = false;
            elapsedTime = 0f;
            freezeTime = waitTime * 1.25f;
            SetState(CharacterState.Move);
        }

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
        //見回りの機能
        protected override void StateMove()
        {
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
                    destinationController.SetDestination(target.position);
                    agent.SetDestination(destinationController.GetDestination());
                }
                //　エージェントの潜在的な速さを設定
                animator.SetFloat(animWalkSpeed, agent.velocity.sqrMagnitude);

                if (state == CharacterState.Move)
                {
                    walkTime += Time.deltaTime;
                    //　おおよそ目的地についた場合、もしくは歩く時間がある程度たっていて、ある程度の時間目的地にたどり着けていないと判断したとき
                    if (walkTime >= maxWalkTime && !arrival || agent.remainingDistance < 0.1f)
                    {
                        SetState(CharacterState.Wait);
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
                //　待ち時間を越えたら次の目的地を設定
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
                if (elapsedTime > freezeTime)
                {
                    SetState(CharacterState.Move);
                }
            }
            else if (state == CharacterState.Attack)
            {
                //　プレイヤーの方向を取得
                var targetDirection = new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position;
                //　敵の向きをプレイヤーの方向に少しづつ変える
                var dir = Vector3.RotateTowards(transform.forward, targetDirection, rotateSpeed * Time.deltaTime, 0f);
                //　算出した方向の角度を敵の角度に設定
                transform.rotation = Quaternion.LookRotation(dir);
                //攻撃後の硬直
                SetState(CharacterState.Freeze);
            }
        }
        //　敵キャラクターの状態変更メソッド
        public override void SetState(CharacterState tempState, Transform targetObj = null)
        {
            state = tempState;
            if(tempState == CharacterState.Death)
            {
                return;
            }
            if (tempState == CharacterState.Move)
            {
                //歩行
                arrival = false;
                destinationController.CreateDestination();
                agent.SetDestination(destinationController.GetDestination());
                agent.isStopped = false;
            }
            else if (tempState == CharacterState.Chase)
            {
                //追跡
                //　待機状態から追いかける場合もあるのでOff
                arrival = false;
                //　追いかける対象をセット
                target = targetObj;
                agent.speed = agentFindSpeed;
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
            else if (tempState == CharacterState.Wait)
            {
                //待機
                elapsedTime = 0f;
                walkTime = 0f;
                arrival = true;            
                agent.isStopped = true;
                animator.SetFloat(animWalkSpeed, 0f);
            }
            else if (tempState == CharacterState.Attack)
            {
                //攻撃
                animator.SetTrigger(attackAnim);
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            else if (tempState == CharacterState.Freeze)
            {
                //フリーズ
                elapsedTime = 0f;
                walkTime = 0;
                animator.SetFloat(animWalkSpeed, 0f);
            }
            else if (tempState == CharacterState.Damage)
            {
                //ダメージ
                animator.SetTrigger("Damage");
                agent.isStopped = true;
            }
        }
        protected override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);
        }
    }
}
