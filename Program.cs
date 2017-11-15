using System;
using Core.XOGameEngine;

namespace XOGameEngine
{
    class Program
    {
        static void Main(string[] args)
        {

            XOEngine e = XOEngine.Instance;
            e.OnValidMove += OnValidMove;
            e.OnBadMove += OnBadMove;
            e.OnGameStopped += OnGameStopped;
            e.OnVictory += OnVictory;

            e.NewGame(true);
            e.StopGame();
            e.NewGame(true);

            e.MakeAMove(1, TransformAction, e);
            e.MakeAMove(9, TransformAction, e);
            e.MakeAMove(7, TransformAction, e);
            e.MakeAMove(3, TransformAction, e);
            e.StopGame();
            e.MakeAMove(3, TransformAction, e);

            e.NewGame(true);
            e.MakeAMove(1, TransformAction, e);
            e.MakeAMove(9, TransformAction, e);
            e.MakeAMove(7, TransformAction, e);
            e.MakeAMove(3, TransformAction, e);
            e.MakeAMove(3, TransformAction, e);
            e.MakeAMove(4, TransformAction, e);

            Console.ReadKey();
        }

        private static void OnVictory(object sender, VictoryEventArgs e)
        {
            System.Console.WriteLine($"The winner is: { e.Winner }!");
        }

        private static void OnGameStopped(object sender, GameStoppedEventArgs e)
        {
            System.Console.WriteLine("Game Stopped!");
        }

        private static void OnBadMove(object sender, BadMoveEventArgs e)
        {
            System.Console.WriteLine($"Invalid move: { e.Move }");
        }

        public static void TransformAction(object o, Player p)
        {
            Console.WriteLine(p);
        }

        private static void OnValidMove(object sender, ValidMoveEventArgs e)
        {
            Console.WriteLine($"Valid move: {e.Move}");
        }
    }
}
