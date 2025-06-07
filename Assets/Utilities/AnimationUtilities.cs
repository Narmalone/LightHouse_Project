using UnityEngine;

namespace LightHouse.Utilities
{
    public static class AnimationUtilities
    {
        public static AnimationCurve LerpCurve(AnimationCurve a, AnimationCurve b, float t)
        {
            if (a == null || b == null)
                return a ?? b;

            // Si les courbes ont le même nombre de keyframes → interpolation complète
            if (a.length == b.length)
            {
                Keyframe[] keys = new Keyframe[a.length];

                for (int i = 0; i < a.length; i++)
                {
                    Keyframe ka = a[i];
                    Keyframe kb = b[i];

                    keys[i] = new Keyframe(
                        Mathf.Lerp(ka.time, kb.time, t),
                        Mathf.Lerp(ka.value, kb.value, t),
                        Mathf.Lerp(ka.inTangent, kb.inTangent, t),
                        Mathf.Lerp(ka.outTangent, kb.outTangent, t)
                    );
                }

                return new AnimationCurve(keys);
            }
            else
            {
                // Si longueur différente → on échantillonne et recrée la courbe
                int steps = Mathf.Max(a.length, b.length, 8);
                Keyframe[] sampledKeys = new Keyframe[steps];

                float start = Mathf.Min(a[0].time, b[0].time);
                float end = Mathf.Max(a[a.length - 1].time, b[b.length - 1].time);

                for (int i = 0; i < steps; i++)
                {
                    float time = Mathf.Lerp(start, end, i / (float)(steps - 1));
                    float va = a.Evaluate(time);
                    float vb = b.Evaluate(time);
                    float value = Mathf.Lerp(va, vb, t);

                    sampledKeys[i] = new Keyframe(time, value);
                }

                return new AnimationCurve(sampledKeys);
            }
        }
    }

}
