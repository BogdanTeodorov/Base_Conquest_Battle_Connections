using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro; // Ensure TextMesh Pro is imported



public class GameController : MonoBehaviour
{

    [Header("Music Tracks")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    public AudioClip battleMusic; // New battle effects music
    public AudioClip winningMusic;
    public AudioClip losingMusic;



    private AudioSource musicSource;
    private AudioSource battleMusicSource; // New AudioSource for battle effects music


    public static GameController Instance { get; private set; }

    [SerializeField] private TMP_Text levelText;



    public GameObject[] levelPrefabs; // Array of level prefabs
    private GameObject currentLevel; // Currently loaded level
    private int currentLevelIndex; // Index of the current level

    private Dictionary<string, List<Tower>> towersByTag = new Dictionary<string, List<Tower>>();

    // Flag to track the pause state
    private bool isGamePaused = false;
    [SerializeField] private GameObject Menuimage;

    [Header("Music Volume")]
    [Range(0, 1)] // Range attribute to make it a slider in the inspector
    public float musicVolume = 0.5f; // Default volume at max



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize the main music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false; // Set to loop the music

        // Initialize the battle music source
        battleMusicSource = gameObject.AddComponent<AudioSource>();
        battleMusicSource.loop = true; // Battle music can loop

        // Set initial music volume
        musicSource.volume = musicVolume;
        battleMusicSource.volume = musicVolume; // Use the same volume for consistency
    }

    void Start()
    {
        LoadCurrentLevel();
        PauseGame();
        // ... existing Start code ...
        PlayMusic(menuMusic, true); // Play menu music initially with looping
    }


    // Method to pause the game
    public void PauseGame()
    {
        Time.timeScale = 0;
        isGamePaused = true;
        // Call your UI method to show pause menu or pause screen
        Menuimage.SetActive(true);
        PlayMusic(menuMusic, true); // Play menu music with looping
        PlayBattleMusic(false);

    }

    // Method to resume the game
    public void ResumeGame()
    {
        levelText.text = "Level " + (currentLevelIndex + 1).ToString();

        Time.timeScale = 1;
        isGamePaused = false;
        Menuimage.SetActive(false);
        PlayMusic(gameMusic, true); // Resume playing game music with looping
        PlayBattleMusic(true);


    }

    public void RegisterTower(Tower tower)
    {
        string tag = tower.gameObject.tag;
        if (!towersByTag.ContainsKey(tag))
        {
            towersByTag[tag] = new List<Tower>();
        }
        towersByTag[tag].Add(tower);
    }

    public Tower GetRandomEnemyTower(string playerTag)
    {
        List<Tower> potentialTargets = towersByTag
            .Where(kvp => kvp.Key != playerTag)
            .SelectMany(kvp => kvp.Value)
            .ToList();

        if (potentialTargets.Count > 0)
        {
            return potentialTargets[Random.Range(0, potentialTargets.Count)];
        }

        return null;
    }

    public void UpdateTowerCount(string oldTag, string newTag)
    {
        // Remove all null or tag-mismatched towers from the oldTag list
        if (towersByTag.ContainsKey(oldTag))
        {
            towersByTag[oldTag].RemoveAll(tower => tower == null || tower.gameObject.tag != oldTag);
        }

        // Ensure the newTag list exists in the dictionary
        if (!towersByTag.ContainsKey(newTag))
        {
            towersByTag[newTag] = new List<Tower>();
        }
        else
        {
            // Clean the newTag list before adding to it
            towersByTag[newTag].RemoveAll(tower => tower == null || tower.gameObject.tag != newTag);
        }

        // Add all current towers with the newTag to the list
        towersByTag[newTag].AddRange(FindObjectsOfType<Tower>().Where(tower => tower != null && tower.gameObject.tag == newTag));

        // Check for win/lose conditions
        CheckWinLoseConditions();
    }


    public void ResetGame()
    {

        // Reset the current level index to 0 (first level)
        currentLevelIndex = 0;
        PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex); // Save the reset level index


        // Reset other game states if necessary
        RestartLevel();

        // // Optionally, play the game music for the first level and resume the game
        // PlayMusic(gameMusic, true);
        // ResumeGame();
    }


    private void CheckWinLoseConditions()
    {
        if (!towersByTag.ContainsKey("Player") || towersByTag["Player"].Count == 0)
        {
            Debug.Log("Player lost!");
            levelText.text = "You Lose!";
            PlayMusic(losingMusic, false); // Play losing music without looping
            Invoke(nameof(ShowMenuAfterDelay), 1.5f); // Show menu after music ends
            RestartLevel();

        }
        else if (towersByTag.Where(kvp => kvp.Key != "Player").All(kvp => kvp.Value.Count == 0))
        {
            Debug.Log("Player won!");
            levelText.text = "You Won!";
            PlayMusic(winningMusic, false); // Play winning music without looping
            Invoke(nameof(ShowMenuAfterDelay), 1.5f); // Show menu after music ends
            NextLevel();
        }
    }

    private void ShowMenuAfterDelay()
    {
        PauseGame(); // This will show the menu as per your existing logic
    }

    // Method to play a specific music track
    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    // Method to play battle effects music
    public void PlayBattleMusic(bool play)
    {
        if (play)
        {
            if (battleMusic != null)
            {
                battleMusicSource.clip = battleMusic;
                battleMusicSource.Play();
            }
        }
        else
        {
            battleMusicSource.Stop();
        }
    }


    // Method to update the music volume
    public void UpdateMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = musicVolume;
    }

    public void RestartGame()
    {


        RestartLevel(); // Restart the current level instead of reloading the scene
    }


    // Method to quit the game
    public void QuitGame()
    {
        // Log a message to the console (useful for debugging in the Unity Editor)
        Debug.Log("Quitting game...");

        // Quit the application
        Application.Quit();

        // If running in the Unity Editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    private void LoadCurrentLevel()
    {
        currentLevelIndex = PlayerPrefs.GetInt("CurrentLevel", 0); // Load the current level index

        // Check if the current level index is within the range of available levels
        if (currentLevelIndex < levelPrefabs.Length)
        {
            levelText.text = "Level " + (currentLevelIndex + 1).ToString();
            currentLevel = Instantiate(levelPrefabs[currentLevelIndex]);
        }
        else
        {
            // If the level index is out of range, reset to the first level
            Debug.Log("Level index out of range. Resetting to first level.");
            currentLevelIndex = 0;
            PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex); // Save the reset level index
            LoadCurrentLevel(); // Recursively call to load the first level
        }
    }


    public void NextLevel()
    {
        Destroy(currentLevel); // Clean up the current level
        currentLevelIndex++;

        // Check if the current level index exceeds the number of levels available
        if (currentLevelIndex >= levelPrefabs.Length)
        {
            // If it's the last level, reset to the first level
            currentLevelIndex = 0;
        }

        PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex); // Save the new level index
        LoadCurrentLevel(); // Load the next or first level
    }



    public void RestartLevel()
    {
        Destroy(currentLevel); // Clean up the current level
        LoadCurrentLevel(); // Reload the current level
    }


}
