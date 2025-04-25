using System.Collections;
using UnityEngine;

namespace BNG {

    public class Marker : GrabbableEvents {

        public Material DrawMaterial;
        public Color DrawColor = Color.red;
        public float LineWidth = 0.02f;

        public Transform RaycastStart;
        public LayerMask DrawingLayers;
        public float RaycastLength = 0.01f;
        public float MinDrawDistance = 0.02f;
        public float maxPointDelta = 0.1f;
        public float ReuseTolerance = 0.001f;

        bool IsNewDraw = true;
        Vector3 lastDrawPoint;
        LineRenderer LineRenderer;
        Transform currentStrokeParent;
        Transform lastTransform;
        Coroutine drawRoutine = null;
        float lastLineWidth = 0;
        int renderLifeTime = 0;

        void Start() {
            currentStrokeParent = new GameObject("StrokeParent").transform;
            drawRoutine = StartCoroutine(WriteRoutine());
        }

        // Al presionar el gatillo (>0.5) borramos todos los trazos
        public override void OnTrigger(float triggerValue) {
            if(triggerValue > 0.5f) {
                if(currentStrokeParent != null) {
                    Destroy(currentStrokeParent.gameObject);
                }
                currentStrokeParent = new GameObject("StrokeParent").transform;
                IsNewDraw = true;
                LineRenderer = null;
            }

            base.OnTrigger(triggerValue);
        }

        IEnumerator WriteRoutine() {
            var wait = new WaitForSeconds(0.02f);
            while (true) {
                if (Physics.Raycast(RaycastStart.position,
                                   RaycastStart.up,
                                   out var hit,
                                   RaycastLength,
                                   DrawingLayers,
                                   QueryTriggerInteraction.Ignore)) {

                    float tipDistance   = Vector3.Distance(hit.point, RaycastStart.position);
                    float tipPercentage = tipDistance / RaycastLength;
                    Vector3 drawStart   = hit.point - RaycastStart.up * 0.0005f;
                    Quaternion drawRot  = Quaternion.FromToRotation(Vector3.back, hit.normal);
                    float  w            = LineWidth * (1 - tipPercentage);

                    InitDraw(drawStart, drawRot, w, DrawColor);
                }
                else {
                    IsNewDraw    = true;
                    LineRenderer = null;
                }

                yield return wait;
            }
        }

        void InitDraw(Vector3 position, Quaternion rotation, float lineWidth, Color lineColor) {
            if (IsNewDraw) {
                lastDrawPoint = position;
                DrawPoint(lastDrawPoint, position, lineWidth, lineColor, rotation);
                IsNewDraw = false;
            }
            else {
                float dist = Vector3.Distance(lastDrawPoint, position);
                if (dist > MinDrawDistance) {
                    lastDrawPoint = DrawPoint(lastDrawPoint, position, lineWidth, lineColor, rotation);
                }
            }
        }

        Vector3 DrawPoint(Vector3 prevPoint,
                          Vector3 endPosition,
                          float   lineWidth,
                          Color   lineColor,
                          Quaternion rotation) {

            if (Vector3.Distance(prevPoint, endPosition) > maxPointDelta)
                return prevPoint;

            float dif = Mathf.Abs(lastLineWidth - lineWidth);
            lastLineWidth = lineWidth;

            if (LineRenderer == null || dif > ReuseTolerance || renderLifeTime >= 98) {
                LineRenderer = null;
                renderLifeTime = 0;
            }
            else {
                renderLifeTime++;
            }

            if (LineRenderer == null) {
                lastTransform = new GameObject("DrawLine").transform;
                lastTransform.parent   = currentStrokeParent;
                lastTransform.position = endPosition;
                lastTransform.rotation = rotation;

                LineRenderer = lastTransform.gameObject.AddComponent<LineRenderer>();
                LineRenderer.startColor     = lineColor;
                LineRenderer.endColor       = lineColor;
                LineRenderer.startWidth     = lineWidth;
                LineRenderer.endWidth       = lineWidth;

                var curve = new AnimationCurve();
                curve.AddKey(0, lineWidth);
                LineRenderer.widthCurve     = curve;

                if (DrawMaterial != null)
                    LineRenderer.material = DrawMaterial;

                LineRenderer.numCapVertices = 5;
                LineRenderer.alignment      = LineAlignment.TransformZ;
                LineRenderer.useWorldSpace  = true;

                LineRenderer.positionCount  = 2;
                LineRenderer.SetPosition(0, prevPoint);
                LineRenderer.SetPosition(1, endPosition);
            }
            else {
                LineRenderer.positionCount++;
                var curve = LineRenderer.widthCurve;
                curve.AddKey((LineRenderer.positionCount - 1) / 100f, lineWidth);
                LineRenderer.widthCurve = curve;
                LineRenderer.SetPosition(LineRenderer.positionCount - 1, endPosition);
            }

            return endPosition;
        }

        void OnDrawGizmosSelected() {
            if (RaycastStart != null) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(RaycastStart.position,
                                RaycastStart.position + RaycastStart.up * RaycastLength);
            }
        }
    }
}
