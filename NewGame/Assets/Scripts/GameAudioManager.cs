using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }
    
    public AudioSource musicSource;
    public Slider volumeSlider;
    private const string VOLUME_KEY = "GameVolume";

    [Header("Музыка для разных сцен")]
    public AudioClip[] sceneMusic;  
    public int[] scenesWithoutMusic;  // Сцены, где музыка не нужна
    

    private void Awake()
    {
        // Синглтон паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
            
        musicSource.loop = true;
        LoadVolume();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = musicSource.volume;
            volumeSlider.onValueChanged.AddListener(HandleVolumeChange);
        }
    }

    public void HandleVolumeChange(float newVolume)
    {
        if (musicSource != null)
        {
            musicSource.volume = newVolume;
            SaveVolume();
        }
    }

    private void LoadVolume()
    {
        if (musicSource != null)
        {
            float savedVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
            musicSource.volume = savedVolume;
        }
    }

    private void SaveVolume()
    {
        if (musicSource != null)
        {
            PlayerPrefs.SetFloat(VOLUME_KEY, musicSource.volume);
            PlayerPrefs.Save();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Проверяем, нужно ли отключить музыку для этой сцены
        if (scenesWithoutMusic != null)
        {
            foreach (int sceneIndex in scenesWithoutMusic)
            {
                if (scene.buildIndex == sceneIndex)
                {
                    musicSource.Stop();
                    return;
                }
            }
        }

        if (sceneMusic != null && scene.buildIndex < sceneMusic.Length && sceneMusic[scene.buildIndex] != null)
        {
            ChangeMusicForScene(scene.buildIndex);
        }
    }

    public void ChangeMusicForScene(int sceneIndex)
    {
        if (musicSource != null && sceneMusic != null && sceneIndex < sceneMusic.Length && sceneMusic[sceneIndex] != null)
        {
            musicSource.clip = sceneMusic[sceneIndex];
            musicSource.Play();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
