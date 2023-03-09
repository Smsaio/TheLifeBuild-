using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombObject : MonoBehaviour
{
    //爆発オブジェクト
    [Header("爆発エフェクト")]
    [SerializeField] private GameObject explosionObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Enemy"))
        {
            var obj = Instantiate(explosionObject, transform.position, Quaternion.identity);
            Destroy(obj, 0.2f);
            Destroy(gameObject);
        }
    }
}
