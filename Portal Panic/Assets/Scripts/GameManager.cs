using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Settings Panel")]
    [Tooltip("Reference to the settings panel GameObject")]
    public GameObject settingsPanel;

    /// <summary>
    /// Initialize the GameManager instance
    /// </summary>
    /*void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }*/

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        
    }

    /// <summary>
    /// Opens a panel
    /// </summary>
    /// <param name="panel"> GameObject to open </param>
    public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
    }

    /// <summary>
    /// Closes a panel
    /// </summary>
    /// <param name="panel"> GameObject to close </param>
    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

	public void LoadScene(string sceneName)
    {
        Debug.Log("LoadScene called with: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Toggle the settings panel (can be called from UI buttons or other scripts)
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    /// <summary>
    /// Open the settings panel
    /// </summary>
    /*public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }*/

    /// <summary>
    /// Close the settings panel
    /// </summary>
    /*public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }*/

    public void DebugLog(string message)
    {
        Debug.Log(message);
    }
}
