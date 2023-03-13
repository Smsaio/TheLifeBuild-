using Ability;
using System;
using PlayerSpace;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using UniRx;

public class FollowCameraNIS : MonoBehaviour,IReactiveProperty
{
    // 単一のパーリンノイズ情報を格納する構造体
    [Serializable]
    private struct NoiseParam
    {
        // 振幅
        public float amplitude;

        // 振動の速さ
        public float speed;

        // パーリンノイズのオフセット
        [NonSerialized] public float offset;

        // 乱数のオフセット値を指定する
        public void SetRandomOffset()
        {
            offset = UnityEngine.Random.Range(0f, 256f);
        }

        // 指定時刻のパーリンノイズ値を取得する
        public float GetValue(float time)
        {
            // ノイズ位置を計算
            var noisePos = speed * time + offset;

            // -1～1の範囲のノイズ値を取得
            var noiseValue = 2 * (Mathf.PerlinNoise(noisePos, 0) - 0.5f);

            // 振幅を掛けた値を返す
            return amplitude * noiseValue;
        }
    }

    // パーリンノイズのXYZ情報
    [Serializable]
    private struct NoiseTransform
    {
        public NoiseParam x, y, z;

        // xyz成分に乱数のオフセット値を指定する
        public void SetRandomOffset()
        {
            x.SetRandomOffset();
            y.SetRandomOffset();
            z.SetRandomOffset();
        }

        // 指定時刻のパーリンノイズ値を取得する
        public Vector3 GetValue(float time)
        {
            return new Vector3(
                x.GetValue(time),
                y.GetValue(time),
                z.GetValue(time)
            );
        }
    }

    [Header("各設定")]
    [SerializeField] private float distance = 10.0f;
    [Header("横や縦の最大、最小数")]
    [SerializeField] private float horizontalAngle = 0.0f;
    [SerializeField] private float verticalAngle = 10.0f;

    //------- カメラの移動限界
    //見上げ限界角度
    [SerializeField] private float verticalAngleMinLimit = -30f;
    // 見下ろし限界角度 
    [SerializeField] private float verticalAngleMaxLimit = 80f;
    // 最大ズーム距離 
    [SerializeField] private float maxDistance = 20f;
    // 最小ズーム距離
    [SerializeField] private float minDistance = 0.6f;
    // 画面の横幅分カーソルを移動させたとき何度回転するか.
    [SerializeField] private float rotationSpeed = 180.0f;
    // 回転の減衰速度 (higher = faster) 
    [SerializeField] private float rotationDampening = 0.5f;
    [SerializeField] private float zoomDampening = 5.0f;

    // 衝突検知用
    [SerializeField] private LayerMask collisionLayers = -1;
    // 衝突する物体からカメラを遠ざけるときのオフセット 
    [SerializeField] private float offsetFromWall = 0.1f;

    [Header("揺れの位置や回転")]
    // 位置の揺れ情報
    [SerializeField] private NoiseTransform _noisePosition;
    // 回転の揺れ情報
    [SerializeField] private NoiseTransform _noiseRotation;
    //ミニマップ用カメラ
    [SerializeField] private MiniMapMove miniMapCamera;
    // カメラ回転後のカメラフォローまでの遅延の待機時間
    [Range(1.0f,10.0f),SerializeField] private float cameraFollowTime = 1.0f;

    // カメラ回転後のカメラフォローまでの遅延
    private float cameraFollowDelay;
    public float CameraFollowDelay { get { return cameraFollowDelay; } }
    // ターゲット
    private Transform target;
    // Transformの初期状態
    private Vector3 _initPosition;
    private Quaternion _initQuaternion;
    private bool isInitialize = false;
    private float targetHeight = 5.0f;
    // 現在のカメラ距離
    private float currentDistance;
    // 目標とするカメラ距離
    private float desiredDistance;
    // 矯正後のカメラ距離
    private float correctedDistance;
    private PlayerInputAction playerInputs;
    private Player currentPlayer;
    private Vector2 cameraRotateMoveAxis;
    private IRole role = default;
    [Inject]
    public void Construct(IRole Irole)
    {
        role = Irole;
    }
    public virtual void Awake()
    {
        // Input Actionインスタンス生成
        playerInputs = new PlayerInputAction();
    }
    private void OnEnable()
    {
        playerInputs.Enable();
    }
    private void OnDisable()
    {
        playerInputs.Disable();
    }
    void Start()
    {
        //ReactivePlayer(role);
        //縦と横の角度設定
        Vector3 angles = transform.eulerAngles;
        horizontalAngle = angles.x;
        verticalAngle = angles.y;
        //カメラのターゲットからの距離
        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;
        cameraFollowDelay = 0f;
        playerInputs.Camera.CameraRot.performed += OnCameraPosition;
        playerInputs.Camera.CameraRot.canceled += OnCameraPosition;
        RotateBehindTarget();
    }
    private void Update()
    {
        
    }
    public void ReactivePlayer(IRole role)
    {
        if (role == null) return;
        role.CurrentPlayerTransform.Subscribe(playerTransform => { target = playerTransform; }).AddTo(this);
        role.CurrentPlayer.Subscribe(player => { currentPlayer = player; }).AddTo(this);
    }
    // Updateより後に実行される
    void LateUpdate()
    {
        // ターゲットが定義されていない場合は何もしない
        if (target == null) return;
        Vector3 vTargetOffset; // ターゲットからのオフセット
        if (GUIUtility.hotControl == 0)
        {
#if UNITY_STANDALONE
            if (currentPlayer.IsCameraRot)
            {
                horizontalAngle += cameraRotateMoveAxis.x * rotationSpeed * 0.001f;
                verticalAngle += cameraRotateMoveAxis.y * rotationSpeed * 0.001f;
            }
#else
            horizontalAngle += cameraRotateMoveAxis.x * rotationSpeed * 0.001f;
            verticalAngle += cameraRotateMoveAxis.y * rotationSpeed * 0.001f;
#endif
            if (cameraRotateMoveAxis.x != 0f && cameraRotateMoveAxis.y != 0f) cameraFollowDelay = cameraFollowTime;
            if (cameraFollowDelay > 0f)
            {
                cameraFollowDelay -= Time.deltaTime;
            }
            verticalAngle = ClampAngle(verticalAngle, verticalAngleMinLimit, verticalAngleMaxLimit);

            // カメラの向きを設定
            Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);

            // 希望のカメラ位置を計算
            vTargetOffset = new Vector3(0, -targetHeight, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + vTargetOffset);

            // 高さを使ってユーザーが設定した真のターゲットの希望の登録点を使って衝突をチェック
            RaycastHit collisionHit;
            Vector3 trueTargetPosition = new Vector3(target.position.x,
                target.position.y + targetHeight, target.position.z);

            // 衝突があった場合は、カメラ位置を補正し、補正後の距離を計算
            var isCorrected = false;
            if (Physics.Linecast(trueTargetPosition, position, out collisionHit, collisionLayers))
            {
                // 元の推定位置から衝突位置までの距離を計算し、衝突した物体から安全な「オフセット」距離を差し引く
                // このオフセットは、カメラがヒットした面の真上にいないよう逃がす距離
                correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
                isCorrected = true;
            }

            // スムージングのために、距離が補正されていないか、または補正された距離が現在の距離より
            // も大きい場合にのみ、距離を返す。
            currentDistance = !isCorrected || correctedDistance > currentDistance
                ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening)
                : correctedDistance;

            // 限界を超えないようにする
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

            // 新しい currentDistance に基づいて位置を再計算する。
            position = target.position - (rotation * Vector3.forward * currentDistance + vTargetOffset);

            Shake(transform);
            //パーリンノイズされていない場合
            if (!isInitialize)
            {
                // 最後にカメラの回転と位置を設定。
                transform.rotation = rotation;
                transform.position = position;
            }
        }
    }
    /// <summary>
    /// マウスでカメラ移動をするときの入力
    /// </summary>
    /// <param name="context"></param>
    private void OnCameraPosition(InputAction.CallbackContext context)
    {
        cameraRotateMoveAxis = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// カメラを揺らす揺らす
    /// </summary>
    /// <param name="perlinTransform"></param>
    private void Shake(Transform perlinTransform)
    {
        if (currentPlayer.IsDamage)
        {
            //初期化してないとき
            if (!isInitialize)
            {
                PerlinInitialize(perlinTransform);
            }
            // ゲーム開始からの時間取得
            var time = Time.time;

            // パーリンノイズの値を時刻から取得
            var noisePos = _noisePosition.GetValue(time);
            var noiseRot = _noiseRotation.GetValue(time);

            // 各Transformにパーリンノイズの値を加算
            perlinTransform.position = _initPosition + noisePos;
            perlinTransform.rotation = Quaternion.Euler(noiseRot) * _initQuaternion;
        }
        else
        {
            isInitialize = false;
        }
    }
    /// <summary>
    /// パーリンノイズをするための初期化
    /// </summary>
    /// <param name="initTransform"></param>
    private void PerlinInitialize(Transform initTransform)
    {
        isInitialize = true;
        // Transformの初期値を保持
        _initPosition = initTransform.position;
        _initQuaternion = initTransform.rotation;

        // パーリンノイズのオフセット初期化
        _noisePosition.SetRandomOffset();
        _noiseRotation.SetRandomOffset();
    }

    /// <summary>
    /// カメラを背後にまわす。
    /// </summary>
    private void RotateBehindTarget()
    {
        float targetRotationAngle = target.eulerAngles.y;
        float currentRotationAngle = transform.eulerAngles.y;
        horizontalAngle = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
    }

    /// <summary>
    /// 角度クリッピング
    /// </summary>
    /// <param name="angle">角度</param>
    /// <param name="min">最低角度</param>
    /// <param name="max">最高角度</param>
    /// <returns></returns>
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}