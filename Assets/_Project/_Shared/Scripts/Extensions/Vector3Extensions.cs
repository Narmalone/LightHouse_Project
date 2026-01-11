using UnityEngine;

namespace LightHouse.Core.Extensions
{
    public static class Vector3Extensions
    {
        #region Vectors / Positions To Int

        #region Pos To Int
        public static Vector3Int PositionToInt(this Transform transform)
        {
            Vector3 position = transform.position;

            return new Vector3Int(Mathf.RoundToInt(position.x),
                                  Mathf.RoundToInt(position.y),
                                  Mathf.RoundToInt(position.z));
        }
        public static Vector2Int PositionToIntVector2(this Transform transform)
        {
            Vector2 position = transform.position;

            return new Vector2Int(Mathf.RoundToInt(position.x),
                                  Mathf.RoundToInt(position.y));
        }
        #endregion

        #region Vector 2
        public static Vector2 VectorToInt(this Vector2 v)
        {
            return new Vector3(
                Mathf.RoundToInt(v.x),
                Mathf.RoundToInt(v.y));
        }
        public static Vector2 VectorToIntX(this Vector2 v)
        {
            return new Vector2(
                Mathf.RoundToInt(v.x),
                v.y);
        }
        public static Vector2 VectorToIntY(this Vector2 v)
        {
            return new Vector2(
                v.x,
                Mathf.RoundToInt(v.y));
        }
        #endregion

        #region Vector 3
        public static Vector3 VectorToInt(this Vector3 v)
        {
            return new Vector3(
                Mathf.RoundToInt(v.x),
                Mathf.RoundToInt(v.y),
                Mathf.RoundToInt(v.z));
        }

        public static Vector3 VectorToIntX(this Vector3 v)
        {
            return new Vector3(
                Mathf.RoundToInt(v.x),
                v.y,
                v.z);
        }

        public static Vector3 VectorToIntY(this Vector3 v)
        {
            return new Vector3(
                v.x,
                Mathf.RoundToInt(v.y),
                v.z);
        }

        public static Vector3 VectorToIntZ(this Vector3 v)
        {
            return new Vector3(
                v.x,
                v.y,
                Mathf.RoundToInt(v.z));
        }

        #endregion

        #endregion

        #region Distance
        // Calcule la distance entre deux vecteurs
        public static float DistanceTo(this Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }

        // Calcule la distance en 2D entre deux vecteurs (ignorant la composante Y)
        public static float DistanceTo2D(this Vector3 from, Vector3 to)
        {
            return Vector2.Distance(new Vector2(from.x, from.z), new Vector2(to.x, to.z));
        }

        // Calcule le carré de la distance entre deux vecteurs (plus rapide que DistanceTo)
        public static float SqrDistanceTo(this Vector3 from, Vector3 to)
        {
            return (from - to).sqrMagnitude;
        }

        #endregion

        #region Normalizing
        // Normalise le vecteur (le rend de longueur 1)
        public static Vector3 Normalize(this Vector3 vector)
        {
            return Vector3.Normalize(vector);
        }

        #endregion

        #region Direction
        public static Vector3 Direction(this Vector3 from, Vector3 to)
        {
            return to - from;
        }
        #endregion

        #region Clamping

        // Clamp le vecteur à une longueur maximale
        public static Vector3 ClampMagnitude(this Vector3 vector, float maxLength)
        {
            return Vector3.ClampMagnitude(vector, maxLength);
        }

        #endregion

        // Obtient la projection du vecteur sur un plan donné
        public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(vector, planeNormal);
        }

        #region NO Functions

        public static Vector3 NoX(this Vector3 v)
        {
            return new Vector3(0f, v.y, v.z);
        }
        public static Vector3 NoY(this Vector3 v)
        {
            return new Vector3(v.x, 0f, v.z);
        }
        public static Vector3 NoZ(this Vector3 v)
        {
            return new Vector3(v.x, v.y, 0f);
        }

        public static Vector2 NoX(this Vector2 v)
        {
            return new Vector2(0f, v.y);
        }

        public static Vector2 NoY(this Vector2 v)
        {
            return new Vector2(v.x, 0f);
        }

        #endregion

        #region Vector3 Calculation
        // Calcule le produit scalaire entre deux vecteurs
        public static float Dot(this Vector3 vector, Vector3 other)
        {
            return Vector3.Dot(vector, other);
        }

        // Calcule le produit vectoriel entre deux vecteurs
        public static Vector3 Cross(this Vector3 vector, Vector3 other)
        {
            return Vector3.Cross(vector, other);
        }

        // Calcule le vecteur réfléchi par rapport à une normale de surface
        public static Vector3 Reflect(this Vector3 vector, Vector3 surfaceNormal)
        {
            return Vector3.Reflect(vector, surfaceNormal);
        }

        #endregion

        #region Angles
        // Calcul de l'angle en degrés entre deux vecteurs
        public static float AngleTo(this Vector3 from, Vector3 to)
        {
            return Vector3.Angle(from, to);
        }

        // Calcul de l'angle signé en degrés entre deux vecteurs
        public static float SignedAngleTo(this Vector3 from, Vector3 to, Vector3 axis)
        {
            return Vector3.SignedAngle(from, to, axis);
        }
        #endregion

        // Rotation d'un vecteur autour de l'axe spécifié en degrés
        public static Vector3 RotateAround(this Vector3 vector, Vector3 axis, float angleDegrees)
        {
            Quaternion rotation = Quaternion.AngleAxis(angleDegrees, axis);
            return rotation * vector;
        }
    }
}