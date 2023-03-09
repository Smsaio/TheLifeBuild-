using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System;

/// <summary>
/// 発見時のマーク表示
/// </summary>
public class FindMark : MonoBehaviour
{
    //発見時に出るマーク
    [SerializeField] private TextMeshProUGUI mark;
    [Range(1.25f,2.5f),SerializeField] private float textMulSize = 1.25f;
    //サイズを変えたか
    private bool isSizeChange;
    public bool IsSizeChange { get { return isSizeChange; } set { isSizeChange = value; } }
    //どのくらいの間隔で小さくなるか
    private float sizeInterval = 0.05f;
    private float beforeSize = 0;
    //サイズを変える間隔
    private float sizeChangeTimeInterval = 0.1f;
    private float sizeChangeTime = 0.0f;
    //最高サイズ
    private float maxSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        beforeSize = mark.fontSize;
        maxSize = beforeSize * textMulSize;
        mark.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        TextScaleChange();
    }
    /// <summary>
    /// レベルアップ時、レベルアップのテキストを設定した最大のサイズに設定する。
    /// </summary>
    public void SetSize()
    {
        //一旦サイズを最大にする。
        mark.fontSize = maxSize;
        //サイズ変更中
        isSizeChange = true;
        sizeChangeTime = 0.0f;     
    }
    /// <summary>
    /// レベルアップしたときの表示テキストサイズ変更
    /// </summary>
    private void TextScaleChange()
    {
        //サイズ変更中
        if (isSizeChange)
        {
            sizeChangeTime += Time.deltaTime;
            //サイズが大きくなる間隔に達したら
            if (sizeChangeTime > sizeChangeTimeInterval)
            {
                //前のサイズ以下になってたら前のサイズになる
                if (mark.fontSize <= beforeSize)
                {
                    mark.fontSize = beforeSize;
                    isSizeChange = false;
                    mark.gameObject.SetActive(false);
                }
                else
                {
                    //大きくする
                    mark.fontSize -= sizeInterval;
                    sizeChangeTime = 0.0f;
                    mark.gameObject.SetActive(true);
                }
            }
        }
    }
}
