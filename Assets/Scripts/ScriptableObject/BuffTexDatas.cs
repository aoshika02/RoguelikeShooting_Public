using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BuffTexDatas", menuName = "ScriptableObject/BuffTexDatas", order = 0)]
public class BuffTexDatas : ScriptableObject
{
   public List<BuffToTex> BuffToTexes = new List<BuffToTex>();
}
[Serializable]
public class BuffToTex 
{
    public BuffType BuffType;
    public Sprite BuffSprite;
}
