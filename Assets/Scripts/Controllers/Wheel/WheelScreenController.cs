using System.Collections;
using TMPro;
using UnityEngine;

public class WheelScreenController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private WheelConfig wheelConfig;

    [Header("Views")]
    [SerializeField] private SpinButtonView spinButtonView;
    [SerializeField] private CooldownTextView cooldownTextView;
    [SerializeField] private TMP_Text rewardResultText;

    [Header("Animation")]
    [SerializeField] private WheelRotationAnimation wheelRotationAnimation;
    [SerializeField] private WheelSlicesBuilder wheelSlicesBuilder;

    private IWheelStateRepository wheelStateRepository;
    private ISpinAvailabilityService spinAvailabilityService;
    private IWheelRewardResolver wheelRewardResolver;
    private IWheelRewardGrantService wheelRewardGrantService;
    private IWheelAngleCalculator wheelAngleCalculator;
    private IWheelRewardIconProvider wheelRewardIconProvider;
    private ILocalizationService localizationService;

    private SpinWheelUseCase spinWheelUseCase;
    private WheelScreenPresenter wheelScreenPresenter;

    private Coroutine cooldownRefreshCoroutine;

    private void Awake()
    {
        BuildDependencies();
    }

    private void OnEnable()
    {
        if (spinButtonView != null)
            spinButtonView.Clicked += HandleSpinClicked;

        if (wheelRotationAnimation != null)
            wheelRotationAnimation.RotationChanged += HandleWheelRotationChanged;

        cooldownRefreshCoroutine = StartCoroutine(CooldownRefreshRoutine());
    }

    private void OnDisable()
    {
        if (spinButtonView != null)
            spinButtonView.Clicked -= HandleSpinClicked;

        if (wheelRotationAnimation != null)
            wheelRotationAnimation.RotationChanged -= HandleWheelRotationChanged;

        if (cooldownRefreshCoroutine != null)
        {
            StopCoroutine(cooldownRefreshCoroutine);
            cooldownRefreshCoroutine = null;
        }
    }

    private void Start()
    {
        wheelScreenPresenter?.PresentInitial();
    }

    private void BuildDependencies()
    {
        localizationService = UnityLocalizationService.Instance;

        wheelStateRepository = WheelServicesFactory.CreateStateRepository();

        spinAvailabilityService = WheelServicesFactory.CreateSpinAvailabilityService(
            wheelStateRepository,
            wheelConfig
        );

        wheelRewardResolver = WheelServicesFactory.CreateRewardResolver();
        wheelRewardGrantService = WheelServicesFactory.CreateRewardGrantService(spinAvailabilityService);
        wheelAngleCalculator = WheelServicesFactory.CreateAngleCalculator();

        wheelScreenPresenter = new WheelScreenPresenter(
            spinButtonView,
            cooldownTextView,
            spinAvailabilityService,
            localizationService,
            rewardResultText
        );

        spinWheelUseCase = new SpinWheelUseCase(
            spinAvailabilityService,
            wheelRewardResolver,
            wheelRewardGrantService,
            wheelAngleCalculator,
            wheelRotationAnimation,
            wheelConfig
        );

        wheelRewardIconProvider = new WheelRewardIconProvider();

        if (wheelSlicesBuilder != null)
        {
            wheelSlicesBuilder.Initialize(wheelRewardIconProvider);
            wheelSlicesBuilder.Build();

            if (wheelRotationAnimation != null)
                wheelSlicesBuilder.ApplyCounterRotation(wheelRotationAnimation.CurrentZRotation);
        }
        else
        {
            Debug.LogWarning("WheelScreenController: wheelSlicesBuilder is NULL");
        }
    }

    private void HandleSpinClicked()
    {
        wheelScreenPresenter?.PresentSpinning();

        spinWheelUseCase?.Execute(result =>
        {
            if (result == null)
                return;

            if (!result.success)
            {
                wheelScreenPresenter?.PresentBlocked(result.remainingCooldown);
                return;
            }

            wheelScreenPresenter?.PresentCompleted(result);
        });
    }

    private void HandleWheelRotationChanged(float wheelZRotation)
    {
        wheelSlicesBuilder?.ApplyCounterRotation(wheelZRotation);
    }

    private IEnumerator CooldownRefreshRoutine()
    {
        while (true)
        {
            wheelScreenPresenter?.RefreshAvailability();
            yield return new WaitForSeconds(1f);
        }
    }
}
