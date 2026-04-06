using System;
using System.Collections.Generic;

public class SpinWheelUseCase
{
    private readonly ISpinAvailabilityService spinAvailabilityService;
    private readonly IWheelRewardResolver wheelRewardResolver;
    private readonly IWheelRewardGrantService wheelRewardGrantService;
    private readonly IWheelAngleCalculator wheelAngleCalculator;
    private readonly WheelRotationAnimation wheelRotationAnimation;
    private readonly WheelConfig wheelConfig;

    public SpinWheelUseCase(
        ISpinAvailabilityService spinAvailabilityService,
        IWheelRewardResolver wheelRewardResolver,
        IWheelRewardGrantService wheelRewardGrantService,
        IWheelAngleCalculator wheelAngleCalculator,
        WheelRotationAnimation wheelRotationAnimation,
        WheelConfig wheelConfig)
    {
        this.spinAvailabilityService = spinAvailabilityService;
        this.wheelRewardResolver = wheelRewardResolver;
        this.wheelRewardGrantService = wheelRewardGrantService;
        this.wheelAngleCalculator = wheelAngleCalculator;
        this.wheelRotationAnimation = wheelRotationAnimation;
        this.wheelConfig = wheelConfig;
    }

    public void Execute(Action<WheelSpinExecutionResult> onCompleted)
    {
        WheelSpinExecutionResult result = new WheelSpinExecutionResult();

        if (wheelConfig == null)
        {
            result.success = false;
            result.message = "Wheel config is missing.";
            onCompleted?.Invoke(result);
            return;
        }

        if (wheelRotationAnimation == null)
        {
            result.success = false;
            result.message = "Wheel animator is missing.";
            onCompleted?.Invoke(result);
            return;
        }

        if (wheelRotationAnimation.IsSpinning)
        {
            result.success = false;
            result.message = "Wheel is already spinning.";
            onCompleted?.Invoke(result);
            return;
        }

        if (!spinAvailabilityService.CanSpin())
        {
            result.success = false;
            result.remainingCooldown = spinAvailabilityService.GetRemainingCooldown();
            result.message = "Spin is on cooldown.";
            onCompleted?.Invoke(result);
            return;
        }

        List<WheelRewardDto> rewards = wheelConfig.rewards;
        WheelRewardDto selectedReward = wheelRewardResolver.ResolveReward(rewards);

        if (selectedReward == null)
        {
            result.success = false;
            result.message = "No reward could be resolved.";
            onCompleted?.Invoke(result);
            return;
        }

        float targetAngle = wheelAngleCalculator.CalculateTargetAngle(
            selectedReward.sliceIndex,
            wheelConfig.sliceCount
        );

        float finalSpinAngle = (wheelConfig.extraFullRotations * 360f) + targetAngle;

        wheelRotationAnimation.Play(finalSpinAngle, wheelConfig.spinDuration, () =>
        {
            wheelRewardGrantService.GrantReward(selectedReward);
            spinAvailabilityService.ConsumeSpin();

            result.success = true;
            result.reward = selectedReward;
            result.targetAngle = targetAngle;
            result.remainingCooldown = spinAvailabilityService.GetRemainingCooldown();
            result.message = "Spin completed.";

            onCompleted?.Invoke(result);
        });
    }
}