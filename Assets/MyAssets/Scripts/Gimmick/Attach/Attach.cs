using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Ability
{
    //この列挙型で
    /// <summary>
    /// 特技や役割の定義クラス
    /// </summary>
    public class Attach
    {
        /// <summary>
        /// プレイヤーの役割
        /// </summary>
        public enum Role
        {
            Swordman = 0,     //剣士
            Gunner = 1,       //銃士
            DemonPos = 2,     //悪魔憑き
        }
        /// <summary>
        /// プレイヤーの特技
        /// </summary>
        public enum Speciality
        {
            Stagnation = 0, //記憶停滞
            Refusal = 1,    //記憶拒絶
            Convert = 2,    //記憶転換
        }
        /// <summary>
        /// 役割設定
        /// </summary>
        public static Role role = Role.Swordman; //(スタート画面で役割設定する場合にシーンをまたいで参照したいから必要)
    }
}
