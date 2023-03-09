using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerSpace;
using EnemySpace;
using Ability;
using System;
using Zenject;
/// <summary>
/// 敵から出る宝石(負の記憶が付与されている経験値を取得)
/// </summary>
public class LifeBuild : MonoBehaviour
{    
    /*
     憎しみや、怒りの壮絶な記憶であれば、攻撃力が
     悲壮感、悲哀の悲しみの記憶であれば防御力が、
     絶望、不安感のつらい記憶であれば体力がプラスされる
    */
    //ライフビルドのパーティクル
    [Header("注目させるためのパーティクル"),SerializeField] private GameObject convergeParticle;
    //敵から出てくるライフビルドの負の記憶
    [Header("宝箱から出る場合の負の記憶"),SerializeField] private MemoryType.NegativeMemory memory = MemoryType.NegativeMemory.Hatred;
    public MemoryType.NegativeMemory MyMemory { get { return memory; } set { memory = value; } }
    //ライフビルドで増える数
    [SerializeField] private int plusMemoryCount = 1;
    public int PlusMemoryCount { get { return plusMemoryCount; } set { plusMemoryCount = value; } }

    //宝箱から出る場合設定
    [Header("宝箱から出る場合の経験値"), SerializeField] private int exp = 100;

    //ライフビルドのレンダラー
    [SerializeField] private Renderer[] lifeBuildRenderer;
    //敵から出るとき設定
    public int EXP { set { exp = value; } }
    
    // Start is called before the first frame update
    void Start()
    {
        //ライフビルドの負の記憶によって色が変わるマテリアルの数
        int lifeBuildMatsCount;
        for (int k = 0; k < lifeBuildRenderer.Length; k++)
        {
            lifeBuildMatsCount = lifeBuildRenderer[k].materials.Length;
            switch (memory)
            {
                case MemoryType.NegativeMemory.Hatred:
                case MemoryType.NegativeMemory.Angry:
                    //ライフビルドを赤色に
                    for (int i = 0; i < lifeBuildMatsCount; i++)
                    {
                        lifeBuildRenderer[k].materials[i].color = Color.red;
                    }
                    break;
                case MemoryType.NegativeMemory.Tragic:
                case MemoryType.NegativeMemory.Sorrowful:
                    //ライフビルドを青色に
                    for (int i = 0; i < lifeBuildMatsCount; i++)
                    {
                        lifeBuildRenderer[k].materials[i].color = Color.blue;
                    }
                    break;
                case MemoryType.NegativeMemory.Despair:
                case MemoryType.NegativeMemory.Uneasiness:
                    //ライフビルドを黄色に
                    for (int i = 0; i < lifeBuildMatsCount; i++)
                    {
                        lifeBuildRenderer[k].materials[i].color = Color.yellow;
                    }
                    break;
                case MemoryType.NegativeMemory.Trauma:
                case MemoryType.NegativeMemory.Darkhistory:
                    //ライフビルドを黒色に
                    for (int i = 0; i < lifeBuildMatsCount; i++)
                    {
                        lifeBuildRenderer[k].materials[i].color = Color.black;
                    }
                    break;
                default:
                    //ライフビルドを白色に
                    for (int i = 0; i < lifeBuildMatsCount; i++)
                    {
                        lifeBuildRenderer[k].materials[i].color = Color.white;
                    }
                    break;
            }
        }
    }
    void Update()
    {
        
    }
    /// <summary>
    ///(hatred、angry)壮絶な記憶が、攻撃力
    ///(tragic、sorrowful)悲しみの記憶が、防御力
    ///(uneasiness、despair)つらい記憶が、体力を成長させる
    /// </summary>
    private void LifebuildMemory(Player player,PlayerSpecialityController playerSpecialityController)
    {
        if (player != null && playerSpecialityController != null)
        {
            switch (memory)
            {
                //壮絶な記憶(上から憎しみ、怒り)
                case MemoryType.NegativeMemory.Hatred:
                case MemoryType.NegativeMemory.Angry:
                    //記憶停滞が使用可能になるためのカウントが1増える
                    player.PointUP(Mathf.CeilToInt(exp / 50), ItemType.ITEMTYPE.PowerUP);
                    if (!playerSpecialityController.CanSP[(int)Attach.Speciality.Stagnation])
                        playerSpecialityController.NegativeMemoryCount[(int)Attach.Speciality.Stagnation] += plusMemoryCount;
                    break;
                //悲しみの記憶の分類(上から悲壮感、悲哀)
                case MemoryType.NegativeMemory.Tragic:
                case MemoryType.NegativeMemory.Sorrowful:
                    //記憶拒絶が使用可能になるためのカウントが1増える
                    player.PointUP(Mathf.CeilToInt(exp / 50), ItemType.ITEMTYPE.DefenceUP);
                    if (!playerSpecialityController.CanSP[(int)Attach.Speciality.Refusal])
                        playerSpecialityController.NegativeMemoryCount[(int)Attach.Speciality.Refusal] += plusMemoryCount;
                    break;
                //つらい記憶の分類(上から絶望、不安感)
                case MemoryType.NegativeMemory.Despair:
                case MemoryType.NegativeMemory.Uneasiness:
                    //記憶転換が使用可能になるためのカウントが1増える
                    if (!playerSpecialityController.CanSP[(int)Attach.Speciality.Convert])
                        playerSpecialityController.NegativeMemoryCount[(int)Attach.Speciality.Convert] += plusMemoryCount;
                    break;
                //特殊な記憶(上からトラウマ黒歴史)
                case MemoryType.NegativeMemory.Trauma:
                case MemoryType.NegativeMemory.Darkhistory:
                    //すべてのの特技を使用可能になるカウントが1増える。
                    if (!playerSpecialityController.CanSP[(int)Attach.Speciality.Stagnation] ||
                        !playerSpecialityController.CanSP[(int)Attach.Speciality.Refusal] ||
                        !playerSpecialityController.CanSP[(int)Attach.Speciality.Convert])
                    {
                        int length = Enum.GetValues(typeof(MemoryType.MemoryClassification)).Length - 1;
                        for (int i = 0; i < length; i++)
                        {
                            playerSpecialityController.NegativeMemoryCount[i] += plusMemoryCount;
                        }
                    }
                    break;
                default:
                    Debug.LogError("想定外のものが選択されたエラーです。");
                    break;
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject.CompareTag("Player"))
        {
            //ライフビルドに付属された経験値をプレイヤーに
            Player player = other.transform.root.gameObject.GetComponent<Player>();
            PlayerSpecialityController playerSpecialityController = other.transform.root.gameObject.GetComponent<PlayerSpecialityController>();
            if (player != null)
            {
                player.SetExp(exp);
                //ライフビルドに付与された負の記憶の分類に応じてステータスが変化する。
                LifebuildMemory(player, playerSpecialityController);
            }
            Destroy(gameObject);
        }
    }    
}