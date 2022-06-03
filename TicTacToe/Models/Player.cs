namespace TicTacToe.Models
{
    using System;

    /// <summary>
    ///  The player class. Each player of Tic-Tac-Toe game would be an instance of this class.
    /// </summary>
    internal class Player
    {
        public string Name { get; set; }
        public Player Opponent { get; set; }
        public bool IsPlaying { get; set; }
        public bool WaitingForMove { get; set; }
        public bool IsSearchingOpponent { get; set; }
        public DateTime RegisterTime { get; set; }
        public string Image { get; set; }
        public string ConnectionId { get; set; }
    }
}