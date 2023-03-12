using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EnemySpace;
/// <summary>
/// キャンバスの回転補正(見えないなら無駄なので消す。)
/// </summary>
public class RotCorrection : MonoBehaviour
{
    //敵が司っている感情のテキスト
    [SerializeField] private GameObject enemyCanvas;
    private Rect rect = new Rect(0, 0, 1, 1);
    private void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        TextLook();
    }
    /// <summary>
    /// キャンバスをカメラと同じ角度に(常にカメラの正面に)
    /// </summary>
    private void TextLook()
    {
        var viewportPos = Camera.main.WorldToViewportPoint(enemyCanvas.transform.position);
        //角度補正(画面から見えなくなったら非表示にする)
        if (rect.Contains(viewportPos))
        {
            enemyCanvas.SetActive(true);
            enemyCanvas.transform.rotation = Quaternion.Euler(new Vector3(0, Camera.main.transform.eulerAngles.y, 0));
        }
        else
        {
            enemyCanvas.SetActive(false);
        }
    }
}
