using System;

namespace Core.XOGameEngine
{
    /// <summary>
    /// Izvedena klasa klase EventArgs.
    /// Sadrzi informacije o podignutom dogadjaju.
    /// </summary>
    public class GameStoppedEventArgs : EventArgs
    {
        /// <summary>
        /// Logicka promjenljiva koja indicira da li je igra zaustavljena.
        /// </summary>
        public bool IsStopped { get; private set; }
        public GameStoppedEventArgs(bool isStopped)
        {
            IsStopped = isStopped;
        }
    }
}
