using PlayerSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace PlayerSpace
{
    public class StandPlayerMove : PlayerMove
    {

        [Header("�����߂���p�ϐ�")]
        [SerializeField] private StandPlayer standPlayer;
        //�K�E�Z�ŃX�L�������鎞�̃I�u�W�F�N�g
        [SerializeField] private GameObject scanObject;
        //�o��������X�^���h
        [SerializeField] private GameObject standObject;
        public GameObject StandObject { get { return standObject; } }
        //�����̒�ʒu�v���p�e�B
        [SerializeField] private Transform standTransform;
        public Transform StandTransform { get { return standTransform; } }

        private StandMove standMove;
        public StandMove SpawnStandMove { get { return standMove; } }

        //�������o�������ǂ���
        private bool isStand = false;
        private int castAnim = Animator.StringToHash("Cast");
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
            StandManifestation(true);
            standMove = standObject.GetComponent<StandMove>();
            standMove.StandPlayer = this;
            standMove.SPlayer = player;
            //�X�^���h�ƕK�E�Z���\����
            standObject.SetActive(false);
            //�����N��
            playerInputs.Player.RoleSkillOne.performed += OnStand;
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
        public void ScanActive()
        {
            scanObject.SetActive(true);
        }
        /// <summary>
        /// �������o�������B
        /// </summary>
        /// <param name="context"></param>
        private void OnStand(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            anim.SetTrigger(castAnim);
            StandManifestation();
        }
        public override void DamageAnimation()
        {
            if(standMove != null)
                standMove.StandAnimationTrigger(damageAnimName);
        }
        public override void NormalAttack()
        {
            base.NormalAttack();
            anim.SetTrigger(castAnim);
            StandManifestation(true);
            standMove.StandBoolAttack(attackAnimName);
        }
        public override void RoleChangeAfter()
        {
            base.RoleChangeAfter();
            standObject.SetActive(false);
        }
        public override void DeathBlowStart()
        {
            base.DeathBlowStart();
            isDeathBlow = true;
            anim.SetTrigger(castAnim);
            scanObject.SetActive(true);
        }
        public override void DeathBlowPush()
        {

        }
        //�R���{�U��
        public override void DeathBlowFire()
        {
            base.DeathBlowFire();
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="isFire">�X�^���h���o���Ă��Ȃ������ꍇ�ɍU��������</param>
        private void StandManifestation(bool isFire = false)
        {
            //�����������Ă���������A�o�ĂȂ�������o���B
            isStand = isFire ? isFire : !isStand;
            var rot = transform.rotation;
            standObject.transform.position = standTransform.position;
            standObject.transform.rotation = rot;
            standObject.SetActive(isStand);
        }

        protected override void JumpUpdate()
        {
            base.JumpUpdate();
            if (!isGround && standObject.activeSelf)
            {
                if (standMove.StandAnimator != null)
                    standMove.StandAnimator.SetFloat(jumpUpdateAnimName, currentJumpForce);
            }
        }
        protected override void JumpLandCheck()
        {
            base.JumpLandCheck();
            if (standObject.activeSelf && currentJumpForce <= 0)
            {
                standMove.StandAnimator.SetBool(landAnimName, true);
            }
        }
    }
}