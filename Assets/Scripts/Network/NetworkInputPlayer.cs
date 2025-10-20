using Fusion;
using UnityEngine;

namespace Network
{
    public struct NetworkInputPlayer : INetworkInput
    {   
        public const byte JUMP_BUTTON = 1;
        public NetworkButtons Buttons;
        public Vector3 MoveDirection;
    }
}
