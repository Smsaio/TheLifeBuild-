using PlayerSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using Ability;
using GameManagerSpace;
using Zenject;
using System;

namespace PlayerSpace
{
    /// <summary>
    /// プレイヤー入力や移動関係
    /// </summary>
    public class PlayerMove : MonoBehaviour
    {
        #region Serialize
        [SerializeField] protected PlayerSpecialityController specialityController;
        [SerializeField] protected Player player;
        
        // 地面判定に使うレイヤー
        [Header("地面判定関係"), SerializeField] protected LayerMask groundLayers;
        //地面判定用の有効範囲
        [SerializeField] protected Vector3 groundPositionOffset = new Vector3(0f, 0.02f, 0f);
        //　接地確認の球のコライダの半径
        [SerializeField] protected float groundColliderRadius = 0.29f;
        //逆時間最高回数
        [SerializeField] protected int maxReverceCount = 5;
        //　前方に段差があるか調べるレイを飛ばすオフセット位置
        [Header("階段を判定")]
        [SerializeField] private Vector3 stepRayOffset = new Vector3(0f, 0.05f, 0f);
        //　レイを飛ばす距離
        [SerializeField] private float stepDistance = 0.5f;
        //　昇れる段差
        [SerializeField] private float stepOffset = 0.3f;
        //　昇れる角度
        [SerializeField] protected float slopeLimit = 65f;
        //角度を調べるレイヤー
        [SerializeField] protected LayerMask slopeLayer;
        //　昇れる段差の位置から飛ばすレイの距離
        [SerializeField] protected float slopeDistance = 0.6f;
        //　変更する角度
        [SerializeField] protected float rotateAngle = 45f;
        //　回転スピード
        [SerializeField] protected float rotateSpeed = 1f;
        //加える力
        [SerializeField] protected float force = 20;
        [SerializeField] protected float upwards = 0;
        //壁で重力技が敵に利かないようにする時のレイヤー
        [SerializeField] protected int wallLayer = 12;
        #endregion
        //歩行スピード
        protected float moveSpeed = 5f;
        public float MoveSpeed { set { moveSpeed = value; } get { return moveSpeed; } }
        protected Animator anim;
        public Animator Animator { get { return anim; } }

        // 必殺技のボタンを押したか
        protected bool isDeathBlow = false;
        public bool IsDeathBlow { get { return isDeathBlow; } }

        //クールタイムに入った
        private bool isDeathBlowCool = false;
        public bool IsDeathBlowCool { get { return isDeathBlowCool; } }
        //必殺技使用可能
        private bool canDeathBlow = false;
        public bool CanDeathBlow { get { return canDeathBlow; } set { canDeathBlow = value; } }

        //攻撃した
        protected bool isFire;
        public bool IsFire { get { return isFire; } set { isFire = value; } }
        //攻撃中
        protected bool isAttack = false;
        public bool IsAttack { get { return isAttack; } set { isAttack = value; } }

        //奥義のクールタイム
        private float deathBlowCoolTime = 0f;
        public float DeathBlowCoolTime { get { return deathBlowCoolTime; } }
        private float deathBlowCoolMaxTime = 30f;
        public float DeathBlowCoolMaxTime { get { return deathBlowCoolMaxTime;} }

        //現在のジャンプ力
        protected float currentJumpForce = 0.0f;
        //　接地確認
        protected bool isGround = false;
        //特技のボタンを押している
        protected bool isSpecialityOn = false;
        protected Rigidbody rb;
        //攻撃の少しの隙
        private float attackQuit = 0.3f;
        //動く時のインプットシステム(インプットシステムを使用する場合)
        private Vector2 inputMove = Vector3.zero;
        //移動するための速度
        protected Vector3 velocity;
        // 最高速度
        protected float moveMaxSpeed = 35f;
        //キープする速度
        protected float keepSpeed = 0.0f;

        //重力
        private float gravityPower = -9.8f;
        // 元の回転
        protected Quaternion beforeRot;
        //前フレームの位置
        protected Vector3 beforePos;
        //プレイヤーのインプットシステム使用
        protected PlayerInputAction playerInputs;

        //アニメーションの名前ハッシュ
        protected int dodgeAnimName = Animator.StringToHash("dodgeAnim");
        protected int walkAnimName = Animator.StringToHash("WalkSpeed");
        protected int jumpAnimName = Animator.StringToHash("Jump");
        protected int deathBlowAnimName = Animator.StringToHash("DeathBlow");
        protected int attackAnimName = Animator.StringToHash("BoolAttack");
        protected int jumpUpdateAnimName = Animator.StringToHash("JumpPower");
        protected int landAnimName = Animator.StringToHash("IsGround");
        protected int specialityNumAnimName = Animator.StringToHash("SpecialityNum");
        protected int onSpecialityAnimName = Animator.StringToHash("OnSpeciality");
        protected int damageAnimName = Animator.StringToHash("Damage");
        //ゲームマネージャー制御クラス
        protected IGameManager gameManager = default;
        //音制御クラス
        protected IAudioSourceManager audioSourceManager = default;
        //役割制御管理クラス
        protected IRole role = default;

        [Inject]
        public void Construct(IGameManager IgameManager, IAudioSourceManager IaudioSourceManager, IRole Irole)
        {
            gameManager = IgameManager;
            audioSourceManager = IaudioSourceManager;
            role = Irole;
        }
        protected virtual void Awake()
        {
            // Input Actionインスタンス生成
            playerInputs = new PlayerInputAction();
        }
        protected virtual void OnEnable()
        {
            // 有効化する必要がある
            playerInputs.Enable();
        }
        protected virtual void OnDisable()
        {
            playerInputs.Disable();
        }
        // Start is called before the first frame update
        protected virtual void Start()
        {
            if (player == null)
                player = GetComponent<Player>();
            anim = GetComponent<Animator>();
            if(specialityController == null)
                specialityController = GetComponent<PlayerSpecialityController>();
            rb = GetComponent<Rigidbody>();
            moveMaxSpeed = moveSpeed * 1.25f;
            deathBlowCoolTime = deathBlowCoolMaxTime;
            InputInitialize();
        }
        protected virtual void Update()
        {
            if (gameManager.IsStageClear || gameManager.IsGameOver || gameManager.IsMenu.Value) return;
            DeathBlowCool();
        }
        
        // Update is called once per frame
        protected virtual void FixedUpdate()
        {
            if (gameManager.IsStageClear || gameManager.IsGameOver || gameManager.IsMenu.Value) return;
            JumpUpdate();
            rb.AddForce(Vector3.up * (gravityPower * 100));
            //スクリーム状態だと動きが止まる。
            if (player.IsScream)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                Controller();
            }
        }

        /// <summary>
        /// 必殺技のクールタイム
        /// </summary>
        private void DeathBlowCool()
        {
            //奥義もクールタイムに入る。
            if (isDeathBlowCool && deathBlowCoolTime <= deathBlowCoolMaxTime)
            {
                deathBlowCoolTime += Time.deltaTime;
                if (deathBlowCoolTime >= deathBlowCoolMaxTime)
                {
                    isDeathBlowCool = false;
                }
            }
        }
        /// <summary>
        /// インプットシステム初期化
        /// </summary>
        private void InputInitialize()
        {
            var pInput = GetComponent<PlayerInput>();
            pInput.camera = Camera.main;
            var actionmap = pInput.currentActionMap;
            //特技
            playerInputs.Player.SpecialityOn.performed += OnSpeciality;
            playerInputs.Player.SpecialityOn.canceled += OnSpeciality;
            //通常攻撃
            playerInputs.Player.Fire.started += OnFire;
            //メニューを開く
            playerInputs.Player.Menu.performed += OnMenuOpen;
            //移動
            playerInputs.Player.Move.performed += OnMove;
            playerInputs.Player.Move.canceled += OnMove;
            //各役割の奥義
            playerInputs.Player.RoleDeathBlow.started += OnDeathBlow;
            playerInputs.Player.RoleDeathBlow.performed += OnDeathBlow;
            playerInputs.Player.RoleDeathBlow.canceled += OnDeathBlow;
            //カメラの回転起動
            playerInputs.Player.CameraRotOn.performed += OnCameraRotOn;
            playerInputs.Player.CameraRotOn.canceled += OnCameraRotOn;

            playerInputs.Player.RoleChange.performed += OnRoleChange;
        }

        /// <summary>
        /// 役割を変えた時の処理
        /// </summary>
        public virtual void RoleChangeAfter()
        {
            specialityController.DoneSpeciality();
        }
        /// <summary>
        /// 特技
        /// </summary>
        /// <param name="context"></param>
        private void OnSpeciality(InputAction.CallbackContext context)
        {
            if (specialityController.IsSpeciality) return;
            //発動の障害になる何かの途中では起きないように
            if (specialityController.IsSpecialityCool[specialityController.CurrentSpeciality] || isAttack || player.IsDamage || player.IsDown) return;
            if (context.performed)
            {
                isSpecialityOn = true;
            }
            if (context.canceled)
            {
                //特技が使用可能
                if (specialityController.CanSP[specialityController.NextSpeciality])
                {
                    //各特技のアニメーション
                    anim.SetInteger(specialityNumAnimName, specialityController.NextSpeciality);
                    anim.SetTrigger(onSpecialityAnimName);
                    //特技発動
                    specialityController.UseSpeciality();
                }
                isSpecialityOn = false;
            }
        }
        /// <summary>
        /// カメラを元に戻す
        /// </summary>
        /// <param name="context"></param>
        private void OnCameraRotOn(InputAction.CallbackContext context)
        {
            player.IsCameraRot = true;
            if (context.canceled)
            {
                player.IsCameraRot = false;
            }
        }
        /// <summary>
        /// メニューを開閉
        /// </summary>
        /// <param name="context"></param>
        private void OnMenuOpen(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (gameManager != null)
            {
                //メニュー画面を開く
                gameManager.IsMenu.Value = !gameManager.IsMenu.Value;
            }
        }

        /// <summary>
        /// 方向
        /// </summary>
        /// <param name="context"></param>
        private void OnMove(InputAction.CallbackContext context)
        {
            // 発動の障害になる何かの途中では起きないように
            if (isAttack  || player.IsDown) return;
            inputMove = context.ReadValue<Vector2>();
            if (context.canceled) inputMove = Vector2.zero;
        }
        /// <summary>
        /// 攻撃
        /// </summary>
        /// <param name="context"></param>
        private void OnFire(InputAction.CallbackContext context)
        {
            // 発動の障害になる何かの途中では起きないように
            if (player.IsDamage || player.IsDown || gameManager.IsMenu.Value) return;
            if (context.started)
            {
                if (!isFire)
                    isFire = true;
                //攻撃中
                isAttack = true;
                NormalAttack();
                Observable.Timer(TimeSpan.FromSeconds(attackQuit), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ =>
                {
                    //攻撃してから0.3秒後に切る。攻撃終了
                    isAttack = false;
                }).AddTo(this);
                inputMove = Vector2.zero;
            }
        }

        /// <summary>
        /// 奥義
        /// </summary>
        /// <param name="context"></param>
        protected virtual void OnDeathBlow(InputAction.CallbackContext context)
        {
            // 発動の障害になる何かの途中では起きないように
            if (isDeathBlowCool || isAttack || player.IsDamage || player.IsDown || specialityController.IsSpeciality) return;
            if (context.started)
            {
                DeathBlowStart();
            }
            if (context.performed)
            {
                DeathBlowPush();
            }
            if (context.canceled)
            {
                DeathBlowFire();
            }
        }
        /// <summary>
        /// 役割チェンジ
        /// </summary>
        /// <param name="context"></param>
        private void OnRoleChange(InputAction.CallbackContext context)
        {
            // 発動の障害になる何かの途中では起きないように
            if (specialityController.IsSpecialityCool[specialityController.CurrentSpeciality] || specialityController.IsSpeciality || isDeathBlow || isAttack || player.IsDamage || player.IsLevelUP || player.IsLevelUPContinue) return;
            //まだ役割を変えようとしていないとき
            role.RoleChange();
            RoleChangeAfter();
        }

        ///<summary>
        ///役割ごとの通常攻撃
        ///</summary>
        public virtual void NormalAttack()
        {
            rb.velocity = Vector3.zero;
        }
        /// <summary>
        /// 必殺技開始
        /// </summary>
        public virtual void DeathBlowStart()
        {
            anim.SetTrigger(deathBlowAnimName);
        }
        //必殺技のボタンを押している最中
        public virtual void DeathBlowPush()
        {
            isDeathBlow = true;
        }
        //発動時の役割各自の発動
        public virtual void DeathBlowFire()
        {
            isDeathBlow = false;
            isDeathBlowCool = true;
            deathBlowCoolTime = 0;
        }
        public virtual void DamageAnimation()
        {
            if (!isAttack || !isDeathBlow)
            {
                //攻撃しているか必殺技以外の時に、アニメーション発動
                anim.SetTrigger(damageAnimName);
            }
        }
        //-----コントローラー系------
        /// <summary>
        /// 操作
        /// </summary>
        protected virtual void Controller()
        {
            var currentRot = transform.rotation;
            //最高速度に足していないときの上昇率
            float nomalMulSpeed = 100.0f;
            //ジャンプ中はチェックしない
            isGround = CheckGrounded();
            velocity = Vector3.zero;
            if (isGround && !isDeathBlow)
            {
                anim.SetFloat(jumpUpdateAnimName, 0);
                JumpLandCheck();
                float speedMag = (moveSpeed) - rb.velocity.magnitude;
                var input = new Vector3(inputMove.x, 0f, inputMove.y);
                //特技を使おうとしている
                if (isSpecialityOn)
                {
                    specialityController.PlaySpecialityControll(input, isSpecialityOn);
                }
                else
                {
                    if (input.magnitude > 0)
                    {
                        anim.SetFloat(walkAnimName, input.magnitude, 0.1f, Time.deltaTime);
                        var stepRayPosition = rb.position + stepRayOffset;
                        // カメラの方向から、X-Z平面の単位ベクトルを取得
                        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

                        // 方向キーの入力値とカメラの向きから、移動方向を決定
                        Vector3 moveForward = cameraForward * input.z + Camera.main.transform.right * input.x;

                        // 移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
                        velocity = moveForward * moveSpeed;
                        //敵をロックオンしていない
                        // キャラクターの向きを進行方向に
                        if (moveForward != Vector3.zero)
                        {
                            currentRot = Quaternion.LookRotation(moveForward);
                            transform.rotation = currentRot;
                        }
                        // 坂や階段を検知して坂や階段を上りやすくする　
                        //　ステップ用のレイが地面に接触しているかどうか
                        if (Physics.Linecast(stepRayPosition, stepRayPosition + rb.transform.forward * stepDistance, out var stepHit, slopeLayer))
                        {
                            //　進行方向の地面の角度が指定以下、または昇れる段差より下だった場合の移動処理
                            float stairSpeed = moveSpeed * 2.25f;
                            if (Vector3.Angle(rb.transform.up, stepHit.normal) <= slopeLimit
                            || (Vector3.Angle(rb.transform.up, stepHit.normal) > slopeLimit)
                            && !Physics.Linecast(rb.position + new Vector3(0f, stepOffset, 0f), rb.position + new Vector3(0f, stepOffset, 0f) + rb.transform.forward * slopeDistance))
                            {
                                velocity = new Vector3(0f, (Quaternion.FromToRotation(Vector3.up, stepHit.normal) * rb.transform.forward * stairSpeed).y, 0f) + rb.transform.forward * stairSpeed;
                            }
                            else
                            {
                                velocity = Vector3.zero;
                            }
                        }
                    }
                    else
                    {
                        //現在の入力値も速度ベクトルも0に変更
                        velocity = Vector3.zero;
                        rb.velocity = velocity;
                        //回転値はそのまま
                        transform.rotation = currentRot;
                        anim.SetFloat(walkAnimName, 0);
                    }
                    beforePos = transform.position;
                    //移動速度制限
                    if (rb.velocity.magnitude > (moveSpeed))
                    {
                        rb.AddForce(velocity * (speedMag), ForceMode.Force);
                    }
                    else
                    {
                        rb.AddForce(velocity * (speedMag + nomalMulSpeed), ForceMode.Force);
                    }
                }
            }
        }
        /// <summary>
        /// 接地確認
        /// </summary>
        protected virtual void JumpLandCheck()
        {
            //ジャンプ中であれば着地する
            if (currentJumpForce <= 0)
            {
                anim.SetBool(landAnimName, true);
                currentJumpForce = 0;
            }
        }
        /// <summary>
        /// 空中にいる
        /// </summary>
        protected virtual void JumpUpdate()
        {
            var minusSpeed = 25;
            //地面判定ができない場合かジャンプ中か
            if (!isGround)
            {
                anim.SetBool(landAnimName, false);
                currentJumpForce += Physics.gravity.y * (Time.deltaTime * minusSpeed);                
                anim.SetFloat(jumpUpdateAnimName, currentJumpForce);
            }
        }
        //------地面判定系------
        /// <summary>
        /// 地面に接地しているかどうかを調べる
        /// </summary>
        private bool CheckGrounded()
        {
            //放つ光線の初期位置と姿勢
            //若干身体にめり込ませた位置から発射しないと正しく判定できない時がある
            var rayPosition = transform.position + (Vector3.up * 0.1f);
            var ray = new Ray(rayPosition, Vector3.down);
            RaycastHit hit;
            //探索距離
            var tolerance = 0.85f;
            //ジャンプ中ではない
            if (Physics.Raycast(ray, out hit, tolerance, groundLayers))
            {
                //高さが変わっている場合
                if (transform.position.y != beforePos.y)
                {
                    //レイで算出したy座用に設定
                    transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                }
                return true;
            }
            return false;
        }
    }
}