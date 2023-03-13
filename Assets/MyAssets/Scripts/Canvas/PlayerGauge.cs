using PlayerSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;

public class PlayerGauge : HPGauge
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    protected override void Update()
    {
        base.Update();
    }
    public override void GaugeReduction(float reducationValue, int currentHP, int currentMaxHP, float delayTime = 0.5f)
    {
        base.GaugeReduction(reducationValue, currentHP, currentMaxHP, delayTime);
    }
}
