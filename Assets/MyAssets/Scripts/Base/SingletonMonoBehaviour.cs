using UnityEngine;
using System;

/// <summary>
/// Singletonは: SingletonMonoBehaviour<GameManager>(GameManagerの場合)のように使う
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    //このクラスを継承したクラスのインスタンスを設定して自分以外の自分のクラスを探し、あった場合は自分を消す。
    private static T instance;
    public static T Instance
    {
        get
        {
            //インスタンス生成されていなかった場合
            if (instance == null)
            {
                Type t = typeof(T);

                instance = (T)FindObjectOfType(t);
                if (instance == null)
                {
                    Debug.LogError(t + " をアタッチしているGameObjectはありません");
                }
            }

            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    protected virtual void Awake()
    {
        // 他のゲームオブジェクトにアタッチされているか調べる
        // アタッチされている場合は破棄する。
        CheckInstance();
    }

    protected virtual void OnDestroy()
    {
        if(Instance == instance)
        {
            Instance = null;
        }
    }
    /// <summary>
    /// インスタンスの有無審査
    /// </summary>
    /// <returns></returns>
    protected bool CheckInstance()
    {
        if (instance == null)
        {
            instance = this as T;
            return true;
        }
        else if (Instance == this)
        {
            return true;
        }
        Destroy(this);
        return false;
    }
}