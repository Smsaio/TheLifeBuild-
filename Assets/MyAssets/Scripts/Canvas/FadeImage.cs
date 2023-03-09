using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeImage : MonoBehaviour
{
    [Header("最初からフェードインが完了しているかどうか")] public bool firstFadeInComp;
    //画像
    private Image image = null;
    //フレームの待ち時間
    private int frameCount = 0;
    //透明になっていく時間
    private float timer = 0.0f;
    private float alphaTime = 1.0f;
    //フェードイン・アウト中か
    private bool fadeIn = false;
    private bool fadeOut = false;
    //フェードイン・アウトが完了したか
    private bool compFadeIn = false;
    private bool compFadeOut = false;

    

    /// <summary>
    /// フェードインが完了したかどうか
    /// </summary>
    /// <returns></returns>
    public bool IsFadeInComplete()
    {
        return compFadeIn;
    }


    /// <summary>
    /// フェードアウトを完了したかどうか
    /// </summary>
    /// <returns></returns>
    public bool IsFadeOutComplete()
    {
        return compFadeOut;
    }

    void Start()
    {
        image = GetComponent<Image>();
        if (firstFadeInComp)
        {
            FadeInComplete();
        }
        else
        {
            StartFadeIn();
        }
    }

    void Update()
    {
        //シーン移行時の処理の重さでTime.deltaTimeが大きくなってしまうから2フレーム待つ
        if (frameCount > 2)
        {
            if (fadeIn)
            {
                FadeInUpdate();
            }
            else if (fadeOut)
            {
                FadeOutUpdate();
            }
        }
        ++frameCount;
    }

    //フェードイン中
    private void FadeInUpdate()
    {
        if (timer < alphaTime)
        {
            image.color = new Color(1, 1, 1, 1 - timer);
            image.fillAmount = 1 - timer;
        }
        else
        {
            FadeInComplete();
        }
        timer += Time.deltaTime;
    }

    //フェードアウト中
    private void FadeOutUpdate()
    {
        if (timer < alphaTime)
        {
            image.color = new Color(1, 1, 1, timer);
            image.fillAmount = timer;
        }
        else
        {
            FadeOutComplete();
        }
        timer += Time.deltaTime;
    }

    //フェードイン完了
    private void FadeInComplete()
    {
        image.color = new Color(1, 1, 1, 0);
        image.fillAmount = 0;
        image.raycastTarget = false;
        timer = 0.0f;
        fadeIn = false;
        compFadeIn = true;
    }

    //フェードアウト完了
    private void FadeOutComplete()
    {
        image.color = new Color(1, 1, 1, 1);
        image.fillAmount = 1;
        image.raycastTarget = false;
        timer = 0.0f;
        fadeOut = false;
        compFadeOut = true;
    }
    /// <summary>
    /// フェードインを開始する
    /// </summary>
    private void StartFadeIn()
    {
        if (fadeIn || fadeOut)
        {
            return;
        }
        fadeIn = true;
        compFadeIn = false;
        timer = 0.0f;
        image.color = new Color(1, 1, 1, 1);
        image.fillAmount = 1;
        image.raycastTarget = true;
    }

    /// <summary>
    /// フェードアウトを開始する
    /// </summary>
    public void StartFadeOut()
    {
        if (fadeIn || fadeOut)
        {
            return;
        }
        fadeOut = true;
        compFadeOut = false;
        timer = 0.0f;
        image.color = new Color(1, 1, 1, 0);
        image.fillAmount = 0;
        image.raycastTarget = true;
    }
}