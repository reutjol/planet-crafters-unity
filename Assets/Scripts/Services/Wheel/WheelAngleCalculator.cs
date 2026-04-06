public class WheelAngleCalculator : IWheelAngleCalculator
{
    public float CalculateTargetAngle(int sliceIndex, int sliceCount)
    {
        if (sliceCount <= 0)
            return 0f;

        float sliceAngle = 360f / sliceCount;
        float topOffset = 90f;

        return topOffset - (sliceIndex * sliceAngle);
    }
}