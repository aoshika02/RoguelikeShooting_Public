using System.Collections.Generic;
using UnityEngine;

public static class RandomTypeBase
{
    public static List<T> ShuffleList<T>(List<T> list)
    {
        List<T> returnlist = new List<T>(list);
        for (int i = returnlist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (returnlist[i], returnlist[randomIndex]) = (returnlist[randomIndex], returnlist[i]);
        }
        return returnlist;
    }
    public static List<T> MakeListByCount<T>(List<T> list, int repeatCount = 1)
    {
        List<T> returnlist = new List<T>();
        foreach (T item in list)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                returnlist.Add(item);
            }
        }
        return returnlist;
    }
    public static void FillQueueFromList<T>(Queue<T> queue, List<T> list)
    {
        foreach (T item in list)
        {
            queue.Enqueue(item);
        }
    }
    public static T GetRandomType<T>(Queue<T> targetType, List<T> list, int repeatCount = 1)
    {
        if (targetType.Count <= 0)
        {
            FillQueueFromList(targetType, ShuffleList(MakeListByCount(list, repeatCount)));
        }
        return targetType.Dequeue();
    }
    public static List<T> GetRandomType<T>(int count, Queue<T> targetType, bool isMix, List<T> list, int repeatCount = 1)
    {
        if (targetType.Count <= 0)
        {
            FillQueueFromList(targetType, ShuffleList(MakeListByCount(list, repeatCount)));
        }
        List<T> returnlist = new List<T>();
        if (isMix)
        {
            for (int i = 0; i < count; i++)
            {
                if (targetType.Count <= 0)
                {
                    FillQueueFromList(targetType, ShuffleList(MakeListByCount(list, repeatCount)));
                }
                returnlist.Add(targetType.Dequeue());
            }
        }
        else
        {
            T type = targetType.Dequeue();
            for (int i = 0; i < count; i++)
            {
                returnlist.Add(type);
            }
        }
        return returnlist;
    }
}

