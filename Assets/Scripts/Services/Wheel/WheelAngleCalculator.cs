public class WheelAngleCalculator : IWheelAngleCalculator
{
    // Wheel rotates clockwise (currentZRotation decreases).
    // Slice i is placed at local angle: 90 - i * sliceAngle.
    // After spinning by deltaAngle, Z = currentZRotation - deltaAngle.
    // Slice sliceIndex is at world angle: (90 - sliceIndex*sliceAngle) + Z_final = 90
    // => deltaAngle = currentZRotation - sliceIndex*sliceAngle  (mod 360)
    public float CalculateTargetAngle(int sliceIndex, int sliceCount, float currentZRotation)
    {
        if (sliceCount <= 0)
            return 0f;

        float sliceAngle = 360f / sliceCount;
        float raw = (currentZRotation - sliceIndex * sliceAngle) % 360f;
        float targetAngle = ((raw % 360f) + 360f) % 360f;

        // Ensure at least a small rotation so the wheel visibly spins to the slice
        if (targetAngle < 0.001f)
            targetAngle = 360f;

        return targetAngle;
    }
}