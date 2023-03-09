using EnemySpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Ability
{
    public class RefusalArea : MonoBehaviour
    {
        //������΂���
        [SerializeField] protected float force = 20;
        //��ɔ�ԗ�
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
                //�擾����
                var enemyBase = other.transform.root.gameObject.GetComponent<EnemyBase>();
                //�擾�ł����A�G���L������ɂ܂������Ă��Ȃ��ꍇ
                if (enemyBase != null)
                {
                    var rb = other.transform.root.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        //���͂ɐ�����΂�
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
            //�������łɋ���͈͓��ɂ����ꍇ�A���x��x������
            if (other.transform.root.gameObject.CompareTag("Enemy") || other.transform.root.gameObject.CompareTag("Boss"))
            {
                //�擾����
                var enemyBase = other.transform.root.gameObject.GetComponent<EnemyBase>();
                //�擾�ł����A�G���L������ɂ܂������Ă��Ȃ��ꍇ
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