using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BGMClips", menuName = "ScriptableObjects/BGMClips", order = 2)]
public class BGMClips : ScriptableObject
{
    public List<BGMClip> BGMClipList = new List<BGMClip>();
}

[Serializable]
public class BGMClip
{
    public BGMType BGMType;
    public AudioClip Clip;
}