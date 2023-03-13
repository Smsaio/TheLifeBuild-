using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerSpace
{
    /// <summary>
    /// プレイヤー入力や移動関係(銃士)
    /// </summary>
    public class GunnerPlayerMove : PlayerMove
    {
        public enum GunType : int
        {
            None = -1,
            Rifle = 0, //ライフル
            ShotGun,   //ショットガン
            Pistol,    //ピストル
        }
        [Header("銃士専用変数")]
        //爆弾発射用のクラス
        [SerializeField] private ShootBomb bomb;
        //感度
        [SerializeField, Range(0.01F, 5.0F), Tooltip("感度")] private float sensitivity = 1.0f;

        //銃の種類
        [SerializeField] private GameObject[] gunVariation = new GameObject[4];
        //現在所持している銃の番号
        [SerializeField] private GunType[] haveGunIndex = { GunType.Rifle, GunType.None };
        //銃の種類ごとの音
        [NamedArray(new string[] { "ライフル", "ショットガン", "ピストル" })]
        [SerializeField] private AudioClip[] gunSounds = new AudioClip[Enum.GetValues(typeof(GunType)).Length - 1];
        //銃の位置設定
        [SerializeField] private Transform gunTransform;
        //投擲の土台(腕)
        [SerializeField] private Transform throwBase;
        //弾オブジェクト
        [SerializeField] private GameObject bulletObject;
        //発射場所
        [SerializeField] private Transform shotPoint;
        //マズルフラッシュ
        [SerializeField] private GameObject flashObject;
        //銃しまう場所
        [SerializeField] private Transform backHolster;
        //爆弾を投げるときの放物線計算用クラス
        [SerializeField] private DrawArc drawArc;

        //投擲初期位置
        [SerializeField] private GameObject throwPoint;
        public GameObject ThrowPoint { get { return throwPoint; } }
        //銃入れ替え
        private bool isGunChange = false;
        //銃の元のソケット
        private Transform gunSocket;
        //銃をしまっているとき
        private bool isBack;
        //所持銃の種類の指定
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

        //発射場所の角度をずらす際の操作
        private void OnThrowPointMove(InputAction.CallbackContext context)
        {
            throwMove = context.ReadValue<Vector2>();
            if (context.canceled)
                throwMove = Vector2.zero;
        }
        /// <summary>
        /// 必殺技の爆弾を投げる位置設定
        /// </summary>
        private void SetThrowPoint()
        {
            Vector2 value = Vector3.zero;
            //必殺技の時
            if (isDeathBlow)
            {
                //感度も考慮
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
        /// <summary>
        /// 弾発射し終えた
        /// </summary>
        private void ShotDone()
        {
            flashObject.SetActive(false);
        }
        /// <summary>
        /// 銃の音
        /// </summary>
        private void PlayGunSound()
        {
            audioSourceManager.PlaySE(gunSounds[(int)haveGunIndex[gunNumber]]);
        }

        /// <summary>
        /// 銃弾発射
        /// </summary>
        private void BulletFire()
        {
            var spawnBullet = bulletObject;
            var bullet = Instantiate(spawnBullet, shotPoint.position, transform.rotation);
            var damage = bullet.GetComponent<WeaponDamageStock>();
            PlayGunSound();
            if (damage != null)
            {
                flashObject.SetActive(true);
                damage.Player = player;
                bullet.GetComponent<Rigidbody>().AddForce(damage.bulletVersion.bulletSpeed * bullet.transform.forward);
                Destroy(bullet, damage.bulletVersion.bulletDestroyTime);
            }
        }

        /// <summary>
        /// 銃を変えるとき
        /// </summary>
        void GunChange()
        {
            //銃を変更する場合
            if (isGunChange)
            {
                //今の要素番号を保存
                int currentNum = gunNumber;
                //次の番号を計算
                int nextNum = gunNumber < gunVariation.Length - 1 ? gunNumber + 1 : 0;
                //次のある場合
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
        //銃士必殺技
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