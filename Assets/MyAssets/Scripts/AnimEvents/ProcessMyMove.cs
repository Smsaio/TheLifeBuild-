using PlayerSpace;
using UnityEngine;
using System.Collections;
using EnemySpace;
using System;
using UniRx;
using Zenject;

namespace AttackProcess
{
    public enum AttackEffectType
    {
        RotSlash = 0,
        NormalSlash = 1,
        None,
    }
    /// <summary>
    /// アニメーションイベントコライダーなどのオンオフ関連
    /// </summary>
    public class ProcessMyMove : MonoBehaviour
    {
        //攻撃が当たる範囲
        [Header("攻撃のコライダー"), SerializeField] private Collider attackCollider;
        //エフェクトの角度を参照する
        [NamedArray(new string[] { "回転するスラッシュ", "普通のスラッシュ" })]
        [Header("攻撃のエフェクト"), SerializeField] private ParticleSystem[] attackEffect;
        //スラッシュの音
        [NamedArray(new string[] { "回転するスラッシュ", "普通のスラッシュ" })]
        [SerializeField] private AudioClip[] attackSE;
        //攻撃するプレイヤー
        private Player player;
        //要素番号
        private int index = 0;
        IAudioSourceManager audioSourceManager = default;

        [Inject]
        public void Construct(IAudioSourceManager IaudioSourceManager)
        {
            audioSourceManager = IaudioSourceManager;
        }
        private void Start()
        {
            player = GetComponent<Player>();
            attackCollider.enabled = false;
            //Noneがあるため、-1
            int length = Enum.GetValues(typeof(AttackEffectType)).Length - 1;
            for (int i = 0; i < length; i++)
            {
                //出現させて止める
                attackEffect[i].gameObject.SetActive(false);
                attackEffect[i].Stop();
            }
        }

        //----アニメーション中にエフェクト出現-----
        /// <summary>
        /// エフェクト表示
        /// </summary>
        private void EffectOn(AttackEffectType attackType = AttackEffectType.None)
        {
            attackType = AttackEffectType.None == attackType ? AttackEffectType.NormalSlash : attackType;
            index = (int)attackType;
            attackEffect[index].gameObject.SetActive(true);
            //エフェクトを表示する
            attackEffect[index].Play();
            //ダメージ処理クラスに設定
            var damage = attackEffect[index].GetComponent<WeaponDamageStock>();
            if (damage != null)
            {
                damage.Player = player;
                damage.IsAttack = true;
            }
        }
        public void EffectOff()
        {
            var damage = attackEffect[index].GetComponent<WeaponDamageStock>();
            if(damage != null)
            {
                damage.IsAttack = false;
            }
            attackEffect[index].Stop();
            attackEffect[index].gameObject.SetActive(false);
        }

        //--------アニメーションイベント----------
        /// <summary>
        /// 回避無敵開始
        /// </summary>
        private void DodgeInvinsibilityStart()
        {
            player.IsInvinsibility = true;
        }
        /// <summary>
        /// 回避無敵終了
        /// </summary>
        private void DodgeInvinsibilityEnd()
        {
            player.IsInvinsibility = false;
        }
        /// <summary>
        /// 攻撃のコライダーオン
        /// </summary>
        public void AttackStart()
        {
            attackCollider.enabled = true;
        }
        /// <summary>
        ///  攻撃のコライダーオフ
        /// </summary>
        public void AttackEnd()
        {
            attackCollider.enabled = false;
        }
        /// <summary>
        /// 攻撃音
        /// </summary>
        /// <param name="attackSound">アニメーションイベントの攻撃音(入れなくてもいい)</param>
        public void AttackSE(AudioClip attackSound = null)
        {
            var sound = attackSound == null ? attackSE[index] : attackSound;
            audioSourceManager.PlaySE(sound);
        }
    }
}