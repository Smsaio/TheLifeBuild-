using PlayerSpace;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;
using UniRx;

public class TextScaleChange : ReactivePropertyController
{
    //レベルアップという文字を表示するだけのテキスト
    [Header("レベルアップ表示"), SerializeField] private TextMeshProUGUI levelUPText;
    //拡大される倍数
    [Header("レベルアップテキストの拡大率"), Range(1.25f, 3.0f), SerializeField] private float levelTextMulSize = 3.0f;
    [Header("レベルアップテキストが小さくなる速度"), Range(0.25f, 2.0f), SerializeField] private float subSpeed = 0.5f;
    //スケール変更
    private bool isSizeChange = false;
    //サイズ変更の時間
    private float sizeChangeTime = 0.0f;
    //サイズ変更インターバル
    private float sizeChangeTimeInterval = 0.01f;
    //前回のサイズ
    private float beforeSize = 0.0f;
    private Player currentPlayer;
    private int playerLevel;
    
    // Start is called before the first frame update
    void Start()
    {
        //前のサイズを格納
        beforeSize = levelUPText.fontSize;
        //ReactivePlayer(role);
    }

    // Update is called once per frame
    void Update()
    {
        ScaleChange(currentPlayer.IsLevelUP);
    }
    public override void ReactivePlayer(IRole role)
    {
        base.ReactivePlayer(role);
        if (role == null) return;
        role.CurrentPlayer.Subscribe(player => { currentPlayer = player; }).AddTo(this);
        role.CurrentPlayerLevel.Subscribe(level => 
        {
            playerLevel = level;
            SetSize();
        }).AddTo(this);
    }
    /// <summary>
    /// レベルアップ時、レベルアップのテキストを設定した最大のサイズに設定する。
    /// </summary>
    public void SetSize()
    {
        //一旦サイズを最大にする。
        levelUPText.fontSize = beforeSize * levelTextMulSize;
        //サイズ変更中
        isSizeChange = true;
        sizeChangeTime = 0.0f;
    }
    /// <summary>
    /// レベルアップしたときの表示テキストサイズ変更
    /// </summary>
    private void ScaleChange(bool isLevelUP)
    {
        levelUPText.gameObject.SetActive(isLevelUP);
        if (isLevelUP)
        {
            //サイズ変更中
            if (isSizeChange)
            {
                //元のサイズと違っていたら
                if (levelUPText.fontSize > beforeSize)
                {
                    sizeChangeTime += Time.deltaTime;
                    //サイズが大きくなる間隔に達したら
                    if (sizeChangeTime > sizeChangeTimeInterval)
                    {
                        //前のサイズ以下になってたら前のサイズになる
                        if (levelUPText.fontSize <= beforeSize)
                        {
                            levelUPText.fontSize = beforeSize;
                            isSizeChange = false;
                        }
                        else
                        {
                            //小さくする
                            levelUPText.fontSize -= subSpeed;
                            sizeChangeTime = 0.0f;
                        }
                    }
                }
            }
        }
    }
}
