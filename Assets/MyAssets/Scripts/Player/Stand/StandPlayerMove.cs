using PlayerSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace PlayerSpace
{
    /// <summary>
    /// プレイヤー入力や移動関係(悪魔憑き)
    /// </summary>
    public class StandPlayerMove : PlayerMove
    {

        [Header("悪魔憑き専用変数")]
        [SerializeField] private StandPlayer standPlayer;
        //必殺技でスキャンする時のオブジェクト
        [SerializeField] private GameObject scanObject;
        //出現させるスタンド
        [SerializeField] private GameObject standObject;
        public GameObject StandObject { get { return standObject; } }
        //悪魔の定位置プロパティ
        [SerializeField] private Transform standTransform;
        public Transform StandTransform { get { return standTransform; } }

        private StandMove standMove;
        public StandMove SpawnStandMove { get { return standMove; } }

        //悪魔を出したかどうか
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
            //スタンドと必殺技を非表示に
            standObject.SetActive(false);
            //悪魔起動
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
        /// 悪魔を出す処理。
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
        //コンボ攻撃
        public override void DeathBlowFire()
        {
            base.DeathBlowFire();
        }

        /// <summary>
        /// 悪魔顕現
        /// </summary>
        /// <param name="isFire">スタンドを出していなかった場合に攻撃をした</param>
        private void StandManifestation(bool isFire = false)
        {
            //悪魔をだしていたら消す、出てなかったら出す。
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