using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHPGauge
{
    public void GaugeReduction(float reducationValue, int currentHP, int currentMaxHP, float delayTime = 0.5f);
}