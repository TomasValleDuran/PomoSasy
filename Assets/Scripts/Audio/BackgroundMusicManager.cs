using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio
{
    public class BackgroundMusicManager : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const float MenuVolume = 0.18f;
        private const float GameplayVolume = 0.14f;

        private static BackgroundMusicManager _instance;

        private AudioSource _audioSource;
        private AudioClip _mainMenuClip;
        private AudioClip _gameplayClip;
        private string _currentMusicKey;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (_instance != null)
                return;

            GameObject musicManager = new GameObject(nameof(BackgroundMusicManager));
            _instance = musicManager.AddComponent<BackgroundMusicManager>();
            DontDestroyOnLoad(musicManager);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;

            _mainMenuClip = CreateMusicClip("Main Menu Music", 12f, new[] { 261.63f, 329.63f, 392f, 493.88f }, true);
            _gameplayClip = CreateMusicClip("Gameplay Music", 8f, new[] { 110f, 146.83f, 164.81f, 196f }, false);

            SceneManager.sceneLoaded += HandleSceneLoaded;
            PlayForScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (_instance == this)
                SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayForScene(scene);
        }

        private void PlayForScene(Scene scene)
        {
            string sceneName = scene.name;
            if (sceneName == "MainMenu")
            {
                PlayMusic("main-menu", _mainMenuClip, MenuVolume);
                return;
            }

            if (sceneName.StartsWith("Gameplay"))
            {
                PlayMusic("gameplay", _gameplayClip, GameplayVolume);
                return;
            }

            _currentMusicKey = null;
            _audioSource.Stop();
        }

        private void PlayMusic(string key, AudioClip clip, float volume)
        {
            if (_currentMusicKey == key && _audioSource.isPlaying)
                return;

            _currentMusicKey = key;
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.Play();
        }

        private static AudioClip CreateMusicClip(string name, float durationSeconds, float[] scale, bool mellow)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * durationSeconds);
            float[] samples = new float[sampleCount];
            float beatLength = mellow ? 0.75f : 0.375f;

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                float loopFade = Mathf.Min(1f, Mathf.Min(time, durationSeconds - time) / 0.05f);
                int beat = Mathf.FloorToInt(time / beatLength);

                float root = scale[(beat / 4) % scale.Length];
                float arp = scale[beat % scale.Length] * (mellow ? 2f : 3f);
                float bass = root * (mellow ? 0.5f : 1f);
                float beatProgress = (time % beatLength) / beatLength;
                float pluckEnvelope = Mathf.Exp(-beatProgress * (mellow ? 5f : 8f));

                float pad = Mathf.Sin(2f * Mathf.PI * root * time) * 0.12f;
                pad += Mathf.Sin(2f * Mathf.PI * root * 1.5f * time) * 0.06f;

                float melody = Mathf.Sin(2f * Mathf.PI * arp * time) * pluckEnvelope * (mellow ? 0.11f : 0.16f);
                float bassLine = Mathf.Sin(2f * Mathf.PI * bass * time) * (mellow ? 0.08f : 0.18f);

                samples[i] = (pad + melody + bassLine) * loopFade;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
