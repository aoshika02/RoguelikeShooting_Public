using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SEClips", menuName = "ScriptableObjects/SEClips", order = 1)]
public class SEClips : ScriptableObject 
{
    public List<SEClip> SEClipList = new List<SEClip>();
}

[Serializable]
public class SEClip
{
    public SEType SEType;
    public AudioClip Clip;
}
