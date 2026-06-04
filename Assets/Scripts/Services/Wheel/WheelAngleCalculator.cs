public class WheelAngleCalculator : IWheelAngleCalculator
{
    public float CalculateTargetAngle(int sliceIndex, int sliceCount, float currentWheelZRotation)
    {
        if (sliceCount <= 0)
            return 0f;

        float sliceAngle = 360f / sliceCount;
        float selectedSliceCenterAngle = 90f - (sliceIndex * sliceAngle);
        float pointerAngle = 90f;
        float clockwiseDeltaToPointer = currentWheelZRotation + selectedSliceCenterAngle - pointerAngle;

        return NormalizeAngle(clockwiseDeltaToPointer);
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle < 0f)
            angle += 360f;

        return angle;
    }
}
