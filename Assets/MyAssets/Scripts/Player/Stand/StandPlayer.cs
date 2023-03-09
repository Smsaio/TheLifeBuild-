using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Ability;
using System.IO;

namespace PlayerSpace
{
    /// <summary>
    /// 悪魔憑きのプレイヤー管理クラス
    /// </summary>
    public class StandPlayer : Player
    {

        //悪魔のクラスで取得する用のプロパティ
        public Rigidbody Rigid { get { return rb; } }


        protected override void Start()
        {
            base.Start();
        }
        protected override void Update()
        {
            base.Update();
        }

        public override void EXPInitialize(StreamWriter sw)
        {
            sw.WriteLine("悪魔憑き");
            base.EXPInitialize(sw);
        }
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="attackPoint"></param>
        /// <param name="playDamageAnim"></param>
        /// <param name="memoryType"></param>
        /// <param name="isBodyHit"></param>
        public override void ReceiveDamage(int attackPoint,
            bool playDamageAnim = true, MemoryType.MemoryClassification memoryType = MemoryType.MemoryClassification.None,
            bool isBodyHit = false)
        {
            base.ReceiveDamage(attackPoint,playDamageAnim, memoryType,isBodyHit);
        }
    }
}
