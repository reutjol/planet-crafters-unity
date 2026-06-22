using System;

public static class WheelServicesFactory
{
    public static IWheelStateRepository CreateStateRepository()
    {
        return new LocalWheelStateRepository();
    }

    public static ISpinAvailabilityService CreateSpinAvailabilityService(
        IWheelStateRepository wheelStateRepository,
        WheelConfig wheelConfig)
    {
        return new ServerWheelAvailabilityService(WheelApiClient.GetOrCreate());
    }

    public static IWheelRewardResolver CreateRewardResolver()
    {
        return new WeightedWheelRewardResolver();
    }

    public static IWheelRewardGrantService CreateRewardGrantService(ISpinAvailabilityService spinAvailabilityService)
    {
        return new WheelRewardGrantService(spinAvailabilityService);
    }

    public static IWheelAngleCalculator CreateAngleCalculator()
    {
        return new WheelAngleCalculator();
    }
}
