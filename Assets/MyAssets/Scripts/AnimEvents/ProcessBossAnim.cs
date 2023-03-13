using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemySpace;
namespace AttackProcess
{
    /// <summary>
    /// アニメーションイベントコライダーなどのオンオフ関連
    /// </summary>
    public class ProcessBossAnim : ProcessEnemyAnim
    {
        //チャージエフェクト
        [SerializeField] private GameObject chargeEffect;
        //しっぽ攻撃
        [SerializeField] private Collider tailCollider;
        //叫び攻撃
        [SerializeField] private Collider screamCollider;
        private BossEnemy bossEnemy;
        private int attackNumAnim = Animator.StringToHash("AttackNum");
        // Start is called before the first frame update
        protected override void Start()
        {
            chargeEffect.gameObject.SetActive(false);
            bossEnemy = GetComponent<BossEnemy>();
        }
        protected override void Update()
        {

        }
        //--------アニメーションイベント---------
        /// <summary>
        /// チャージエフェクトオン
        /// </summary>
        public void ChargeEffect()
        {
            chargeEffect.SetActive(true);
        }
        /// <summary>
        /// チャージエフェクトオフ
        /// </summary>
        public void ChargeEffectEnd()
        {
            chargeEffect.SetActive(false);
        }
        /// <summary>
        /// テール攻撃のコライダーオン
        /// </summary>
        private void TailAttackStart()
        {
            tailCollider.enabled = true;
        }
        /// <summary>
        /// テール攻撃のコライダーオフ
        /// </summary>
        public void TailAttackEnd()
        {
            tailCollider.enabled = false;
        }
        /// <summary>
        /// スクリーム攻撃のコライダーオン
        /// </summary>
        public void ScreamAttackStart()
        {
            screamCollider.enabled = true;
        }
        /// <summary>
        /// スクリームの攻撃のコライダーオフ
        /// </summary>
        public void ScreamAttackEnd()
        {
            screamCollider.enabled = false;
        }
        /// <summary>
        /// 攻撃の後の硬直
        /// </summary>
        public override void AttackEnd()
        {
            //攻撃をやめる
            bossEnemy.DoAttack = false;
            bossEnemy.EnemyAnimator.SetInteger(attackNumAnim, 0);
            bossEnemy.SetState(CharacterState.Freeze);
        }
        /// <summary>
        /// ダメージ後の移動
        /// </summary>
        public override void DamageEnd()
        {
            //スタン状態以外では移動
            if (bossEnemy.GetState() != CharacterState.Stan)
            {
                bossEnemy.SetState(CharacterState.Move);
            }
        }
        /// <summary>
        /// 攻撃のSE
        /// </summary>
        /// <param name="attackSound">攻撃音</param>
        public override void AttackSE(AudioClip attackSound = null)
        {

        }
    }
}