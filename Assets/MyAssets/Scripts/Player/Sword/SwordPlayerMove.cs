using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSpace
{
    /// <summary>
    /// プレイヤー入力や移動関係(剣士)
    /// </summary>
    public class SwordPlayerMove : PlayerMove
    {
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void NormalAttack()
        {
            base.NormalAttack();
            if (isAttack)
            {
                anim.SetBool(attackAnimName, true);
            }
        }
        public override void DeathBlowStart()
        {
            base.DeathBlowStart();
        }
        ///<summary>
        ///剣での必殺技
        ///</summary>
        public override void DeathBlowFire()
        {
            base.DeathBlowFire();
        }
    }
}