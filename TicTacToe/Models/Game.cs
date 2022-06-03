using System.Collections.Generic;

namespace TicTacToe.Models
{
    internal class Game
    {
        public string GameId { get; set; }
        public bool IsOver { get; private set; }

        public bool IsDraw { get; private set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public int size { get; set; }

        private int[] field;

        private int movesLeft;

        public Game()
        {

        }

        public void Init()
        {
            movesLeft = size * size;
            var fieldTemp = new List<int>();
            for (var i = 0; i < movesLeft; i++)
            {
                fieldTemp.Add(-1);
            }
            field = fieldTemp.ToArray();
        }

        public bool Play(int player, int position)
        {
            if (this.IsOver)
            {
                return false;
            }

            this.PlacePlayerNumber(player, position);
            return this.CheckWinner();
        }
        private bool CheckWinner()
        {
            var indexInner = 1;
            var firstVal = field[indexInner - 1];
            var index = 1;
            var match = true;
            if (firstVal != -1)
            {
                for (int i = 1; i <= size; i++)
                {
                    if (i != 1)
                    {
                        indexInner += size;
                    }
                    firstVal = field[indexInner - 1];
                    if (firstVal != -1)
                    {
                        match = true;
                        for (var j = indexInner; j < (size + indexInner); j++)
                        {
                            if (firstVal != field[j - 1])
                            {
                                match = false;
                            }
                        }
                        if (match)
                        {
                            this.IsOver = true;
                            return true;
                        }
                    }
                }
            }
            for (int i = 1; i <= size; i++)
            {
                index = i;
                firstVal = field[index - 1];
                if (firstVal != -1)
                {
                    match = true;
                    for (int j = 1; j <= size; j++)
                    {
                        if (j != 1)
                        {
                            index += size;
                        }
                        if (firstVal != field[index - 1])
                        {
                            match = false;
                        }
                    }
                    if (match)
                    {
                        this.IsOver = true;
                        return true;
                    }
                }
            }

            index = 1;
            firstVal = field[index - 1];
            if (firstVal != -1)
            {
                match = true;
                for (int i = 1; i <= size; i++)
                {
                    if (i != 1)
                    {
                        index += (size + 1);
                    }
                    if (firstVal != field[index - 1])
                    {
                        match = false;
                    }
                }
                if (match)
                {
                    this.IsOver = true;
                    return true;
                }
            }
            index = 3;
            firstVal = field[index - 1];
            if (firstVal != -1)
            {
                match = true;
                for (int i = 1; i <= size; i++)
                {
                    if (i != 1)
                    {
                        index += (size - 1);
                    }
                    if (firstVal != field[index - 1])
                    {
                        match = false;
                    }
                }
                if (match)
                {
                    this.IsOver = true;
                    return true;
                }
            }
            return false;
        }


        private void PlacePlayerNumber(int player, int position)
        {
            this.movesLeft -= 1;

            if (this.movesLeft <= 0)
            {
                this.IsOver = true;
                this.IsDraw = true;
            }

            if (position < field.Length && field[position] == -1)
            {
                field[position] = player;
            }
        }
    }
}