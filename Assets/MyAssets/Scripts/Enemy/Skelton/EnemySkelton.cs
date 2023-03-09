using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemySpace
{
    /// <summary>
    /// 止まって監視するクラス(スケルトン)
    /// </summary>
    [RequireComponent(typeof(Animator))]    
    public class EnemySkelton : EnemyBase
    {
        //攻撃の間隔
        [SerializeField] private float attackInterval = 1;
        //自分と目標のコリジョン
        private float myCollisionRadius;
        private float targetCollisionRadius;
        //攻撃をした後、次に攻撃するまでの時間
        private float attackBetweenTime = 0.0f;
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            deathSpawnY = 0.5f;            
            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        }

        // Update is called once per frame
        protected override void Update()
        {            
            base.Update();
            if (!isDeath)
            {
                StateMove();
                TargetSettings();
            }
        }
        public override void SetState(CharacterState state, Transform targetTransform = null)
        {
            base.SetState(state, targetTransform);
            if (state == CharacterState.Death) return;
            if(state == CharacterState.Wait)
            {
                agent.velocity = Vector3.zero;
                agent.isStopped = true;
            }
            else if(state == CharacterState.Chase)
            {
                agent.isStopped = false;
                target = targetTransform;
            }
            else if(state == CharacterState.Attack)
            {
                agent.velocity = Vector3.zero;
                agent.isStopped = true;
                attackBetweenTime = 0.0f;
                animator.SetTrigger(attackAnim);
            }
        }
        //ターゲット設定
        private void TargetSettings()
        {
            if (target != null && isTargetFind)
            {
                if (targetCollisionRadius == 0f)
                {
                    targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
                }
                // 方向を求める
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                // directionToTarget * (自分の半径+ターゲットの半径)で、自分とターゲットの半径の長さ分の向きベクトルが求められる。
                Vector3 targetPosition = target.position - directionToTarget * (myCollisionRadius + targetCollisionRadius + attackDistance / 2);
                agent.SetDestination(targetPosition);
            }
        }
        //追跡目標に接近
        protected override void StateMove()
        {
            if (target != null && isTargetFind)
            {
                // 次の攻撃までの時間を満たしていれば
                attackBetweenTime += Time.deltaTime;
                if (attackBetweenTime > attackInterval)
                {
                    //　攻撃する距離だったら攻撃
                    if (agent.remainingDistance < attackDistance)
                    {
                        SetState(CharacterState.Attack);
                    }
                    else
                    {
                        agent.isStopped = false;
                    }
                }
                animator.SetFloat(animWalkSpeed, agent.velocity.sqrMagnitude);
            }
            else
            {
                animator.SetFloat(animWalkSpeed, 0);
                SetState(CharacterState.Wait);
            }
        }
        protected override void OnCollisionEnter(Collision other)
        {
            base.OnCollisionEnter(other);
        }        
    }
}
