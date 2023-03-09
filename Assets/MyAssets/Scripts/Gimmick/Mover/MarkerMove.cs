using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 銃士の必殺技用のマーカーの上下移動
/// </summary>
public class MarkerMove : MonoBehaviour
{
    [Range(1.0f,5.0f),SerializeField]private float sinTime = 2.0f;
    private float plusTall = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    private void Move()
    {
        //周期的に上下する(1秒あたりの周波数)
        float f = 1.0f / sinTime;
        //sin波を使い、上下に移動
        float ysin = Mathf.Sin(2 * Mathf.PI * f * Time.time);
        var pos = transform.localPosition;
        transform.localPosition = new Vector3(pos.x, plusTall + ysin, pos.z);
    }
}
