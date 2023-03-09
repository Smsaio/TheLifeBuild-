using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerSpace
{
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
        ///Œ•‚Å‚Ì•KŽE‹Z
        ///</summary>
        public override void DeathBlowFire()
        {
            base.DeathBlowFire();
        }
    }
}