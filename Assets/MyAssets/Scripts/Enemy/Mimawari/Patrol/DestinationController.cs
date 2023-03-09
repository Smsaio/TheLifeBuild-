using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パトロールする際の設定された目的へ進む用のクラス
/// </summary>
public class DestinationController : MonoBehaviour
{

    public enum Route { inOrder, random }
    //目的地
    [SerializeField] private Transform[] targets;
    //目的地の順番
    [SerializeField] private int order = 0;
    //順番に目的地に行くか設定された目的地の中からランダムに移動するか
    [SerializeField] private Route route;
    //目的地
    private Vector3 destination;


    void Start()
    {
        CreateDestination();
    }
    /// <summary>
    /// 目的地設定
    /// </summary>
    public void CreateDestination()
    {
        //順番かランダムに移動するか
        if (route == Route.inOrder)
        {
            CreateInOrderDestination();
        }
        else if (route == Route.random)
        {
            CreateRandomDestination();
        }
    }

    /// <summary>
    /// 順番に目的地を設定
    /// </summary>
    private void CreateInOrderDestination()
    {
        //周回、最大であれば0から
        if (order < targets.Length - 1)
        {
            //順番に目的地へ進む
            SetDestination(new Vector3(targets[order].transform.position.x, transform.position.y, targets[order].transform.position.z));
            ++order;
        }
        else
        {
            //最終地点に着いた場合はマイナスされていく
            SetDestination(new Vector3(targets[order].transform.position.x, transform.position.y, targets[order].transform.position.z));
            --order;
        }
    }

    /// <summary>
    /// 目的地をランダムに設定
    /// </summary>
    private void CreateRandomDestination()
    {
        int num = UnityEngine.Random.Range(0, targets.Length);
        SetDestination(new Vector3(targets[num].transform.position.x, transform.position.y, targets[num].transform.position.z));
    }

    /// <summary>
    /// ターゲットを索敵したときの目的地の設定
    /// </summary>
    /// <param name="position">目的地</param>
    public void SetDestination(Vector3 position)
    {
        destination = position;
    }

    /// <summary>
    /// 目的地の取得
    /// </summary>
    /// <returns></returns>
    public Vector3 GetDestination()
    {
        return destination;
    }
}
