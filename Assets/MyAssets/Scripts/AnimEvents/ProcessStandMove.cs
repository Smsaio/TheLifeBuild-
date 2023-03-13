using PlayerSpace;
using UnityEngine;
using System.Collections;
using EnemySpace;
using System;
using UniRx;
using Zenject;

namespace AttackProcess
{
    /// <summary>
    /// アニメーションイベントコライダーなどのオンオフ関連
    /// </summary>
    public class ProcessStandMove : MonoBehaviour,IProcessAnim
    {
        //攻撃のタイプ
        public enum StandAttackType
        {
            RotSlash = 0,    //回転するスラッシュ
            NormalSlash = 1, //普通のスラッシュ
        }
        //攻撃が当たる範囲
        [Header("攻撃のコライダー"), SerializeField] private Collider attackCollider;
        [SerializeField] private AudioClip attackSE;
        //回転するスラッシュ
        [SerializeField] private ParticleSystem standAttackEffect;
        //攻撃するプレイヤー
        private StandMove standMove;
        IAudioSourceManager audioSourceManager = default;

        [Inject]
        public void Construct(IAudioSourceManager IaudioSourceManager)
        {
            audioSourceManager = IaudioSourceManager;
        }
        private void Start()
        {
            standMove = GetComponent<StandMove>();
            //攻撃のコライダー
            attackCollider.enabled = false;
            //出現させて止める
            standAttackEffect.Stop();
            standAttackEffect.gameObject.SetActive(false);
        }
        //----アニメーション中にエフェクト出現-----
        /// <summary>
        /// エフェクト表示
        /// </summary>
        public void EffectOn()
        {
            standAttackEffect.gameObject.SetActive(true);
            //エフェクトを表示する
            standAttackEffect.Play();
            //ダメージ処理クラスに設定
            var damage = standAttackEffect.GetComponent<WeaponDamageStock>();
            if (damage != null)
                damage.Player = standMove.SPlayer;
        }
        public void EffectOff()
        {
            standAttackEffect.Stop();
            standAttackEffect.gameObject.SetActive(false);
        }
        //----------アニメーションイベントにつける-----------
        /// <summary>
        /// 攻撃のコライダーオン
        /// </summary>
        public void AttackStart()
        {
            //攻撃のコライダー起動
            attackCollider.enabled = true;
            standMove.IsAttack = true;
            standAttackEffect.gameObject.SetActive(true);
            //回転のスラッシュ
            standAttackEffect.Play();
            //ダメージを与えるコンポーネント取得
            var damage = standAttackEffect.GetComponent<WeaponDamageStock>();
            if (damage != null)
            {
                damage.Player = standMove.SPlayer;
            }
        }
        /// <summary>
        /// 攻撃のコライダーオフ
        /// </summary>
        public void AttackEnd()
        {
            attackCollider.enabled = false;
            EffectOff();
        }
        /// <summary>
        /// 攻撃音
        /// </summary>
        public void AttackSE(AudioClip attackSound = null)
        {
            var sound = attackSound == null ? attackSE : attackSound;
            audioSourceManager.PlaySE(sound);
        }
    }
}
