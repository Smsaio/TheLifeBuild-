using System.Collections;
using System.Collections.Generic;
using PlayerSpace;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// 予測線
/// </summary>
public class DrawArc : MonoBehaviour
{
    // 放物線のMaterial
    [SerializeField, Tooltip("放物線のマテリアル")] private Material arcMaterial;
    // 放物線の幅
    [SerializeField, Tooltip("放物線の幅")] private float arcWidth = 0.02F;
    //座標補正(そのままだと小さすぎるため)
    [Range(10.0f,60.0f),SerializeField] private float arcSensitivity = 50.0f;
    [SerializeField] private LayerMask layer = -1;
    [SerializeField] private GunnerPlayerMove gunnerPlayerMove;

    //着弾マーカーオブジェクトのPrefab
    [SerializeField, Tooltip("着弾地点に表示するマーカーのPrefab")] private GameObject pointerPrefab;
    public  GameObject Pointer { get { return pointerPrefab; } }
    //放物線を構成するLineRenderer
    private LineRenderer[] lineRenderers;
    // 弾の初速度や生成座標を持つコンポーネント
    private ShootBomb shootBomb;
    //弾の初速度
    private Vector3 initialVelocity;
    //放物線の開始座標
    private Vector3 arcStartPosition;
    // 着弾点のマーカーのオブジェクト
    private GameObject pointerObject;
    // 放物線を構成する線分の数
    private int segmentCount = 60;
    // 放物線を何秒分計算するか
    private float predictionTime = 6.0f;
    void Start()
    {
        // 放物線のLineRendererオブジェクトを用意
        CreateLineRendererObjects();

        // マーカーのオブジェクトを用意
        pointerObject = Instantiate(pointerPrefab, Vector3.zero, Quaternion.identity);
        pointerObject.SetActive(false);

        // 弾の初速度や生成座標を持つコンポーネント
        shootBomb = GetComponent<ShootBomb>();
    }
    void Update()
    {
        LineArcPoint();
    }
    /// <summary>
    /// 全体のラインを描画
    /// </summary>
    private void LineArcPoint()
    {
        // 初速度と放物線の開始座標を更新
        initialVelocity = shootBomb.ShootVelocity;
        arcStartPosition = shootBomb.InstantiatePosition;
        if (gunnerPlayerMove.IsDeathBlow)
        {
            // 放物線を表示
            float timeStep = predictionTime / segmentCount;
            bool draw = false;
            float hitTime = float.MaxValue;
            for (int i = 0; i < segmentCount; i++)
            {
                // 線の座標を更新
                float startTime = timeStep * i;
                float endTime = startTime + timeStep;
                SetLineRendererPosition(i, startTime, endTime, !draw);

                // 衝突判定
                if (!draw)
                {
                    hitTime = GetArcHitTime(startTime, endTime);
                    if (hitTime != float.MaxValue)
                    {
                        draw = true; // 衝突したらその先の放物線は表示しない
                    }
                }
            }

            // マーカーの表示
            if (hitTime != float.MaxValue)
            {
                Vector3 hitPosition = GetArcPositionAtTime(hitTime);
                ShowPointer(hitPosition);
            }
        }
        else
        {
            // 放物線とマーカーを表示しない
            for (int i = 0; i < lineRenderers.Length; i++)
            {
                lineRenderers[i].enabled = false;
            }
            pointerObject.SetActive(false);
        }
    }

    /// <summary>
    /// 指定時間に対するアーチの放物線上の座標を返す
    /// </summary>
    /// <param name="time">経過時間</param>
    /// <returns>座標</returns>
    private Vector3 GetArcPositionAtTime(float time)
    {
        return (arcStartPosition + ((initialVelocity * time) + (0.5f * time * time) * Physics.gravity) * arcSensitivity);
    }

    /// <summary>
    /// LineRendererの座標を更新
    /// </summary>
    /// <param name="index"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    private void SetLineRendererPosition(int index, float startTime, float endTime, bool draw = true)
    {
        lineRenderers[index].SetPosition(0, GetArcPositionAtTime(startTime));
        lineRenderers[index].SetPosition(1, GetArcPositionAtTime(endTime));
        lineRenderers[index].enabled = draw;
    }

    /// <summary>
    /// LineRendererオブジェクトを作成
    /// </summary>
    private void CreateLineRendererObjects()
    {
        // 親オブジェクトを作り、LineRendererを持つ子オブジェクトを作る
        GameObject arcObjectsParent = new GameObject("ArcObject");

        lineRenderers = new LineRenderer[segmentCount];
        //放物線の数、設定と描画をする
        for (int i = 0; i < segmentCount; i++)
        {
            GameObject newObject = new GameObject("LineRenderer_" + i);
            newObject.transform.SetParent(arcObjectsParent.transform);
            lineRenderers[i] = newObject.AddComponent<LineRenderer>();

            // 光源関連を使用しない
            lineRenderers[i].receiveShadows = false;
            lineRenderers[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
            lineRenderers[i].lightProbeUsage = LightProbeUsage.Off;
            lineRenderers[i].shadowCastingMode = ShadowCastingMode.Off;

            // 線の幅とマテリアル
            lineRenderers[i].material = arcMaterial;
            lineRenderers[i].startWidth = arcWidth;
            lineRenderers[i].endWidth = arcWidth;
            lineRenderers[i].numCapVertices = 5;
            lineRenderers[i].enabled = false;
        }
    }

    /// <summary>
    /// 指定座標にマーカーを表示
    /// </summary>
    /// <param name="position"></param>
    private void ShowPointer(Vector3 position)
    {
        pointerObject.transform.position = position;
        pointerObject.SetActive(true);
    }

    /// <summary>
    /// 2点間の線分で衝突判定し、衝突する時間を返す
    /// </summary>
    /// <returns>衝突した時間(してない場合はfloat.MaxValue)</returns>
    private float GetArcHitTime(float startTime, float endTime)
    {
        // Linecastする線分の始終点の座標
        Vector3 startPosition = GetArcPositionAtTime(startTime);
        Vector3 endPosition = GetArcPositionAtTime(endTime);
        // 衝突判定
        RaycastHit hitInfo;
        if (Physics.Linecast(startPosition, endPosition, out hitInfo, layer))
        {
            // 衝突したColliderまでの距離から実際の衝突時間を算出
            float distance = Vector3.Distance(startPosition, endPosition);
            return startTime + (endTime - startTime) * (hitInfo.distance / distance);
        }
        return float.MaxValue;
    }
}
