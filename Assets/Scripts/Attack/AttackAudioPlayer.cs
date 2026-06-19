using UnityEngine;

namespace Attack
{
    public static class AttackAudioPlayer
    {
        public static void Play(AudioSource source, AttackData attackData, float volumeScale = 0.8f)
        {
            if (source == null || attackData?.AttackSfx == null)
                return;

            source.PlayOneShot(attackData.AttackSfx, volumeScale);
        }

        public static void PlayAtPoint(Vector3 position, AudioClip clip)
        {
            if (clip == null)
                return;

            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}
