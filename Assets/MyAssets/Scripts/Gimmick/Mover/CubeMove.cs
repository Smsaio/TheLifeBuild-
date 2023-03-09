using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 障害物(横移動する)
/// </summary>
public class CubeMove : MonoBehaviour
{
    private Vector3 basePos;
    [SerializeField] private float moveSpeed = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        basePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = basePos + new Vector3(Mathf.Sin(Time.time) * moveSpeed, 0f, 0f);
    }
}