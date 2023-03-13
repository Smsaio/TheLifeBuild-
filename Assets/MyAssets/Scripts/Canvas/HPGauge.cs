using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Zenject;
using UniRx;
using PlayerSpace;

/// <summary>
/// 体力ゲージ管理、制御クラス
/// </summary>
public class HPGauge : MonoBehaviour,IHPGauge
{
    //色を変化させるオブジェクトと色
    [SerializeField] private Image hpBarFill;
    [SerializeField] private Color color_1, color_2, color_3, color_4;
    [SerializeField] private Image backGauge;
    //後ろの体力ゲージ
    [SerializeField] private Slider backHPSlider;
    //点滅するかどうか
    [SerializeField] private bool isFlash = false;
    [SerializeField] private Slider hpSlider;
    //dotween(後ろの体力のアニメーション)
    private Tween backGaugeTween;
    protected float valueFrom = 1;
    protected float valueTo = 1;
    private float alpha_Sin = 0;
    //変化するスピード
    private float sinSpeed = 4;   
    void Start()
    {
        
    }
    protected virtual void Update()
    {
        //危険なら点滅させる
        if (valueTo < 0.2f && isFlash)
        {
            backGauge.color = Color.black;
            alpha_Sin = Mathf.Sin(Time.time * sinSpeed) / 2 + 0.5f;
            ColorCoroutine();
        }
    }
    /// <summary>
    /// 体力バーの設定
    /// </summary>
    /// <param name="reducationValue">ダメージ値</param>
    /// <param name="delayTime">背中で減る体力バーの遅延時間</param>
    public virtual void GaugeReduction(float reducationValue,int currentHP,int currentMaxHP,float delayTime = 0.5f)
    {
        //現在の体力
        valueFrom = (float)currentHP / currentMaxHP;
        //減った後の体力
        valueTo = (currentHP - reducationValue) / currentMaxHP;
        //どの部分も0.25fが最大なので、4倍にする
        float mul = 4.0f;
        //体力があった場合
        if (0 <= currentHP && Time.time > 0.01f)
        {
            //1から始まってある程度で下がる
            //色変化
            if (valueFrom > 0.75f)
            {
                hpBarFill.color = Color.Lerp(color_2, color_1, (valueFrom - 0.75f) * mul);
            }
            else if (valueFrom > 0.25f)
            {
                hpBarFill.color = Color.Lerp(color_3, color_2, (valueFrom - 0.25f) * mul);
            }
            else
            {
                hpBarFill.color = Color.Lerp(color_4, color_3, valueFrom * mul);
            }
            hpSlider.value = valueTo;
        }
        if (backGaugeTween != null)
        {
            backGaugeTween.Kill();
        }

        // 赤ゲージ減少
        backGaugeTween = DOTween.To(
            () => valueFrom,
            x => {
                backHPSlider.value = x;
            },
            valueTo,
            delayTime
        );
    }
    //体力バーを点滅
    void ColorCoroutine()
    {
        Color _color = hpBarFill.color;

        _color.a = alpha_Sin;

        hpBarFill.color = _color;
    }
}