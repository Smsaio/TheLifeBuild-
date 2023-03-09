using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// 剣士の必殺技地面を這う必殺技
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WeaponDamageStock))]
public class GroundSlash : MonoBehaviour
{
    //速度ベクトル
    [Header("速度ベクトル"),Range(15.0f,100.0f),SerializeField] private float moveSpeed = 20f;
    //地面を調べるレイの最高の長さ
    [Header("地面を調べるレイの最高の長さ"), Range(3.0f,8.0f),SerializeField] private float groundDetectingDistance = 6f;
    //地面を調べる
    [Header("地面を調べるレイヤー"), SerializeField] private LayerMask groundLayer;
    //スラッシュエフェクト
    [Header("スラッシュエフェクト"), SerializeField] private VisualEffect slashEffect;
    //完全に失速するまでの時間
    [Header("完全に失速するまでの時間"), Range(1.0f,4.0f),SerializeField] private float maxSlowDownTime = 1.0f;
    private Rigidbody rb;
    public Rigidbody Rb { get { return rb; } set { rb = value; } }
    //初期化しているか
    private bool isInitialize;
    public bool IsInitialize { get { return isInitialize; } set { isInitialize = value; } }

    private bool isStopped;
    //遅くなる
    private float slowDownTime = 0.0f;
    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();        
    }
    private void Start()
    {
        slowDownTime = maxSlowDownTime;
    }

    private void FixedUpdate()
    {
        if (isInitialize)
        {
            SlowDown();
            SnapToFloor();
        }
    }
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="origin">出現位置</param>
    /// <param name="rot">回転値</param>
    /// <param name="mulSpeed">速度</param>
    public void Initialize(Transform origin, Vector3 forward, float rot = 0,int mulSpeed = 1)
    {
        var qua = Quaternion.Euler(0,rot, 0);
        isStopped = false;
        slowDownTime = maxSlowDownTime;
        transform.rotation = qua;
        transform.position = origin.position + forward;
        //前に進ませる
        rb.velocity = transform.forward * (moveSpeed * mulSpeed);
    }
    /// <summary>
    /// 床をはって移動
    /// </summary>
    private void SnapToFloor()
    {
        RaycastHit hit;
        var rayOrigin = transform.position + (Vector3.up * 0.1f);
        var direction = transform.position + (Vector3.up * -1);
        //停止していない
        if (!isStopped)
        {
            //地面の判定
            bool didHitFloor = Physics.Raycast(rayOrigin, direction,
                 out hit, groundDetectingDistance,
                groundLayer
            );
            //床であった時に動く
            transform.position = new Vector3(transform.position.x,didHitFloor ? hit.point.y : transform.position.y,transform.position.z);
        }
    }
    /// <summary>
    /// 速度を徐々に遅く
    /// </summary>
    void SlowDown()
    {
        //0に近づける
        rb.velocity = Vector3.Lerp(Vector3.zero, rb.velocity, slowDownTime);
        slowDownTime -= Time.deltaTime;
        //完全に失速すれば非表示
        if (slowDownTime <= 0)
        {
            isStopped = true;
            slashEffect.Stop();
            gameObject.SetActive(false);
        }
    }
}
