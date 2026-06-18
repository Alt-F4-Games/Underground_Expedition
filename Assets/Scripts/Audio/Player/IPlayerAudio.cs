namespace Audio.Player
{
    public interface IPlayerAudio
    {
        void PlayFootstep();
        void PlayAttack();
        void PlayDamage();
        void PlayDeath();
        void PlayJump();
        void PlayLand();
        void PlayInteract();
        void PlayUseItem();
        void PlaySkill1();
        void PlaySkill2();
    }
}