using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    public enum FadeType
    {
        In,Out
    }
    //扱うレンダラー
    [Header("扱うレンダラー"),SerializeField] private Renderer[] renderers = { };
    //小さければ早く、大きければ遅くなる
    [Header("小さければ早く、大きければ遅くなる"),SerializeField, Min(0)] private float effectDuration = 1f;
    //Easeの型変更
    [Header("Ease変更"),SerializeField] private Ease effectEase = Ease.Linear;
    //変更するパラメータ
    [Header("変更するマテリアルの変数"),SerializeField] private string progressParamName = "_Progress";
    public string ProgressParamName { get { return progressParamName; } }
    [SerializeField] private FadeType fadeType = FadeType.Out;
    List<Material> materials = new();
    Sequence sequence;

    void Start()
    {
        GetMaterials();
        //フェードアウトかイン
        if (fadeType == FadeType.Out)
        {
            DissolveOut();
        }
        else
        {
            DissolveIn();
        }
    }
    private void Update()
    {
        
    }
    public void DissolveIn()
    {
        sequence = DOTween.Sequence().SetLink(gameObject).SetEase(effectEase);

        foreach (Material m in materials)
        {
            m.SetFloat(progressParamName, 0);
            sequence.Join(m.DOFloat(1, progressParamName, effectDuration));
        }

        sequence.Play();
    }

    public void DissolveOut()
    {
        sequence = DOTween.Sequence().SetLink(gameObject).SetEase(effectEase);

        foreach (Material m in materials)
        {
            m.SetFloat(progressParamName, 1);
            sequence.Join(m.DOFloat(0, progressParamName, effectDuration));
        }

        sequence.Play();
    }

    void GetMaterials()
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                materials.Add(m);
            }
        }
    }
}
