using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UniRx;
using UnityEngine.InputSystem;
using Zenject;

namespace PlayerSpace
{
    /// <summary>
    /// 銃士のプレイヤー管理クラス
    /// </summary>
    public class GunnerPlayer : Player
    {
        //役割ごとの通常攻撃
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
            sw.WriteLine("銃士");
            base.EXPInitialize(sw);
        }
    }
}