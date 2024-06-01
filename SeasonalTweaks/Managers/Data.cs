using System;
using System.Collections.Generic;

namespace SeasonalTweaks.Managers;

[Serializable]
public class Configurations
{
    public List<PickableData> Pickable = new();
    public List<PlantData> Plants = new();
    public List<BeeHiveData> Beehives = new();
    public List<PieceData> Pieces = new();
    public List<ItemData> Items = new();

}

[Serializable]
public class PickableData
{
    public string m_prefabName = null!;
    public HarvestData m_spring = new();
    public HarvestData m_summer = new();
    public HarvestData m_fall = new();
    public HarvestData m_winter = new();
}

[Serializable]
public class HarvestData
{
    public int m_amount = 0;
    public bool m_canHarvest = true;
}

[Serializable]
public class PlantData
{
    public string m_prefabName = null!;
    public PlantValues m_spring = new();
    public PlantValues m_summer = new();
    public PlantValues m_fall = new();
    public PlantValues m_winter = new();
}

[Serializable]
public class PlantValues
{
    public float m_maxScale = 0f;
    public float m_minScale = 0f;
    public float m_growTimeMax = 0f;
    public float m_growTime = 0f;
    public bool m_canHarvest = true;
}

[Serializable]
public class BeeHiveData
{
    public string m_prefabName = null!;
    public BeeData m_spring = new();
    public BeeData m_summer = new();
    public BeeData m_fall = new();
    public BeeData m_winter = new();
}

[Serializable]
public class BeeData
{
    public int m_maxHoney = 0;
    public bool m_canHarvest = true;
}

[Serializable]
public class PieceData
{
    public string m_prefabName = null!;
    public SeasonKeys.Season m_season = SeasonKeys.Season.None;
}

[Serializable]
public class ItemData
{
    public string m_prefabName = null!;
    public int m_price = 1000;
    public int m_stack = 1;
    public SeasonKeys.Season m_season = SeasonKeys.Season.None;
}