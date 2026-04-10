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
        return new WheelAvailabilityService(
            wheelStateRepository,
            TimeSpan.FromHours(wheelConfig.cooldownHours)
        );
    }

    public static IWheelRewardResolver CreateRewardResolver()
    {
        return new WeightedWheelRewardResolver();
    }

    public static IWheelRewardGrantService CreateRewardGrantService()
    {
        return new WheelRewardGrantService();
    }

    public static IWheelAngleCalculator CreateAngleCalculator()
    {
        return new WheelAngleCalculator();
    }
}