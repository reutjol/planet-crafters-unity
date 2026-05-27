using System.Collections;
using UnityEngine;

public class WheelScreenController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private WheelConfig wheelConfig;

    [Header("Views")]
    [SerializeField] private SpinButtonView spinButtonView;
    [SerializeField] private CooldownTextView cooldownTextView;

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

        cooldownRefreshCoroutine = StartCoroutine(CooldownRefreshRoutine());
    }

    private void OnDisable()
    {
        if (spinButtonView != null)
            spinButtonView.Clicked -= HandleSpinClicked;

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
        wheelRewardGrantService = WheelServicesFactory.CreateRewardGrantService();
        wheelAngleCalculator = WheelServicesFactory.CreateAngleCalculator();

        wheelScreenPresenter = new WheelScreenPresenter(
            spinButtonView,
            cooldownTextView,
            spinAvailabilityService,
            localizationService
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

        Debug.Log("WheelScreenController: trying to build slices");

        if (wheelSlicesBuilder != null)
        {
            Debug.Log("WheelScreenController: wheelSlicesBuilder found");
            wheelSlicesBuilder.Initialize(wheelRewardIconProvider);
            wheelSlicesBuilder.Build();
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

    private IEnumerator CooldownRefreshRoutine()
    {
        while (true)
        {
            wheelScreenPresenter?.RefreshAvailability();
            yield return new WaitForSeconds(1f);
        }
    }
}
