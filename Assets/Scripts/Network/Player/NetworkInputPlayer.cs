using Fusion;
using UnityEngine;

namespace Network
{
    public struct NetworkInputPlayer : INetworkInput
    {   
        public const byte JUMP_BUTTON = 1;
        public const byte SPRINT_BUTTON = 2;
        public const byte INTERACT_BUTTON = 3;
        
        public const byte SKILL1_BUTTON = 4;
        public const byte SKILL2_BUTTON = 5;
        public const byte ATTACK_BUTTON = 6;
        
        public NetworkButtons Buttons;
        public Vector3 MoveDirection;
        public Vector2 MouseRotation;
    }
}
