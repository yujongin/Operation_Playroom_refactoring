using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [SerializeField]
    public class UnitData
    {
        public int DataId;
        public string DescriptionTextId;
        public string PrefabLabel;
        public float HP;
        public float Atk;
    }

    [SerializeField]
    public class UnitDataLoader : ILoader<int, UnitData>
    {
        public List<UnitData> units = new List<UnitData>();
        public Dictionary<int, UnitData> MakeDict()
        {
            Dictionary<int, UnitData> dict = new Dictionary<int, UnitData>();
            foreach(UnitData unit in units)
            {
                dict.Add(unit.DataId, unit);
            }
            return dict;
        }
    }
}
