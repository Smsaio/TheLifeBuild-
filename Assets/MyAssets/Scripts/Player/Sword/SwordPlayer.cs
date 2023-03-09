using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.VFX;

namespace PlayerSpace
{
    /// <summary>
    /// 剣士のプレイヤー管理クラス
    /// </summary>
    public class SwordPlayer : Player
    {
        //必殺技で出るオブジェクト
        [Header("剣士専用変数")]
        //必殺技で発射されるスラッシュエフェクト
        [Header("発射するエフェクト"),SerializeField] private VisualEffect groundSlash;
        //角度最大、最小値設定
        [Range(45,120),SerializeField] private float maxLimitAngle = 90.0f;
        [Range(45,120),SerializeField] private float minLimitAngle = 90.0f;
        [Range(3, 7), SerializeField] private int slashLoopCount = 3;
        //必殺技のエフェクトを出す数
        [Header("必殺技を角度内で出したい数"),SerializeField] private int slashCount = 5;
        private List<VisualEffect> slashList = new();
        protected override void Start()
        {
            base.Start();
            SlashInit();
        }
        protected override void Update()
        {
            base.Update();
        }   
        public override void EXPInitialize(StreamWriter sw)
        {
            sw.WriteLine("剣士");
            base.EXPInitialize(sw);
        }
        private void SlashInit()
        {
            //何回連続で放つか
            for (int loop = slashLoopCount; loop >= 1; loop--)
            {
                for (int i = slashCount; i >= 1; i--)
                {
                    //角度を分割
                    var slashEffect = Instantiate(groundSlash, transform.position, Quaternion.identity);
                    slashEffect.Stop();
                    slashEffect.gameObject.SetActive(false);
                    slashList.Add(slashEffect);
                }
            }
        }
        /// <summary>
        /// 正面から見て等分割した角度を計算アニメーションイベント
        /// </summary>
        public void SlashSplitAngle()
        {
            float angle;
            float degree;
            float minAngle = transform.eulerAngles.y - minLimitAngle;
            float maxAngle = transform.eulerAngles.y + maxLimitAngle;
            float between = (maxAngle) / slashCount;
            int correction = 30;
            GroundSlash groundSlash;
            int index = 0;
            //何回連続で放つか
            for (int loop = slashLoopCount; loop >= 1; loop--)
            {
                for (int i = slashCount; i >= 1; i--)
                {
                    //角度を分割
                    degree = ((transform.eulerAngles.y + correction) - (i * between)) - (transform.eulerAngles.y - (correction - 5)); 
                    angle = GetNormalizedAngle(degree , minAngle, maxAngle);
                    //スラッシュエフェクトを取得できているとき
                    if (slashList[index] != null)
                    {
                        slashList[index].gameObject.SetActive(true);
                        slashList[index].Play();
                        groundSlash = slashList[index].gameObject.GetComponent<GroundSlash>();
                        var damage = slashList[index].gameObject.GetComponent<WeaponDamageStock>();
                        //グランドスラッシュ初期化
                        groundSlash.IsInitialize = true;
                        groundSlash.Initialize(transform, transform.forward * 3.5f, angle, loop);
                        if (damage != null)
                        {
                            damage.ReactivePlayer(role);
                            damage.IsDeathBlow = true;
                            Debug.Log(damage);
                        }
                    }
                    index++;
                }
            }
        }
        private float GetNormalizedAngle(float angle, float min, float max)
        {
            return Mathf.Repeat(angle - min, max - min) + min;
        }
    }
}