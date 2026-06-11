namespace UI
{
    public static class InputBlocker
    {
        private static int _blockCount;

        public static bool IsBlocked =>
            _blockCount > 0;

        public static void PushBlock()
        {
            _blockCount++;
        }

        public static void PopBlock()
        {
            _blockCount--;

            if (_blockCount < 0)
                _blockCount = 0;
        }
    }
}