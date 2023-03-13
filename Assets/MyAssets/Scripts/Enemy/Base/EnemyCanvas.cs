using EnemySpace;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyCanvas : MonoBehaviour
{
    [SerializeField] private EnemyBase enemyBase;
    //負の感情の記憶テキスト
    [Header("負の記憶表示用とレベルのテキスト")]
    [SerializeField] private TextMeshProUGUI memoryText;
    //レベルテキスト
    [SerializeField] private TextMeshProUGUI levelText;
    //敵か味方か
    private bool isEnemy = true;
    // Start is called before the first frame update
    void Start()
    {
        if(enemyBase == null)
        {
            enemyBase = transform.root.GetComponent<EnemyBase>();
        }
        SetNegativeMemory(enemyBase.Memory);
        LevelText(enemyBase.CurrentLevel);
    }

    // Update is called once per frame
    void Update()
    {
        if(enemyBase != null)
        {
            if (enemyBase.OnFellow && isEnemy)
            {
                isEnemy = false;
                SetGoodMemory((MemoryType.GoodMemory)enemyBase.Memory);
            }
        }
    }
    private void LevelText(int level)
    {
        levelText.text = "Lv." + level.ToString();
    }
    public void SetNegativeMemory(MemoryType.NegativeMemory memoryType)
    {
        //記憶の種類に応じてテキストの色を変える。
        //テキストを司っている記憶に変換
        switch (memoryType)
        {
            case MemoryType.NegativeMemory.Hatred:
                memoryText.text = "憎悪";
                memoryText.color = Color.red;
                break;
            case MemoryType.NegativeMemory.Angry:
                memoryText.text = "怒り";
                memoryText.color = Color.red;
                break;
            case MemoryType.NegativeMemory.Tragic:
                memoryText.text = "悲壮感";
                memoryText.color = Color.blue;
                break;
            case MemoryType.NegativeMemory.Sorrowful:
                memoryText.text = "悲哀";
                memoryText.color = Color.blue;
                break;
            case MemoryType.NegativeMemory.Despair:
                memoryText.text = "絶望";
                memoryText.color = Color.yellow;
                break;
            case MemoryType.NegativeMemory.Uneasiness:
                memoryText.text = "不安感";
                memoryText.color = Color.yellow;
                break;
            case MemoryType.NegativeMemory.Trauma:
                memoryText.text = "トラウマ";
                memoryText.color = Color.white;
                break;
            case MemoryType.NegativeMemory.Darkhistory:
                memoryText.text = "黒歴史";
                memoryText.color = Color.black;
                break;
            default:
                Debug.Log("負の記憶が設定されていません。");
                break;
        }
    }
    /// <summary>
    /// いい記憶に変換
    /// </summary>
    private void SetGoodMemory(MemoryType.GoodMemory goodMemory)
    {
        //味方になった時に負の記憶に合わせた変化が起きる
        switch (goodMemory)
        {
            case MemoryType.GoodMemory.Affection:
                memoryText.text = "情愛";
                break;
            case MemoryType.GoodMemory.Laugh:
                memoryText.text = "笑う";
                break;
            case MemoryType.GoodMemory.Satisfaction:
                memoryText.text = "満足";
                break;
            case MemoryType.GoodMemory.Delight:
                memoryText.text = "歓喜";
                break;
            case MemoryType.GoodMemory.Hope:
                memoryText.text = "希望";
                break;
            case MemoryType.GoodMemory.Peaceofmind:
                memoryText.text = "安心";
                break;
            default:
                Debug.Log("いい記憶に設定されている値は想定されていません。");
                break;
        }
    }
}
