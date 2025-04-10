using System.Collections.Generic;
using App.Interactable;
using UnityEngine;

namespace App.Core.POI
{
    public class PointOfInterestSystem : SystemBase<PointOfInterest>
    {
        [SerializeField] private Transform observer;
        public ISet<PointOfInterest> PointsOfInterest => Components;

        public Transform Observer => observer;

        public Vector3 GetRelativeWorldPosition(PointOfInterest poi) =>
            poi.transform.position - observer.transform.position;

        public Vector3 GetDirection(PointOfInterest poi) => GetDelta(poi).normalized;

        public Vector3 GetPosition(PointOfInterest poi) => poi.transform.position;

        public float GetDistance(PointOfInterest poi) =>
            Vector3.Distance(poi.transform.position, observer.transform.position);

        protected override void OnRegistered(PointOfInterest component)
        {
        }

        protected override void OnUnregistered(PointOfInterest component)
        {
        }

        public Vector3 GetDelta(PointOfInterest poi) => poi.transform.position - observer.transform.position;
    }
}