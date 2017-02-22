﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Items
{
    public string name;
    public double msrp;
    public double salePrice;
}

[System.Serializable]
public class Walmart
{
    public List<Items> items;
}