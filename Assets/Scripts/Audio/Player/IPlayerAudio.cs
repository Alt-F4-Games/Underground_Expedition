namespace Audio.Player
{
    public interface IPlayerAudio
    {
        void PlayFootstep();
        
        void PlayMissAttack();
        void PlayAttack();

        void PlayEmpoweredAttack();
        void PlayAttackAOE();
        
        void PlayDamaged();
        void PlayDeath();
        
        void PlayLand();
        
        void PlayInteract();
        void PlayUseItem();
    }
}