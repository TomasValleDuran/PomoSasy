using UnityEngine;

namespace Audio
{
    public static class CoinPickupNotePlayer
    {
        private const int SampleRate = 44100;
        private const float Duration = 0.18f;
        private const float Volume = 0.35f;

        private static readonly float[] Frequencies =
        {
            523.25f, // C5
            587.33f, // D5
            659.25f, // E5
            783.99f, // G5
            880.00f, // A5
            1046.50f // C6
        };

        private static AudioClip[] _clips;

        public static void PlayRandom(Vector3 position)
        {
            EnsureClips();

            if (_clips == null || _clips.Length == 0)
                return;

            AudioClip clip = _clips[Random.Range(0, _clips.Length)];
            AudioSource.PlayClipAtPoint(clip, position, Volume);
        }

        private static void EnsureClips()
        {
            if (_clips != null)
                return;

            _clips = new AudioClip[Frequencies.Length];
            for (int i = 0; i < Frequencies.Length; i++)
                _clips[i] = CreateCoinNoteClip($"Coin Note {i + 1}", Frequencies[i]);
        }

        private static AudioClip CreateCoinNoteClip(string name, float frequency)
        {
            int sampleCount = Mathf.CeilToInt(SampleRate * Duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                float progress = time / Duration;
                float attack = Mathf.Clamp01(time / 0.01f);
                float release = Mathf.Clamp01((Duration - time) / 0.12f);
                float envelope = attack * release * release;

                float fundamental = Mathf.Sin(2f * Mathf.PI * frequency * time);
                float sparkle = Mathf.Sin(2f * Mathf.PI * frequency * 2f * time) * 0.35f;
                float chime = Mathf.Sin(2f * Mathf.PI * frequency * 3f * time) * 0.15f;

                samples[i] = (fundamental + sparkle + chime) * envelope * (1f - progress * 0.25f);
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
