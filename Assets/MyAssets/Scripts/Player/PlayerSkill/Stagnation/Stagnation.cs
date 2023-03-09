using EnemySpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    /// <summary>
    /// 記憶停滞の際に用いる特技のクラス
    /// </summary>
    public class Stagnation : SpecialityBase
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// プレイヤーが敵止めを使い、敵が動き始めた後敵止め中に与えられたダメージを与える
        /// </summary>
        public override void StopStay()
        {
            //静止させている敵にダメージを与えた場合
            if (damageStock > 0 && stopDamageObject.Count > 0)
            {
                //静止させている敵すべてに今まで与えたダメージを与える
                for (int i = 0; i < stopDamageObject.Count; i++)
                {
                    var enemyBase = stopDamageObject[i].GetComponent<EnemyBase>();
                    enemyBase.ReceiveDamage(damageStock, true);
                }
                //初期化
                damageStock = 0;
                stopDamageObject.Clear();
            }
        }
        public override void EnemyDamageStock(int attackPoint, GameObject other)
        {
            //敵のダメージを蓄積
            damageStock += attackPoint;
            //ダメージを与えた敵を配列に追加
            stopDamageObject.Add(other);
        }
        public override void UseSpeciality()
        {
            Pauser.Pause();
        }
        public override void DoneSpeciality()
        {
            // 解除したときに、敵に攻撃している場合
            if (Pauser.isPause)
            {
                StopStay();
                Pauser.Resume();
            }
        }
    }
}