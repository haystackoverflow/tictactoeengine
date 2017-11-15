using System;

namespace Core.XOGameEngine
{
    /// <summary>
    /// Izvedena klasa klase EventArgs.
    /// Sadrzi informacije o podignutom dogadjaju.
    /// </summary>
    public class BadMoveEventArgs : EventArgs
    {
        /// <summary>
        /// Potez koji nije validan (moguc).
        /// </summary>
        public int Move { get; private set; }

        public BadMoveEventArgs(int move)
        {
            Move = move;
        }
    }
}
