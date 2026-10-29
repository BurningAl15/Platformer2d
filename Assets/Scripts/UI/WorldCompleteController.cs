using TMPro;
using UnityEngine;

/// <summary>
/// Resumen de estrellas, gemas, tréboles y mejores tiempos de un mundo.
/// El panel y sus referencias visuales se asignan desde el Inspector.
/// </summary>
public class WorldCompleteController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Resumen")]
    [SerializeField] private TextMeshProUGUI worldNameText;
    [SerializeField] private TextMeshProUGUI starsText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private TextMeshProUGUI trebolsText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Siguiente mundo")]
    [SerializeField] private WorldData nextWorldData;

    private bool isShowing;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(WorldData completedWorld)
    {
        if (completedWorld == null || WorldProgressManager.Instance == null)
            return;

        var progress = WorldProgressManager.Instance;
        SetText(worldNameText, completedWorld.worldName);
        SetText(starsText, $"{progress.GetTotalStarsInWorld(completedWorld)} / {completedWorld.MaxStars}");
        SetText(gemsText, $"{progress.GetGemsCollectedInWorld(completedWorld)} / {completedWorld.totalGems}");
        SetText(trebolsText, $"{progress.GetTrebolsInWorld(completedWorld)} / {completedWorld.totalTrebols}");
        SetText(timeText, FormatTime(progress.GetBestTimesTotalInWorld(completedWorld)));

        isShowing = true;
        if (panel != null)
            panel.SetActive(true);
    }

    /// <summary>Conectar al botón principal del panel desde el Inspector.</summary>
    public void ContinueToNextWorld()
    {
        if (!isShowing) return;
        isShowing = false;

        if (nextWorldData != null && !string.IsNullOrEmpty(nextWorldData.selectionSceneName))
            SceneUtils.LoadScene(nextWorldData.selectionSceneName);
        else
            SceneUtils.ToSelectionScene();
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target != null)
            target.text = value;
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int remainingSeconds = totalSeconds % 60;
        return hours > 0
            ? $"{hours:00}:{minutes:00}:{remainingSeconds:00}"
            : $"{minutes:00}:{remainingSeconds:00}";
    }
}
