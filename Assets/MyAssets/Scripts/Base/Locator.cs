using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �C���^�[�t�F�C�X�ƍ��킹��ƂȂ��ǂ�
/// </summary>
/// <typeparam name="T"></typeparam>
public static class Locator<T> where T : class
{
    public static T Instance { get; private set; }
    public static bool IsValid() => Instance != null;

    public static void Bind(T instance)
    {
        Instance = instance;
    }

    public static void UnBind(T instance)
    {
        if(Instance == instance)
        {
            Instance = null;
        }
    }
    public static void Clear()
    {
        Instance = null;
    }
}
