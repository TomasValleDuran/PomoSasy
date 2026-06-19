using UnityEngine;

namespace UI
{
    public class UIAudioPlayer
    {
        private static AudioClip _buttonClickClip;
        private static float _buttonClickVolume = 1f;
        private static AudioSource _sharedSource;

        public static void ConfigureButtonClick(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
                return;

            _buttonClickClip = clip;
            _buttonClickVolume = volumeScale;
        }

        public static void PlayButtonClick()
        {
            if (_buttonClickClip == null)
                return;

            EnsureSharedSource();
            _sharedSource.PlayOneShot(_buttonClickClip, _buttonClickVolume);
        }

        public static void Play(AudioSource source, AudioClip clip, float volumeScale = 1f)
        {
            if (source == null || clip == null)
                return;

            source.PlayOneShot(clip, volumeScale);
        }

        private static void EnsureSharedSource()
        {
            if (_sharedSource != null)
                return;

            GameObject audioObject = new GameObject("UIAudioPlayer");
            Object.DontDestroyOnLoad(audioObject);
            _sharedSource = audioObject.AddComponent<AudioSource>();
            _sharedSource.playOnAwake = false;
            _sharedSource.spatialBlend = 0f;
        }
    }
}
