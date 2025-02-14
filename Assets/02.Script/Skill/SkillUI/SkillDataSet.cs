using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SkillDataSet : IListViewItem
{
    public List<SkillData> dataSet;
    public SkillDataSet(List<SkillData> dataSet)
    {
        this.dataSet = dataSet;
    }
}
