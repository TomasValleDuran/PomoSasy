namespace Attack
{
    /// <summary>
    /// Implemented by an equipped attack's visual to be notified each time that attack actually fires
    /// (deals damage), so it can react in sync with the hit. <see cref="AttackSlot"/> calls this on the
    /// visual instance's components when the slot's attack lands.
    /// </summary>
    public interface IAttackPulse
    {
        void Pulse();
    }
}
