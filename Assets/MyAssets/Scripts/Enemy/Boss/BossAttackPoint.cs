using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;
namespace EnemySpace
{
    public class BossAttackPoint : EnemyAttackPoint
    {
        private BossEnemy bossEnemy;
        public BossEnemy Boss { set { bossEnemy = value; } }
        private bool isFireBall = false;
        private Transform target;
        private float speed = 0;
        private float homingTime = 0;
        private float homingMaxTime = 10;
        // Start is called before the first frame update
        protected override void Start()
        {
            if(bossEnemy == null)
                bossEnemy = transform.root.GetComponent<BossEnemy>();
        }

        // Update is called once per frame
        protected override void Update()
        {
            if (bossEnemy != null)
            {
                enabled = bossEnemy.enabled;
            }
            if (isFireBall && homingTime < homingMaxTime)
            {
                homingTime += Time.deltaTime;
                if (target != null)
                {
                    transform.position += ((target.position + (Vector3.up * 5.0f)) - transform.position).normalized * speed * Time.deltaTime;
                }
            }
        }
        public void FireBall(bool fireBall,Transform targetTransform,float fireBallSpeed)
        {
            isFireBall = fireBall;
            speed = fireBallSpeed;
            target = targetTransform;
            if (isFireBall)
            {
                var plus = Vector3.up * 5.0f;
                transform.LookAt(targetTransform.position + plus);
                //var rb = GetComponent<Rigidbody>();
                //rb.AddForce(speed * transform.forward);
                Destroy(gameObject, 5.0f);
            }
        }
        protected override void EnemyAttack(GameObject other, EnemyBase enemyBase = null)
        {
            var player = other.transform.root.GetComponent<Player>();
            var damage = other.transform.root.GetComponent<IDamageble>();
            if (other.transform.root.CompareTag("Player") || other.transform.root.CompareTag("Fellow"))
            {
                if (bossEnemy != null)
                {
                    //攻撃目標にダメージを与える
                    damage.ReceiveDamage(bossEnemy.AttackP, true, bossEnemy.MemoryClassification);
                    if (player != null)
                    {
                        //スクリームを発動したら
                        if (bossEnemy.DoScream)
                        {
                            player.HitScream(bossEnemy.ScreamTime);
                            bossEnemy.DoScream = false;
                        }
                    }
                }
            }
        }
        protected override void OnTriggerEnter(Collider other)
        {
            if (this.enabled)
            {
                if (!other.gameObject.CompareTag("Search"))
                {
                    EnemyAttack(other.gameObject);
                }
            }
        }
        protected override void OnParticleCollision(GameObject other)
        {
            if (this.enabled)
            {
                if (!other.gameObject.CompareTag("Search"))
                {
                    EnemyAttack(other.gameObject);
                }
            }
        }
    }
}