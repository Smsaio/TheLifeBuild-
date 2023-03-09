using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;
namespace EnemySpace
{
    /// <summary>
    /// 敵の武器につける攻撃判定クラス
    /// </summary>
    public class EnemyAttackPoint : MonoBehaviour
    {
        private EnemyBase enemyBase;
        public EnemyBase Enemy { get { return enemyBase; } set { enemyBase = value; } }
        // Start is called before the first frame update
        protected virtual void Start()
        {
            enemyBase = transform.root.GetComponent<EnemyBase>();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if(enemyBase != null)
                enabled = enemyBase.enabled;
        }
        /// <summary>
        /// 敵の攻撃をプレイヤーに与える
        /// </summary>
        /// <param name="obj">コライダーが当たったオブジェクト</param>
        /// <param name="enemyBase">敵</param>
        protected virtual void EnemyAttack(GameObject obj, EnemyBase enemyBase = null)
        {
            if (obj.CompareTag("Search")) return;
            if (obj.transform.root.gameObject.CompareTag("Player") || obj.transform.root.gameObject.CompareTag("Fellow"))
            {
                Player player = obj.transform.root.gameObject.GetComponent<Player>();
                var damage = obj.transform.root.GetComponent<IDamageble>();
                Debug.Log("敵の攻撃が当たった");
                //プレイヤーの場合
                if (player != null)
                {
                    //レベルが上がっている途中ではない
                    if (!player.IsLevelUP)
                    {
                        if (enemyBase != null && damage != null)
                        {
                            //プレイヤーにダメージを与える
                            damage.ReceiveDamage(enemyBase.AttackP, true, enemyBase.MemoryClassification);
                        }
                    }
                }
                else
                {
                    //それ以外の場合
                    damage.ReceiveDamage(enemyBase.AttackP, true);
                }
            }
        }
        protected virtual void OnTriggerEnter(Collider other)
        {
            if(this.enabled)
                EnemyAttack(other.gameObject, enemyBase);
        }
        protected virtual void OnParticleCollision(GameObject other)
        {
            if(this.enabled)
                EnemyAttack(other.gameObject, enemyBase);
        }
    }
}
