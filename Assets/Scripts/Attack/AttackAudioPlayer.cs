using UnityEngine;

namespace Attack
{
    public static class AttackAudioPlayer
    {
        public static void Play(AudioSource source, AttackData attackData)
        {
            if (source == null || attackData?.AttackSfx == null)
                return;

            source.PlayOneShot(attackData.AttackSfx);
        }
    }
}
