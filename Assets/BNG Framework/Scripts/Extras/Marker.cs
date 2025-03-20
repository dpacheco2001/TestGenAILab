using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    [System.Serializable]
    public class EnclosableObject {
        [Tooltip("Objeto que puede ser encerrado.")]
        public Transform target;
        [Tooltip("Indica si el objeto fue encerrado por un trazo.")]
        public bool isEnclosed = false;
    }

    public class Marker : GrabbableEvents {

        public Material DrawMaterial;
        public Color DrawColor = Color.red;
        public float LineWidth = 0.02f;

        public Transform RaycastStart;
        public LayerMask DrawingLayers;
        public float RaycastLength = 0.01f;
        public float MinDrawDistance = 0.02f;
        public float ReuseTolerance = 0.001f;

        [Tooltip("Lista de objetos que pueden ser encerrados.")]
        public List<EnclosableObject> enclosableObjects = new List<EnclosableObject>();

        [Tooltip("Tiempo de espera antes de borrar el trazo si no se cierra (en segundos).")]
        public float eraseDelay = 0.5f;

        [Tooltip("Si la diferencia entre puntos es mayor que este valor se descarta (para evitar ruido).")]
        public float maxPointDelta = 0.1f;

        bool IsNewDraw = false;
        Vector3 lastDrawPoint;
        LineRenderer LineRenderer;
        // Cada trazo tendrá su contenedor independiente.
        private Transform currentStrokeParent = null;
        Transform lastTransform;
        Coroutine drawRoutine = null;
        float lastLineWidth = 0;
        int renderLifeTime = 0;

        // Referencia al StrokeAnalyzer para almacenar los puntos del trazo.
        private StrokeAnalyzer strokeAnalyzer;

        // Usamos OnTrigger para iniciar/detener el dibujo según el gatillo.
        public override void OnTrigger(float triggerValue) {
            if (triggerValue > 0.5f) {
                if(drawRoutine == null) {
                    // Iniciamos un nuevo trazo: creamos el StrokeAnalyzer y el contenedor.
                    strokeAnalyzer = gameObject.AddComponent<StrokeAnalyzer>();
                    currentStrokeParent = new GameObject("StrokeParent").transform;
                    IsNewDraw = true;
                    drawRoutine = StartCoroutine(WriteRoutine());
                }
            }
            else {
                if(drawRoutine != null) {
                    StopCoroutine(drawRoutine);
                    drawRoutine = null;
                }
                if(strokeAnalyzer != null) {
                    bool closed = strokeAnalyzer.IsStrokeClosed();
                    Debug.Log(closed ? "¡El trazo está cerrado!" : "El trazo no está cerrado.");
                    if(closed) {
                        // Para cada objeto de la lista, comprobamos si está encerrado.
                        foreach(var obj in enclosableObjects) {
                            if(obj.target != null) {
                                Vector2 targetPos2D = new Vector2(obj.target.position.x, obj.target.position.y);
                                bool encloses = strokeAnalyzer.ContainsPoint(targetPos2D);
                                if(encloses) {
                                    obj.isEnclosed = true;
                                    Debug.Log("Se ha encerrado: " + obj.target.name);
                                }
                                else {
                                    obj.isEnclosed = false;
                                }
                            }
                        }
                    }
                    if(!closed) {
                        StartCoroutine(EraseStrokeAfterDelay());
                    }
                    Destroy(strokeAnalyzer);
                }
            }
            base.OnTrigger(triggerValue);
        }

        IEnumerator WriteRoutine() {
            // Usamos un delay fijo en lugar de FixedUpdate para reducir la frecuencia de raycasts.
            WaitForSeconds wait = new WaitForSeconds(0.02f);
            while (true) {
                if (Physics.Raycast(RaycastStart.position, RaycastStart.up, out RaycastHit hit, RaycastLength, DrawingLayers, QueryTriggerInteraction.Ignore)) {
                    float tipDistance = Vector3.Distance(hit.point, RaycastStart.position);
                    float tipPercentage = tipDistance / RaycastLength;
                    Vector3 drawStart = hit.point + (-RaycastStart.up * 0.0005f);
                    Quaternion drawRotation = Quaternion.FromToRotation(Vector3.back, hit.normal);
                    float lineWidth = LineWidth * (1 - tipPercentage);
                    InitDraw(drawStart, drawRotation, lineWidth, DrawColor);
                }
                else {
                    IsNewDraw = true;
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

        Vector3 DrawPoint(Vector3 prevPoint, Vector3 endPosition, float lineWidth, Color lineColor, Quaternion rotation) {
            // Si el delta es excesivo, ignoramos el punto (para evitar saltos bruscos)
            if(Vector3.Distance(prevPoint, endPosition) > maxPointDelta) {
                return prevPoint;
            }

            float dif = Mathf.Abs(lastLineWidth - lineWidth);
            lastLineWidth = lineWidth;
            if (dif > ReuseTolerance || renderLifeTime >= 98) {
                LineRenderer = null;
                renderLifeTime = 0;
            }
            else {
                renderLifeTime += 1;
            }
            if (IsNewDraw || LineRenderer == null) {
                lastTransform = new GameObject("DrawLine").transform;
                if (currentStrokeParent == null) {
                    currentStrokeParent = new GameObject("StrokeParent").transform;
                }
                lastTransform.parent = currentStrokeParent;
                lastTransform.position = endPosition;
                lastTransform.rotation = rotation;
                LineRenderer = lastTransform.gameObject.AddComponent<LineRenderer>();

                LineRenderer.startColor = lineColor;
                LineRenderer.endColor = lineColor;
                LineRenderer.startWidth = lineWidth;
                LineRenderer.endWidth = lineWidth;
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0, lineWidth);
                LineRenderer.widthCurve = curve;
                if (DrawMaterial) {
                    LineRenderer.material = DrawMaterial;
                }
                LineRenderer.numCapVertices = 5;
                LineRenderer.alignment = LineAlignment.TransformZ;
                LineRenderer.useWorldSpace = true;
                LineRenderer.SetPosition(0, prevPoint);
                LineRenderer.SetPosition(1, endPosition);
            }
            else {
                if (LineRenderer != null) {
                    LineRenderer.widthMultiplier = 1;
                    LineRenderer.positionCount += 1;
                    AnimationCurve curve = LineRenderer.widthCurve;
                    curve.AddKey((LineRenderer.positionCount - 1) / 100f, lineWidth);
                    LineRenderer.widthCurve = curve;
                    LineRenderer.SetPosition(LineRenderer.positionCount - 1, endPosition);
                }
            }

            if (strokeAnalyzer != null) {
                strokeAnalyzer.AddPoint(endPosition);
            }
            return endPosition;
        }

        IEnumerator EraseStrokeAfterDelay() {
            yield return new WaitForSeconds(eraseDelay);
            if(currentStrokeParent != null) {
                Destroy(currentStrokeParent.gameObject);
                currentStrokeParent = null;
                Debug.Log("Trazo borrado por no estar cerrado.");
            }
        }

        // MÉTODO NUEVO:
        // Este método devuelve una cadena con los nombres de los objetos encerrados y borra todos los trazos.
        public string GetEnclosedObjectsAndClearStrokes() {
            List<string> enclosedNames = new List<string>();
            foreach (var obj in enclosableObjects) {
                if (obj.target != null && obj.isEnclosed) {
                    enclosedNames.Add(obj.target.name);
                }
            }

            string result = (enclosedNames.Count > 0) ? string.Join(", ", enclosedNames) : "Ningún objeto encerrado";
            Debug.Log("Respuesta GET: " + result);

            // Borramos todos los trazos
            if (currentStrokeParent != null) {
                Destroy(currentStrokeParent.gameObject);
                currentStrokeParent = null;
                Debug.Log("Trazos borrados tras la solicitud GET.");
            }

            return result;
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(RaycastStart.position, RaycastStart.position + RaycastStart.up * RaycastLength);
        }
    }
}
