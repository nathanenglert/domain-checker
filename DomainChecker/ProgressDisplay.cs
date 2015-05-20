using System;

namespace DomainChecker
{
    class ProgressDisplay
    {
        private static int _ticks = 0;

        public static void Update(int progress, int total)
        {
            Console.CursorLeft = 0;
            Console.Write("Loading{0}{1}", new String('.', _ticks), new String(' ', 3 - _ticks));
            Console.CursorLeft = 11;
            Console.Write(progress + " of " + total + "    ");

            _ticks = _ticks == 3 ? 0 : _ticks + 1;
        }
    }
}
