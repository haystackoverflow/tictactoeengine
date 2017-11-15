using System;

namespace Core.XOGameEngine
{
    /// <summary>
    /// Izvedena klasa klase EventArgs.
    /// Sadrzi informacije o podignutom dogadjaju.
    /// </summary>
    public class VictoryEventArgs : EventArgs
    {
        /// <summary>
        /// Igrac koji je pobjedio.
        /// </summary>
        public Player Winner { get; private set; }
        public VictoryEventArgs(Player player)
        {
            Winner = player;
        }
    }
}
