using System;

namespace Core.XOGameEngine
{
    /// <summary>
    /// Izvedena klasa klase EventArgs.
    /// Sadrzi informacije o podignutom dogadjaju.
    /// </summary>
    public class ValidMoveEventArgs : EventArgs
    {
        /// <summary>
        /// Polje na tabli koje je uspjesno popunjeno.
        /// Vrijednost ce biti [1,9] ukoliko je potez uspjesno odigran.
        /// </summary>
        public int Move { get; private set; }

        public ValidMoveEventArgs(int polje)
        {
            Move = polje;
        }
    }
}
