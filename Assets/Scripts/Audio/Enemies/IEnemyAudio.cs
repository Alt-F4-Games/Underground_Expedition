namespace Audio.Enemies
{
    public interface IEnemyAudio
    {
        void PlayDeath();
        void PlayDamage();
        void PlayExplosion();
        void PlayAttack();
    }
}