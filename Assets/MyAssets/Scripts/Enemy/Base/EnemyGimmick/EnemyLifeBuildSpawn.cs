using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemySpace;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Dissolve))]
public class EnemyLifeBuildSpawn : MonoBehaviour
{
    // Materialにアクセスする容器
    [Header("召喚させるか判断するレンダラー"),SerializeField] private Renderer fadeRenderer;
    //人生の記憶の結晶
    [Header("召喚させるライフビルド"),SerializeField] private GameObject lifeBuildObject;
    //地面のレイヤー
    [SerializeField]  private LayerMask groundLayer;
    [Header("ライフビルドを召喚するレンダラーの透明度"),Range(0.0f,0.35f),SerializeField] private float spawnDuration = 0.0f;
    //このオブジェクトが司っている負の記憶
    private MemoryType.NegativeMemory lifeBuildInMemory = MemoryType.NegativeMemory.Hatred;
    public MemoryType.NegativeMemory LifeBuildInMemory { set { lifeBuildInMemory = value; } }
    //敵に設定されていた経験値をライフビルドに入れる
    private int keepEXP = 100;
    //召喚される時に経験値を代入する
    public int KeepEXP { set { keepEXP = value; } }
    //マテリアルのパラメータ
    private string progressParamName = "_Progress";
    //レンダラーのマテリアル
    private Material[] mats;
    //召喚する位置
    private Vector3 spawnPos;

    private void Start()
    {
        mats = fadeRenderer.materials;
        // Materialの描画をオンにする
        fadeRenderer.enabled = true;
        var dissolve = GetComponent<Dissolve>();
        if (dissolve != null)
        {
            if (progressParamName != dissolve.ProgressParamName)
            {
                progressParamName = dissolve.ProgressParamName;
            }
        }
        Ray ray = new(transform.position, -transform.up);
        RaycastHit hit;
        //下にレイを発射して確認
        if (Physics.Raycast(ray, out hit,groundLayer))
        {
            spawnPos = hit.point;
        }
    }
    private void Update()
    {
        LifeBuildSpawn();
    }
    /// <summary>
    /// 出現させる条件確認
    /// </summary>
    private void LifeBuildSpawn()
    {        
        //メッシュを徐々に消す
        for (int i = 0; i < mats.Length; i++)
        {
            if(mats[i] != null)
            {
                if(mats[i].GetFloat(progressParamName) <= spawnDuration)
                {
                    Spawn(7);
                }
            }
        }        
    }
    /// <summary>
    /// ライフビルドを出現させる
    /// </summary>
    private void Spawn(float plusY=6)
    {
        //ライフビルドのオブジェクトを召喚
        var lifeObject = Instantiate(lifeBuildObject, new Vector3(spawnPos.x, spawnPos.y + plusY, spawnPos.z), Quaternion.identity);
        var lifeBuild = lifeObject.GetComponent<LifeBuild>();
        //ライフビルドの管理クラスを取得出来た
        if (lifeBuild != null)
        {
            //負の記憶の種類
            lifeBuild.MyMemory = lifeBuildInMemory;
            //経験値
            lifeBuild.EXP = keepEXP;
        }
        Destroy(gameObject);
    }
}