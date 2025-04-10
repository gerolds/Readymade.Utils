using System.Collections.Generic;
using App.Interactable;
using com.convalise.UnityMaterialSymbols;
using Readymade.Utils.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace App.Core.POI
{
    public class PointOfInterestPresenter : MonoBehaviour
    {
        private enum Origin
        {
            Observer,
            World
        }

        private enum ProjectionMode
        {
            HeadUpDisplay,
            Radar,
            Map
        }

        private enum MapMode
        {
            FollowNorth,
            FollowForward,
            Fixed
        }

        private enum RadarMode
        {
            FollowForward
        }

        [Required] [SerializeField] private PointOfInterestSystem system;
        [Required] [SerializeField] private PointOfInterestDisplay display;
        [Required] [SerializeField] private BlipDisplay blipPrefab;
        [SerializeField] private ProjectionMode projection;

        [ShowIf(nameof(projection), ProjectionMode.HeadUpDisplay)]
        [SerializeField]
        private Camera pointOfView;

        [ShowIf(nameof(projection), ProjectionMode.Radar)]
        [SerializeField]
        private RadarMode radarMode = RadarMode.FollowForward;

        [ShowIf(nameof(projection), ProjectionMode.Map)]
        [SerializeField]
        private MapMode mapMode = MapMode.FollowNorth;

        private readonly Dictionary<PointOfInterest, BlipDisplay> _poiToBlip = new();
        private readonly Dictionary<BlipDisplay, PointOfInterest> _blipToPoi = new();

        [ShowIf(nameof(projection), ProjectionMode.Radar)]
        [Min(0)]
        [SerializeField]
        private float radarRange = 100;

        [ShowIf(nameof(projection), ProjectionMode.Radar)]
        [Min(0)]
        [SerializeField]
        private float radarDisplayRadius = 300f;

        [ShowIf(nameof(projection), ProjectionMode.Map)]
        [Min(1)]
        [SerializeField]
        private float mapScale = 100f;

        [ShowIf(nameof(projection), ProjectionMode.Map)]
        [Min(1)]
        [SerializeField]
        private float mapDisplayUnit = 100f;

        [SerializeField] private MaterialSymbolData defaultSymbol = new('\ue6b7', true);

        private Camera _main;
        private Canvas _containerCanvas;

        private void OnEnable()
        {
            _main = Camera.main;
            _containerCanvas = display.Container.GetComponentInParent<Canvas>(true).rootCanvas;
            system.CompositionChanged += OnSystemChanged;
            pointOfView ??= system.Observer.GetComponentInChildren<Camera>();
            pointOfView ??= Camera.main;
        }

        private void Update()
        {
            if (!display.Container.gameObject.activeInHierarchy)
            {
                return;
            }

            switch (projection)
            {
                case ProjectionMode.HeadUpDisplay:
                    UpdateHUDProjection();
                    break;
                case ProjectionMode.Radar:
                    UpdateRadarProjection();
                    break;
                case ProjectionMode.Map:
                    UpdateMapProjection();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateRadarProjection()
        {
            if (display.BoundaryIndicator)
            {
                display.BoundaryIndicator.gameObject.SetActive(true);
                display.BoundaryIndicator.sizeDelta = Vector2.one * (radarDisplayRadius * 2f);
            }

            if (display.ScaleIndicator)
            {
                display.ScaleIndicator.gameObject.SetActive(true);
                display.ScaleIndicator.SetText("Radar\n{0}m", radarRange);
            }

            foreach (var pointOfInterest in system.PointsOfInterest)
            {
                if (!pointOfInterest.IsVisible)
                {
                    _poiToBlip[pointOfInterest].gameObject.SetActive(false);
                    continue;
                }

                float distance = system.GetDistance(pointOfInterest);
                if (distance > pointOfInterest.MaxRange)
                {
                    _poiToBlip[pointOfInterest].gameObject.SetActive(false);
                    continue;
                }

                Vector3 north = Vector3.forward;
                Vector3 pos = system.GetPosition(pointOfInterest);
                Vector3 delta = system.GetDelta(pointOfInterest);


                Vector3 observerForward = Vector3.ProjectOnPlane(system.Observer.forward, Vector3.up).normalized;
                Vector3 radarDelta3 = Quaternion.FromToRotation(observerForward, north) *
                    Vector3.ProjectOnPlane(delta, Vector3.up);
                Vector2 radarDelta2 = new Vector2(radarDelta3.x, radarDelta3.z);
                Vector2 clampedRadarDelta2 = Vector2.ClampMagnitude(radarDelta2, radarRange);
                Vector2 normalizedRadarDelta2 = clampedRadarDelta2 / radarRange;
                Vector2 scaledRadarDelta2 = normalizedRadarDelta2 * radarDisplayRadius;
                Vector3 blipPositionInRect = display.Container.rect.center + scaledRadarDelta2;
                BlipDisplay blip = _poiToBlip[pointOfInterest];
                blip.Pivot.anchoredPosition = blipPositionInRect;
                blip.gameObject.SetActive(true);
            }
        }

        private void UpdateMapProjection()
        {
            if (display.BoundaryIndicator)
            {
                display.BoundaryIndicator.gameObject.SetActive(true);
                display.BoundaryIndicator.sizeDelta = mapDisplayUnit * Vector2.one;
            }

            if (display.ScaleIndicator)
            {
                display.ScaleIndicator.gameObject.SetActive(true);
                display.ScaleIndicator.SetText("Map\n{0}m", mapScale);
            }

            foreach (var pointOfInterest in system.PointsOfInterest)
            {
                if (!pointOfInterest.IsVisible)
                {
                    _poiToBlip[pointOfInterest].gameObject.SetActive(false);
                    continue;
                }

                Vector3 observerForwardInPlane = Vector3.ProjectOnPlane(system.Observer.forward, Vector3.up).normalized;
                Vector3 alignDirection = mapMode switch
                {
                    MapMode.FollowNorth => Vector3.forward,
                    MapMode.FollowForward => Vector3.ProjectOnPlane(observerForwardInPlane, Vector3.up),
                    MapMode.Fixed => Vector3.forward,
                    _ => throw new ArgumentOutOfRangeException()
                };

                Vector3 delta = Vector3.ProjectOnPlane(system.GetDelta(pointOfInterest), Vector3.up);
                Vector3 alignedDirection = Quaternion.FromToRotation(alignDirection, observerForwardInPlane) * delta;
                Vector2 mapDirection =
                    new Vector2(alignedDirection.x, alignedDirection.z) / mapScale * mapDisplayUnit;
                Vector3 blipPositionInRect = display.Container.rect.center + mapDirection;
                BlipDisplay blip = _poiToBlip[pointOfInterest];
                if (display.Container.rect.Contains(blipPositionInRect))
                {
                    blip.gameObject.SetActive(true);
                    blip.Pivot.anchoredPosition = blipPositionInRect;
                }
                else
                {
                    blip.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateHUDProjection()
        {
            if (display.BoundaryIndicator)
            {
                display.BoundaryIndicator.gameObject.SetActive(false);
            }

            if (display.ScaleIndicator)
            {
                display.ScaleIndicator.gameObject.SetActive(false);
            }

            var plane = new Plane(system.Observer.forward, system.Observer.position);

            foreach (var pointOfInterest in system.PointsOfInterest)
            {
                if (!pointOfInterest.IsVisible)
                {
                    _poiToBlip[pointOfInterest].gameObject.SetActive(false);
                    continue;
                }

                float distance = system.GetDistance(pointOfInterest);
                if (distance > pointOfInterest.MaxRange)
                {
                    _poiToBlip[pointOfInterest].gameObject.SetActive(false);
                    continue;
                }

                Vector3 pos = system.GetPosition(pointOfInterest);
                bool isInFront = plane.GetSide(pos);
                BlipDisplay blip = _poiToBlip[pointOfInterest];
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(pointOfView, pos);
                bool isHit = RectTransformUtility.ScreenPointToLocalPointInRectangle(display.Container, screenPoint,
                    null, out var containerPoint);
                if (isHit)
                {
                    Rect rect = display.Container.rect;
                    if (isInFront)
                    {
                        containerPoint = new Vector2(
                            Mathf.Clamp(containerPoint.x, rect.xMin, rect.xMax),
                            Mathf.Clamp(containerPoint.y, rect.yMin, rect.yMax)
                        );
                    }
                    else
                    {
                        var n = -containerPoint.normalized;
                        var r = Mathf.Max(rect.width, rect.height);
                        var p = n * r;

                        containerPoint = ClampToRect(p, rect);
                    }

                    blip.Pivot.anchoredPosition = containerPoint;
                    blip.gameObject.SetActive(true);
                }
                else
                {
                    blip.gameObject.SetActive(false);
                }
            }
        }

        private static Vector2 ClampToRect(Vector2 p, Rect rect)
        {
            Vector2 containerPoint;
            if (Mathf.Abs(p.x) * rect.height <= Mathf.Abs(p.y) * rect.width)
            {
                containerPoint = new Vector2(
                    rect.center.x + rect.height / 2f * p.x / Mathf.Abs(p.y),
                    rect.center.y + Mathf.Sign(p.y) * rect.height / 2f
                );
            }
            else
            {
                containerPoint = new Vector2(
                    rect.center.x + Mathf.Sign(p.x) * rect.width / 2f,
                    rect.center.y + rect.width / 2f * p.y / Mathf.Abs(p.x)
                );
            }

            return containerPoint;
        }

        private void OnSystemChanged(
            SystemBase<PointOfInterest> source,
            ISystemComponent<SystemBase<PointOfInterest>> component,
            RegistrationEvent shape
        )
        {
            switch (shape)
            {
                case RegistrationEvent.Added:
                    OnAdded((PointOfInterest)component);
                    break;
                case RegistrationEvent.Removed:
                    OnRemoved((PointOfInterest)component);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
            }
        }

        private void OnRemoved(PointOfInterest poi)
        {
            if (_poiToBlip.Remove(poi, out BlipDisplay blip))
            {
                _blipToPoi.Remove(blip);
                if (blip)
                {
                    blip.GetComponent<PooledInstance>().Release();
                }
            }
        }

        private void OnAdded(PointOfInterest poi)
        {
            GameObjectPool.TryGetInstance(blipPrefab, out BlipDisplay blip, out PooledInstance handle);
            blip.transform.SetParent(display.Container);
            blip.gameObject.SetActive(true);
            blip.Symbol.symbol = poi.Symbol.code == default ? defaultSymbol : poi.Symbol;
            blip.Symbol.color = poi.Color;
            _poiToBlip.Add(poi, blip);
            _blipToPoi.Add(blip, poi);
        }
    }
}