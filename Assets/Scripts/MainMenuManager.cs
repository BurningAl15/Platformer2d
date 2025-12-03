using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
   public GameObject continueButton;

   [SerializeField] private Image textImg;
   [SerializeField] private CanvasGroup panelBG;
   [SerializeField] private GameObject panelOptions;

   [SerializeField] private float initPoint, endPoint;
   [SerializeField] private AnimationCurve initCurve;
   
   [Header("First Selected Buttons")]
   [Tooltip("Primer botón a seleccionar cuando el jugador YA jugó (Continue visible)")]
   [SerializeField] private GameObject continueFirstButton;
   
   [Tooltip("Primer botón a seleccionar cuando el jugador NUNCA jugó (Continue oculto)")]
   [SerializeField] private GameObject newGameFirstButton;
   
   [Tooltip("Primer botón del panel de confirmación")]
   [SerializeField] private GameObject panelFirstButton;
   
   private bool hasPlayedBefore = false;
   
   private void Awake()
   {
      DOTween.Init(true, true, LogBehaviour.Default);
   }

   private void Start()
   {
      hasPlayedBefore = PlayerPrefs.HasKey("Level_1");
      
      if(hasPlayedBefore)
      {
         continueButton.SetActive(true);
         SelectButton(continueFirstButton);
      }
      else
      {
         continueButton.SetActive(false);
         SelectButton(newGameFirstButton);
      }

      panelBG.alpha = 0;
      panelBG.interactable = false;
      panelBG.blocksRaycasts = false;
   }

   private void SelectButton(GameObject button)
   {
      if (button != null && EventSystem.current != null)
      {
         EventSystem.current.SetSelectedGameObject(null);
         EventSystem.current.SetSelectedGameObject(button);
         Debug.Log($"Selected button: {button.name}");
      }
      else
      {
         Debug.LogWarning("Cannot select button: button is null or EventSystem is missing");
      }
   }

   public void DeactivatePanel()
   {
      panelOptions.transform.DOLocalMoveY(initPoint, .5f).SetEase(initCurve).OnComplete(()=>TurnOffPanel());
   }

   void TurnOffPanel()
   {
      panelBG.alpha = 0;
      panelBG.interactable = false;
      panelBG.blocksRaycasts = false;
      
      if (hasPlayedBefore)
      {
         SelectButton(continueFirstButton);
      }
      else
      {
         SelectButton(newGameFirstButton);
      }
   }
   
   public void ActivatePanel()
   {
      if (hasPlayedBefore)
      {
         panelBG.alpha = 1;
         panelBG.interactable = true;
         panelBG.blocksRaycasts = true;
         panelOptions.transform.DOLocalMoveY(endPoint, .5f).SetEase(initCurve).OnComplete(() => SelectButton(panelFirstButton));
      }
      else
      {
         ToNextScene();
      }
   }
   
   public void ToNextScene()
   {
      PlayerPrefs.DeleteAll();
      for (int i = 0; i < 10; i++)
      {
         int j = i + 1;
         PlayerPrefs.SetInt(StringUtils.Get_Level(j), 0);
         PlayerPrefs.SetInt(StringUtils.Get_GemsInLevel(j), 0);
         PlayerPrefs.SetFloat(StringUtils.Get_TimeInLevel(j), 99999);
      }

      PlayerPrefs.Save();
      SceneUtils.LoadGameplayScene(0);
   }

   public void ContinueScene()
   {
       SceneUtils.ToSelectionScene();
   }

   public void QuitGame()
   {
      Application.Quit();
   }
}
