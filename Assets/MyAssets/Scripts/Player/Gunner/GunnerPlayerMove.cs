using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerSpace
{
    public class GunnerPlayerMove : PlayerMove
    {

        public enum GunType : int
        {
            None = -1,
            Rifle = 0, //���C�t��
            ShotGun,   //�V���b�g�K��
            Pistol,    //�s�X�g��
        }
        [Header("�e�m��p�ϐ�")]
        //���e���˗p�̃N���X
        [SerializeField] private ShootBomb bomb;
        //���x
        [SerializeField, Range(0.01F, 5.0F), Tooltip("���x")] private float sensitivity = 1.0f;

        //�e�̎��
        [SerializeField] private GameObject[] gunVariation = new GameObject[4];
        //���ݏ������Ă���e�̔ԍ�
        [SerializeField] private GunType[] haveGunIndex = { GunType.Rifle, GunType.None };
        //�e�̎�ނ��Ƃ̉�
        [NamedArray(new string[] { "���C�t��", "�V���b�g�K��", "�s�X�g��" })]
        [SerializeField] private AudioClip[] gunSounds = new AudioClip[Enum.GetValues(typeof(GunType)).Length - 1];
        //�e�̈ʒu�ݒ�
        [SerializeField] private Transform gunTransform;
        //�����̓y��(�r)
        [SerializeField] private Transform throwBase;
        //�e�I�u�W�F�N�g
        [SerializeField] private GameObject bulletObject;
        //���ˏꏊ
        [SerializeField] private Transform shotPoint;
        //�}�Y���t���b�V��
        [SerializeField] private GameObject flashObject;
        //�e���܂��ꏊ
        [SerializeField] private Transform backHolster;
        //���e�𓊂���Ƃ��̕������v�Z�p�N���X
        [SerializeField] private DrawArc drawArc;

        //���������ʒu
        [SerializeField] private GameObject throwPoint;
        public GameObject ThrowPoint { get { return throwPoint; } }
        //�e����ւ�
        private bool isGunChange = false;
        //�e�̌��̃\�P�b�g
        private Transform gunSocket;
        //�e�����܂��Ă���Ƃ�
        private bool isBack;
        //�����e�̎�ނ̎w��
        private int gunNumber = 0;
        private Vector2 throwMove = Vector2.zero;
        private Vector2 initMousePos;
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
            throwBase.transform.rotation = Quaternion.Euler(0, 90, 0);
            throwPoint.transform.rotation = Quaternion.Euler(90, 0, 0);
            playerInputs.Player.ThrowMove.performed += OnThrowPointMove;
            playerInputs.Player.ThrowMove.canceled += OnThrowPointMove;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            SetThrowPoint();
            GunChange();
        }
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        //���ˏꏊ�̊p�x�����炷�ۂ̑���
        private void OnThrowPointMove(InputAction.CallbackContext context)
        {
            throwMove = context.ReadValue<Vector2>();
            if (context.canceled)
                throwMove = Vector2.zero;
        }

        private void SetThrowPoint()
        {
            Vector2 value = Vector3.zero;
            //�K�E�Z�̎�
            if (isDeathBlow)
            {
                //���x���l��
                value.x = (initMousePos.x - throwMove.x) * sensitivity;
                value.y = (initMousePos.y - throwMove.y) * sensitivity;
                var qot1 = Quaternion.AngleAxis(value.x, new Vector3(0, 1, 0));
                var qot2 = Quaternion.AngleAxis(value.y, new Vector3(1, 0, 0));
                throwBase.transform.rotation *= qot1;
                throwPoint.transform.rotation *= qot2;
            }
        }

        protected override void OnDeathBlow(InputAction.CallbackContext context)
        {
            base.OnDeathBlow(context);
            if (context.started)
            {
                initMousePos = throwMove;
            }
            if (context.canceled)
            {
                DeathBlowFire();
            }
        }
        private void ShotDone()
        {
            flashObject.SetActive(false);
        }
        private void PlayGunSound()
        {
            audioSourceManager.PlaySE(gunSounds[(int)haveGunIndex[gunNumber]]);
        }

        //�e�e����
        private void BulletFire()
        {
            var spawnBullet = bulletObject;
            var bullet = Instantiate(spawnBullet, shotPoint.position, transform.rotation);
            var damage = bullet.GetComponent<WeaponDamageStock>();
            if (damage != null)
            {
                flashObject.SetActive(true);
                damage.Player = player;
                bullet.GetComponent<Rigidbody>().AddForce(damage.bulletVersion.bulletSpeed * bullet.transform.forward);
                Destroy(bullet, damage.bulletVersion.bulletDestroyTime);
            }
        }

        //�e��ς���Ƃ�
        void GunChange()
        {
            //�e��ύX����ꍇ
            if (isGunChange)
            {
                //���̗v�f�ԍ���ۑ�
                int currentNum = gunNumber;
                //���̔ԍ����v�Z
                int nextNum = gunNumber < gunVariation.Length - 1 ? gunNumber + 1 : 0;
                //���̂���ꍇ
                if (haveGunIndex[nextNum] >= 0)
                {
                    gunVariation[currentNum].SetActive(false);
                    gunVariation[nextNum].SetActive(true);
                    gunVariation[nextNum].transform.SetParent(gunTransform);
                    gunNumber = nextNum;
                }
                isGunChange = false;
            }
        }
        public override void NormalAttack()
        {
            base.NormalAttack();
            anim.SetBool(attackAnimName, true);
        }

        public override void DeathBlowStart()
        {
            base.DeathBlowStart();
            gunVariation[gunNumber].SetActive(false);
        }
        //�e�m�K�E�Z
        public override void DeathBlowFire()
        {
            base.DeathBlowFire();
            AnimReStart();
        }
        private void AnimStop()
        {
            anim.enabled = false;
        }
        private void AnimReStart()
        {
            anim.enabled = true;
        }
    }
}