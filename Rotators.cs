using Godot;
using System;
using System.Diagnostics;

namespace Stage
{
    internal static class Rotators
    {
        private const float Delta = 0.01F;

        public static Quaternion Rotate(float t, float speed, float offset, Vector3 axis, Quaternion baseRotation)
        {
            Quaternion rotation = new(axis, speed * t + offset);
            return rotation * baseRotation;
        }

        public static Quaternion RotateDerivative(float t, float speed, float offset, Vector3 axis, Quaternion baseRotation)
        {
            return (Rotate(t + Delta, speed, offset, axis, baseRotation) - Rotate(t, speed, offset, axis, baseRotation)) / Delta;
        }

        public static Quaternion RotateDoubleDerivative(float t, float speed, float offset, Vector3 axis, Quaternion baseRotation)
        {
            return (RotateDerivative(t + Delta, speed, offset, axis, baseRotation) - RotateDerivative(t, speed, offset, axis, baseRotation)) / Delta;
        }

        public static Quaternion IntegrateOntoRotate(Quaternion start, float t, float speed, float offset, Vector3 axis, Quaternion baseRotation)
        {
            if (t < 0)
            {
                return start;
            }

            if (t > 1)
            {
                return Rotate(t, speed, offset, axis, baseRotation);
            }

            Quaternion f1 = Rotate(1, speed, offset, axis, baseRotation);
            Quaternion fp1 = RotateDerivative(1, speed, offset, axis, baseRotation);
            Quaternion fpp1 = RotateDoubleDerivative(1, speed, offset, axis, baseRotation);

            float t3 = t * t * t;
            float t4 = t3 * t;
            float t5 = t4 * t;

            Quaternion a = 6 * f1 - 3 * fp1 + fpp1 / 2F - 6 * start;
            Quaternion b = -15 * f1 + 7 * fp1 - fpp1 + 15 * start;
            Quaternion c = 10 * f1 - 4 * fp1 + fpp1 / 2F - 10 * start;

            Quaternion result = t5 * a + t4 * b + t3 * c + start;
            return result.Normalized();
        }
    }
}
