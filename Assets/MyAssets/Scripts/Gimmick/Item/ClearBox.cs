using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnemySpace;
using System;
using GameManagerSpace;
using Zenject;

/// <summary>
/// 扉にセットするクリアする用のクラス
/// </summary>
public class ClearBox : MonoBehaviour
{
    IGameManager gameManager = default;
    private BoxCollider boxCollider;

    [Inject]
    public void Construct(IGameManager IgameManager)
    {
        gameManager = IgameManager;
    }
    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.CurrentGameMode == GameMode.BossDefeat)
        {
            boxCollider.isTrigger = true;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject.CompareTag("Player"))
        {
            if (gameManager.CurrentGameMode == GameMode.BossDefeat)
            {
                gameManager.SetGameMode(GameMode.GameClear);
            }
        }
    }
}
