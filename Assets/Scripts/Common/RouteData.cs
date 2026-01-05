using System.Collections.Generic;
using UnityEngine;

public class RouteData : MonoBehaviour
{
    [SerializeField] private List<Transform> _routePoints = new List<Transform>();
    public List<Transform> GetRoutePoints()
    {
        return _routePoints;
    }
}
