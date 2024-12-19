using UnityEngine;

namespace Readymade.Utils.Prototyping {
    [ExecuteAlways]
    [RequireComponent ( typeof ( LineRenderer ) )]
    public class DrawLineBetween : MonoBehaviour {
        [SerializeField]
        private Transform pointA;

        [SerializeField]
        private Transform pointB;

        private void OnEnable () {
            pointA.hasChanged = false;
            pointB.hasChanged = false;
            UpdatePositions ();
        }

        private void UpdatePositions () {
            LineRenderer lr = GetComponent<LineRenderer> ();
            lr.positionCount = 2;
            lr.SetPosition ( 0, pointA.position );
            lr.SetPosition ( 1, pointB.position );
        }

        private void LateUpdate () {
            if ( pointA && pointB && ( pointA.hasChanged || pointB.hasChanged ) ) {
                UpdatePositions ();
            }
        }
    }
}