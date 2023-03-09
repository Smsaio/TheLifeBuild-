using EnemySpace;
using UnityEngine;
using System.Collections;

/// <summary>
/// 敵の目的地設定
/// </summary>
public class SetPosition : MonoBehaviour
{
	//初期位置
	private Vector3 startPosition;
	//目的地
	private Vector3 destination;
	private EnemyBase enemyBase;
    void Start()
	{
		//　初期位置を設定
		startPosition = transform.position;
		SetDestination(startPosition);
		enemyBase = GetComponent<EnemyBase>();
	}
	void Update()
    {
		enabled = enemyBase.enabled;
    }
	/// <summary>
	/// ランダムに目的地作成
	/// </summary>
	public void CreateRandomPosition()
	{
		//　ランダムなVector2の値を得る
		var randDestination = Random.insideUnitCircle * 10;
		//　現在地にランダムな位置を足して目的地とする
		SetDestination(startPosition + new Vector3(randDestination.x, 0, randDestination.y));
	}

	/// <summary>
	/// 目的地を設定する
	/// </summary>
	/// <param name="position">目的地</param>
	public void SetDestination(Vector3 position)
	{
		destination = position;
	}

	/// <summary>
	/// 目的地を取得する
	/// </summary>
	/// <returns></returns>
	public Vector3 GetDestination()
	{
		return destination;
	}
}
