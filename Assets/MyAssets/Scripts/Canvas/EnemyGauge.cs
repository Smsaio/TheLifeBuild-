using EnemySpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGauge : HPGauge
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
    public override void GaugeReduction(float reducationValue, int currentHP, int currentMaxHP, float delayTime = 0.5F)
    {
        base.GaugeReduction(reducationValue, currentHP, currentMaxHP, delayTime);
    }
}
