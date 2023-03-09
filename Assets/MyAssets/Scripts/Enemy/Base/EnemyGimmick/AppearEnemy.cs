using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using EnemySpace;
using System;
using System.Collections.Generic;
using Zenject;
using TMPro;

public class AppearEnemy : MonoBehaviour
{
	//　出現させる敵を入れておく
	[SerializeField] private GameObject[] enemys;
	//出現の場所、エフェクト、音
	[Header("出現する間隔"), SerializeField] private float appearNextTime;
	[Header("出現した時のエフェクト"), SerializeField] private GameObject appearEffect;
	[Header("出現した時の音"), SerializeField] private AudioClip appearSound;
	//単体の種類の敵のみ出現する
	[Header("一種類のみ出す場合"),SerializeField] private bool isSimple = false;
	//要素番号で単体の敵の種類をenemysに入れられた敵から選出
	[Header("敵配列の要素番号"),SerializeField] private int simpleEnemyType;
	//スポーンする場所
	[SerializeField] private Transform[] spawnPoint;
	//地面のレイヤー
	[SerializeField] private LayerMask groundLayer;
	//敵を出す最大数
	[Range(1,10),SerializeField]private int maxEnemyCount = 3;
	//今出している敵のリスト
	private List<EnemyBase> enemyList = new List<EnemyBase>();	
	//　今何人の敵を出現させたか（総数）
	private int enemyCount = 0;
	//プレイヤーが入ってきた
	private bool inPlayer = false;
	//レイの最高飛距離
	private readonly int maxDis = 1000;
	IAudioSourceManager audioSourceManager = default;
	IRole role = default;

	[Inject]
	public void Construct(IAudioSourceManager IaudioSourceManager,IRole Irole)
	{
		audioSourceManager = IaudioSourceManager;
		role = Irole;
	}
	// Use this for initialization
	void Start()
	{
		InitializeSpawn();
		StartCoroutine(Appear());
	}
	/// <summary>
	/// 敵は召喚しているので表示するだけでいい
	/// </summary>
	/// <returns></returns>
	IEnumerator Appear()
    {
		yield return new WaitWhile(() => !inPlayer);
		while (enemyCount < maxEnemyCount)
		{
			AppearEnemyLoop();
			yield return new WaitForSeconds(appearNextTime);
		}
	}
	/// <summary>
	/// 敵出現メソッド
	/// </summary>
	private void AppearEnemyLoop()
	{
		if (enemyCount < maxEnemyCount)
		{
			var obj = Instantiate(appearEffect, enemyList[enemyCount].gameObject.transform.position, Quaternion.identity);
			audioSourceManager.PlaySE(appearSound);
			Destroy(obj,0.4f);
			enemyList[enemyCount].gameObject.SetActive(true);
			++enemyCount;
		}
	}
	/// <summary>
	/// 敵の召喚をしておく
	/// </summary>
	void InitializeSpawn()
    {
		for (int i = 0; i < maxEnemyCount; i++)
		{
			//ランダムな位置
			var randomPos = UnityEngine.Random.Range(0, spawnPoint.Length - 1);
			//　敵の向きをランダムに決定
			var randomRotationY = UnityEngine.Random.value * 360f;
			Ray ray = new(spawnPoint[randomPos].position, -spawnPoint[randomPos].transform.up);
			float spawnPosY = 0.0f;
			RaycastHit hit;
			//下にレイを発射して確認
			if (Physics.Raycast(ray, out hit, maxDis, groundLayer))
			{
				spawnPosY = hit.point.y;
			}
			//出現位置設定
			var pos = new Vector3(spawnPoint[randomPos].position.x, spawnPosY, spawnPoint[randomPos].position.z);
			if (isSimple)
            {
				//単体の種類のみ出現
				EnemySpawn(simpleEnemyType,pos,randomRotationY);
			}
            else
            {
				//出現させる敵をランダムに選ぶ
				var randomIndex = UnityEngine.Random.Range(0, enemys.Length - 1);
				EnemySpawn(randomIndex, pos, randomRotationY);
			}			
		}
	}
	//敵の出現
	private void EnemySpawn(int index,Vector3 pos,float rotY)
    {
		//出現
		var enemy = Instantiate(enemys[index], pos, Quaternion.Euler(0f, rotY, 0f));
		var enemyBase = enemy.GetComponent<EnemyBase>();
		var followChara = enemy.GetComponent<FollowChara>();
		enemyBase.AudioSourceManager = audioSourceManager;
		if (enemyBase != null)
		{
			enemyBase.ReactivePlayer(role);
		}
		if (followChara != null)
		{
			followChara.ReactivePlayer(role);
		}
		enemy.gameObject.SetActive(false);
		enemyList.Add(enemyBase);
	}
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
			inPlayer = true;
        }
    }
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			inPlayer = false;
		}
	}
}
