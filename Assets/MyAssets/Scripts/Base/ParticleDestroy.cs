using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パーティクルを破壊する
/// </summary>
public class ParticleDestroy : MonoBehaviour
{
    //出現エフェクト
    [Header("出現エフェクト"), SerializeField] private ParticleSystem[] particles;
    //パーティクルが何個なくなったか
    private int destroyCount = 0;
    private void Update()
    {
        ParticlesDestroy();
    }
    /// <summary>
    /// エフェクト破壊
    /// </summary>
    public void ParticlesDestroy()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].isStopped)
            {
                if (destroyCount == particles.Length)
                {
                    Destroy(gameObject);
                }
                else
                {
                    ++destroyCount;
                }
            }
        }
    }
}
