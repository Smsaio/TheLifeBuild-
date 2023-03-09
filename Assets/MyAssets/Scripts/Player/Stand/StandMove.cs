using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemySpace;
using UniRx;
using System;

namespace PlayerSpace
{
    /// <summary>
    /// スタンド(悪魔)管理クラス
    /// </summary>
    public class StandMove : MonoBehaviour
    {
        //目的地に到達するまでの時間
        [SerializeField] private float smoothTime = 1.0f;
        //着いたかどうかの距離
        [Range(0.5f, 2.0f), SerializeField] private float arrivalDistance = 1.0f;

        //アニメータープロパティ
        private Animator anim;
        public Animator StandAnimator { get { return anim; } }
        private StandPlayerMove playerMove;
        public StandPlayerMove StandPlayer { set { playerMove = value; }get { return playerMove; } }
        private Player player;
        public Player SPlayer { set { player = value; } get { return player; } }
        //攻撃した
        private bool isAttack = false;
        public bool IsAttack { get { return isAttack; } set { isAttack = value; } } 
        //歩行スピード
        private readonly float maxMoveSpeed = 15;
        //設定された位置に戻る時間
        private float attackAfterTime = 1.0f;
        //速度ベクトル
        private Vector3 smoothVelocity = Vector3.zero;
        private int walkSpeedAnim = Animator.StringToHash("WalkSpeed");
        // Start is called before the first frame update
        private void OnEnable()
        {
            if (playerMove != null)
            {
                transform.position = playerMove.StandTransform.position;
                transform.rotation = playerMove.transform.rotation;
            }
        }
        void Awake()
        {
            
        }
        private void Start()
        {
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (playerMove != null)
            {
                StMove();
            }
        }
        /// <summary>
        /// スタンドを常に定位置に設定
        /// </summary>
        private void StMove()
        {
            //スタンドの定位置に設定される
            transform.position = Vector3.SmoothDamp(transform.position, playerMove.StandTransform.position, ref smoothVelocity, smoothTime, maxMoveSpeed);
            //攻撃中ではない
            if (!isAttack)
            {
                //近くなったら同じ向きになる
                if (Vector3.Distance(transform.position, playerMove.StandTransform.position) < arrivalDistance)
                {
                    //常にプレイヤーと同じ向きに
                    var rot = playerMove.transform.rotation;
                    transform.rotation = rot;
                }
                else
                {
                    //目的地の方向に向く
                    transform.LookAt(playerMove.StandTransform.position);
                }
                //歩行中
                anim.SetFloat(walkSpeedAnim, smoothVelocity.magnitude);
            }
        }
        /// <summary>
        /// スタンドのアニメーションfloatパラメータ設定
        /// </summary>
        /// <param name="paramaterAnimName">アニメーションの名前</param>
        /// <param name="magnitude">歩行速度</param>
        public void StandWalkAnim(int paramaterAnimName, float magnitude)
        {
            if (anim != null)
                anim.SetFloat(paramaterAnimName, magnitude);
        }
        /// <summary>
        /// スタンドのアニメーションのトリガーパラメータ設定
        /// </summary>
        /// <param name="paramaterAnimName"></param>
        public void StandAnimationTrigger(int paramaterAnimName)
        {
            if (anim != null)
                anim.SetTrigger(paramaterAnimName);
        }
        /// <summary>
        /// スタンドのアニメーションのboolパラメータ設定(攻撃)
        /// </summary>
        /// <param name="paramaterAnimName">攻撃のアニメーションの名前</param>
        /// <param name="attack">攻撃する</param>
        public void StandBoolAttack(int paramaterAnimName,bool attack = true)
        {
            if (anim != null)
            {
                //攻撃していない場合
                if (!isAttack)
                {
                    //アニメーションさせて
                    anim.SetBool(paramaterAnimName, attack);
                    isAttack = true;
                    //設定した時間後に攻撃が終わる。
                    Observable.Timer(TimeSpan.FromSeconds(attackAfterTime), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                    {
                        isAttack = false;
                    }).AddTo(this);
                }                
            }
        }
    }
}