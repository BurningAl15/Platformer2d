using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu _instance;
    
    [SerializeField] private CanvasGroup pauseBG;
    public GameObject pauseScreen;
    public bool isPaused;

    [SerializeField] private float initPoint, endPoint;
    [SerializeField] private GameObject firstSelectedButton;
    
    private void Awake()
    {
        _instance = this;
        DOTween.Init(true, true, LogBehaviour.Default);
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed += Pause;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePressed -= Pause;
        }
    }

    private void Start()
    {
        TurnCanvasGroup(false);
    }

    public void Pause()
    {
        isPaused = !isPaused;
        print("Is Paused State: " + isPaused);

        if (isPaused)
        {
            TurnCanvasGroup(true);
            InputManager.Instance.EnableUIOnlyInput();
            
            if (firstSelectedButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
                print("Selected button: " + firstSelectedButton.name);
            }
        }
        else
        {
            Time.timeScale = 1;
            InputManager.Instance.EnablePlayerInput();
            
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        float tempEndValue = isPaused ? endPoint : initPoint;
        print(tempEndValue);
        pauseScreen.transform.DOLocalMoveY(tempEndValue, .25f).SetEase(Ease.InOutBounce).SetUpdate(true).OnComplete(TurnOffPause);
    }

    public void PauseAuxiliar()
    {
        print("Calling auxiliar");
        isPaused = false;
        Time.timeScale = 1;
        InputManager.Instance.EnablePlayerInput();
        
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        pauseScreen.transform.DOLocalMoveY(initPoint, .25f).SetEase(Ease.InOutBounce).SetUpdate(true).OnComplete(TurnOffPause);
    }

    void TurnOffPause()
    {
        Time.timeScale = isPaused ? 0 : 1;
        if (!isPaused)
            TurnCanvasGroup(false);
    }

    public void LevelSelection()
    {
        SceneUtils.ToSelectionScene();
        Time.timeScale = 1;
        InputManager.Instance.EnablePlayerInput();
    }

    public void MainMenu()
    {
        SceneUtils.ToMainScene();
        Time.timeScale = 1;
        InputManager.Instance.EnablePlayerInput();
    }

    void TurnCanvasGroup(bool _)
    {
        pauseBG.interactable = _;
        pauseBG.blocksRaycasts = _;
        pauseBG.alpha = _ ? 1 : 0;
    }
}
