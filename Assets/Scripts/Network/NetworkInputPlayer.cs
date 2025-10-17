using Fusion;
using UnityEngine;

namespace Network
{
    public struct NetworkInputPlayer : INetworkInput
    {   
        public const byte JumpButton = 1;
        public byte Buttons;
        public Vector2 Direction;
    }
}
