using System.Collections.Generic;

[System.Serializable]
public class RegionData
{
    public string regionName;
    public List<int> countryIndexes;

    public RegionData(string name, List<int> indexes)
    {
        regionName = name;
        countryIndexes = indexes;
    }
}