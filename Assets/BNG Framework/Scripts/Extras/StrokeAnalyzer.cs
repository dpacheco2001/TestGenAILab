using System.Collections.Generic;
using UnityEngine;

namespace BNG { // Opcional: si Marker está en BNG, ponlo en el mismo namespace.
    public class StrokeAnalyzer : MonoBehaviour
    {
        [Header("Stroke Settings")]
        [Tooltip("Distancia máxima entre el primer y último punto para considerar el trazo cerrado.")]
        public float closeThreshold = 0.05f;

        private List<Vector3> strokePoints = new List<Vector3>();

        /// <summary>
        /// Agrega un punto al trazo.
        /// </summary>
        public void AddPoint(Vector3 point)
        {
            strokePoints.Add(point);
        }

        /// <summary>
        /// Indica si el trazo está cerrado (el primer y último punto están muy cerca).
        /// </summary>
        public bool IsStrokeClosed()
        {
            if (strokePoints.Count < 2)
                return false;

            float dist = Vector3.Distance(strokePoints[0], strokePoints[strokePoints.Count - 1]);
            return dist <= closeThreshold;
        }

        /// <summary>
        /// Limpia la lista de puntos.
        /// </summary>
        public void ResetStroke()
        {
            strokePoints.Clear();
        }

        /// <summary>
        /// Comprueba si el trazo cerrado (proyectado a 2D en el plano XY) encierra el punto objetivo.
        /// Se asume que el trazo debe estar cerrado para realizar la comprobación.
        /// </summary>
        /// <param name="targetPoint">Punto a comprobar (en 2D, por ejemplo, X e Y).</param>
        /// <returns>True si el punto está dentro, false en caso contrario.</returns>
        public bool ContainsPoint(Vector2 targetPoint)
        {
            if (!IsStrokeClosed() || strokePoints.Count < 3)
                return false;

            // Proyectamos los puntos a 2D (plano XY)
            List<Vector2> polygon = new List<Vector2>();
            foreach (var p in strokePoints)
            {
                polygon.Add(new Vector2(p.x, p.y));
            }

            return IsPointInPolygon(polygon, targetPoint);
        }

        /// <summary>
        /// Algoritmo ray-casting para determinar si un punto está dentro de un polígono 2D.
        /// </summary>
        private bool IsPointInPolygon(List<Vector2> polygon, Vector2 point)
        {
            bool inside = false;
            int j = polygon.Count - 1;
            for (int i = 0; i < polygon.Count; i++)
            {
                if ((polygon[i].y > point.y) != (polygon[j].y > point.y))
                {
                    float intersectX = (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x;
                    if (point.x < intersectX)
                    {
                        inside = !inside;
                    }
                }
                j = i;
            }
            return inside;
        }

        /// <summary>
        /// (Opcional) Permite acceder a los puntos del trazo.
        /// </summary>
        public List<Vector3> GetStrokePoints()
        {
            return strokePoints;
        }
    }
}
