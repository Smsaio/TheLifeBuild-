using EnemySpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Ability
{
    public class RefusalArea : MonoBehaviour
    {
        //吹き飛ばす力
        [SerializeField] protected float force = 20;
        //上に飛ぶ力
        [SerializeField] protected float upwards = 0;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Search") || !this.gameObject.activeSelf) return;
            if (other.transform.root.gameObject.CompareTag("Enemy") || other.transform.root.gameObject.CompareTag("Boss"))
            {
                //取得して
                var enemyBase = other.transform.root.gameObject.GetComponent<EnemyBase>();
                //取得できた、敵が記憶拒絶にまだ入っていない場合
                if (enemyBase != null)
                {
                    var rb = other.transform.root.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        //周囲に吹き飛ばす
                        rb.isKinematic = false;
                        rb.AddExplosionForce(force, transform.position, transform.localScale.x, upwards, ForceMode.Impulse);
                    }
                    if (!enemyBase.IsHitRefusal)
                        enemyBase.IsHitRefusal = true;
                }
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("Search") || !this.gameObject.activeSelf) return;
            //もうすでに拒絶範囲内にいた場合、速度を遅くする
            if (other.transform.root.gameObject.CompareTag("Enemy") || other.transform.root.gameObject.CompareTag("Boss"))
            {
                //取得して
                var enemyBase = other.transform.root.gameObject.GetComponent<EnemyBase>();
                //取得できた、敵が記憶拒絶にまだ入っていない場合
                if (enemyBase != null)
                {
                    if (!enemyBase.IsHitRefusal)
                    {
                        enemyBase.IsHitRefusal = true;
                    }
                }
            }
        }
    }
}