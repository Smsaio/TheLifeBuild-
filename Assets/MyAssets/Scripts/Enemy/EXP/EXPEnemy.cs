using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Threading;
using PlayerSpace;
using Zenject;

namespace EnemySpace
{
    public class EXPEnemy : EnemyBase
    {
        //消えるときのエフェクト
        [SerializeField] private ParticleSystem banishEffect;
        [SerializeField, Range(5, 30)] private float runAwayDistance = 10f;
        //待ち時間
        [SerializeField] private float waitTime = 3f;
        //防御状態時の音
        [SerializeField] private AudioClip barrierSE;

        //　到着フラグ
        private bool arrival;
        //　SetPositionスクリプト
        private SetPosition setPosition;
        //　経過時間
        private float elapsedTime;
        // 敵の状態
        private CharacterState state;
        //歩いている時間
        private float walkTime = 5f;

        //ガードの時にダメージを受けないの疑似的な実装
        private int keepHP = 0;
        //ガードの回数(2回したら消滅)
        private int guardCount = 0;
        //ガードした
        private bool isGuard = false;
        //逃げる場所
        private Vector3 runDestination;
        protected override void Awake()
        {
            base.Awake();
        }
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            setPosition = GetComponent<SetPosition>();
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
        protected override void TakeDamage(int damage, bool playDamageAnim)
        {
            base.TakeDamage(damage,playDamageAnim);
        }
        protected override void OnCollisionEnter(Collision col)
        {
            base.OnCollisionEnter(col);
        }
        /// <summary>
        /// 見回りしながらプレイヤーが来たら逃げる
        /// </summary>
        protected override void StateMove()
        {
            RunState();
            if (state == CharacterState.Death)
            {
                return;
            }
            //　逃げるまたはキャラクターを追いかける状態
            if (state == CharacterState.Move || state == CharacterState.Run)
            {
                //　エージェントの潜在的な速さを設定
                animator.SetFloat(animWalkSpeed, agent.desiredVelocity.magnitude);
                if(state == CharacterState.Move)
                    walkTime -= Time.deltaTime;
                //navmeshagentが発動したとき
                if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
                {
                    //NavMeshAgentの操作
                    //　目的地に到着したかどうかとある程度の時間、目的地にたどり着けていないと判断したとき
                    if (agent.remainingDistance < 0.1f || walkTime <= 0 && !arrival)
                    {
                        //歩行の場合は待機
                        if (state == CharacterState.Move)
                        {
                            walkTime = 5f;
                            SetState(CharacterState.Wait);
                        }
                        else if (state == CharacterState.Run)
                        {
                            //逃げた後の場合は、防御する
                            SetState(CharacterState.Guard);
                        }
                    }                
                }                
            }
            //　到着していたら一定時間待つ
            else if (state == CharacterState.Wait || state == CharacterState.Guard)
            {
                if(state == CharacterState.Guard)
                {
                    //ガード中は食らわないようにする
                    if (isDamage && !Pauser.isPause)
                        currentHP = keepHP;
                }
                elapsedTime += Time.deltaTime;
                //　待ち時間を越えたら次の目的地を設定
                if (elapsedTime > waitTime)
                {
                    //ガード状態
                    if (state == CharacterState.Guard)
                    {
                        //ガード中
                        if (isGuard)
                        {
                            GuardEnd();
                            //ガード回数が3回目になっていたら
                            if (guardCount == 3)
                            {
                                Instantiate(banishEffect, transform.position, Quaternion.identity);
                                Destroy(gameObject);
                            }
                            else
                            {
                                isGuard = false;
                                animator.SetBool("Guard", isGuard);
                            }
                        }
                    }
                    //再び動き始める
                    SetState(CharacterState.Move);
                }
            }
            else if(state == CharacterState.Run)
            {
                //逃げているとき
                animator.SetFloat(animWalkSpeed, agent.desiredVelocity.magnitude);
                //navmeshagentが発動したとき
                if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
                {
                    //　目的地に到着したかどうか
                    if (agent.remainingDistance < 0.1f)
                    {
                        SetState(CharacterState.Guard);
                        animator.SetFloat(animWalkSpeed, 0f);
                    }
                }
            }
        }
        /// <summary>
        /// 敵キャラクターの状態変更メソッド
        /// </summary>
        /// <param name="tempState">今の状態</param>
        public override void SetState(CharacterState tempState,Transform targetobj = null)
        {
            state = tempState;
            if (state == CharacterState.Death)
            {
                return;
            }
            //navmeshagentが発動したとき 
            if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
            {
                //agentの操作
                //歩行中
                if (tempState == CharacterState.Move)
                {
                    arrival = false;
                    agent.isStopped = false;
                    setPosition.CreateRandomPosition();
                    agent.SetDestination(setPosition.GetDestination());
                }
                //逃げる
                else if (tempState == CharacterState.Run)
                {
                    arrival = false;
                    isGuard = false;
                    agent.isStopped = false;
                    //NavMeshの目的地を計算
                    var direction = (transform.position - target.position).normalized;
                    direction.y = 0;
                    runDestination = transform.position + direction * runAwayDistance;
                }
                else if (tempState == CharacterState.Wait)
                {
                    //待機
                    animator.SetFloat(animWalkSpeed, 0f);
                    elapsedTime = 0f;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                    //到着した
                    arrival = true;
                }
                else if (tempState == CharacterState.Guard)
                {
                    //防御
                    isGuard = true;
                    arrival = true;
                    elapsedTime = 0f;
                    guardCount++;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                    audioSourceManager.PlaySE(barrierSE);
                    animator.SetBool("Guard", isGuard);
                }
                else if (tempState == CharacterState.Damage)
                {
                    //ダメージを受けた
                    animator.SetTrigger("Damage");
                    agent.isStopped = true;
                }
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
            base.ReceiveDamage(attackPoint, damage, playDamageAnim);
        }
        /// <summary>
        /// 逃げている状態
        /// </summary>
        void RunState()
        {
            if (player != null)
            {
                //カメラからプレイヤーの位置と自身までの距離を計算
                var distance = Vector3.Distance(target.position, transform.position);
                //プレイヤーとの距離を判定                
                if (distance < runAwayDistance)
                {
                    agent.isStopped = false;
                    arrival = false;
                    Debug.Log("逃げる");
                    SetState(CharacterState.Run);
                    agent.SetDestination(runDestination);
                    //目的地の近くになったら
                    if (agent.remainingDistance <= 0.3f)
                    {
                        //体力をキープする
                        keepHP = currentHP;
                        //防御する(攻撃が利かなくなる)
                        SetState(CharacterState.Guard);
                    }
                }
            }
        }
//--------アニメーションイベント----------
        //防御開始
        public void GuardStart()
        {
            animator.enabled = false;
        }
        //防御終了
        public void GuardEnd()
        {
            animator.enabled = true;
        }
    }
}