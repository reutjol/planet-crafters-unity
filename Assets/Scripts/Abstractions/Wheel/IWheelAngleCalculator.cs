public interface IWheelAngleCalculator
{
    float CalculateTargetAngle(int sliceIndex, int sliceCount, float currentZRotation);
}