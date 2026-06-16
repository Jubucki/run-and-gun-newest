using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Leaderboard UI")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private TextMeshProUGUI leaderboardText;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        Debug.Log("MainMenu Script aktiv!");
    }
   
    public void OnStartClicked()
    {
        Debug.Log("Start clicked!");
        SceneManager.LoadScene("Prototype Map");
    }
        
    public void OnLeaderboardClicked()
    {
        Debug.Log("Leaderboard clicked!");
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            DisplayLeaderboard();
        }
    }

    public void OnCloseLeaderboardClicked()
    {
        Debug.Log("Close Leaderboard clicked!");
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }

    private void DisplayLeaderboard()
    {
        if (leaderboardText == null) return;

        LeaderboardData data = LeaderboardManager.LoadLeaderboard();
        if (data.entries == null || data.entries.Count == 0)
        {
            leaderboardText.text = "No high scores yet!\nPlay the game to set one.";
            return;
        }

        string displayText = "";
        for (int i = 0; i < data.entries.Count; i++)
        {
            var entry = data.entries[i];
            displayText += $"{i + 1}.  {entry.score} Points   ({entry.date})\n";
        }
        leaderboardText.text = displayText;
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}