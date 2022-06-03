using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TicTacToe.Models;

namespace TicTacToe.Hubs
{
    public class GameHub : Hub
    {
        private static ConcurrentBag<Player> players = new ConcurrentBag<Player>();

        private static ConcurrentBag<Game> games = new ConcurrentBag<Game>();

        private static readonly Random toss = new Random();
        public override Task OnDisconnected(bool exception)
        {
            //// Lets find a game if any of which player 1 / player 2 is disconnected.
            var game = games?.FirstOrDefault(j => j.Player1.ConnectionId == Context.ConnectionId || (j.Player2 != null && j.Player2.ConnectionId == Context.ConnectionId));
            if (game == null)
            {
                //// We are here so it means we have no game whose players were disconnected. 
                //// But there may be a scenario that a player is not playing but disconnected and is still in our list.
                //// So, lets remove that player from our list.
                var playerWithoutGame = players.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                if (playerWithoutGame != null)
                {
                    //// Remove this player from our player list.
                    Remove<Player>(players, playerWithoutGame);
                }

                return null;
            }

            //// We have a game in which the player got disconnected, so lets remove that game from our list.
            if (game != null)
            {
                Remove<Game>(games, game);
            }

            //// Though we have removed the game from our list, we still need to notify the opponent that he has a walkover.
            //// If the current connection Id matches the player 1 connection Id, its him who disconnected else its player 2
            var player = game.Player1.ConnectionId == Context.ConnectionId ? game.Player1 : game.Player2;

            if (player == null)
            {
                return null;
            }

            //// Remove this player as he is disconnected and was in the game.
            Remove<Player>(players, player);

            //// Check if there was an opponent of the player. If yes, tell him, he won/ got a walk over.
            if (player.Opponent != null)
            {
                return OnOpponentDisconnected(player.Opponent.ConnectionId, player.Name);
            }

            return base.OnDisconnected(exception);
        }
        public Task OnOpponentDisconnected(string connectionId, string playerName)
        {
            return Clients.Client(connectionId).opponentDisconnected(playerName);
        }

        public void OnRegisterationComplete(string connectionId)
        {
            this.Clients.Client(connectionId).registrationComplete(games);
        }
        public void RegisterPlayer(string nameAndImageData)
        {
            var name = nameAndImageData;
            var player = players?.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                player = new Player { ConnectionId = Context.ConnectionId, Name = name, IsPlaying = false, IsSearchingOpponent = false, RegisterTime = DateTime.UtcNow };
                if (!players.Any(j => j.Name == name))
                {
                    players.Add(player);
                }
            }
            else
            {
                player.IsPlaying = false;
                player.IsSearchingOpponent = false;
            }

            this.OnRegisterationComplete(Context.ConnectionId);
        }

        public void RegisterPlayer2(string nameAndImageData, int size)
        {

        }

        public void MakeAMove(int position)
        {
            var game = games?.FirstOrDefault(x => x.Player1.ConnectionId == Context.ConnectionId || x.Player2.ConnectionId == Context.ConnectionId);

            if (game == null || game.IsOver)
            {
                return;
            }

            int symbol = 0;

            if (game.Player2.ConnectionId == Context.ConnectionId)
            {
                symbol = 1;
            }

            var player = symbol == 0 ? game.Player1 : game.Player2;

            if (player.WaitingForMove)
            {
                return;
            }

            Clients.Client(game.Player1.ConnectionId).moveMade(new MoveInformation { OpponentName = player.Name, ImagePosition = position, Image = player.Image });
            Clients.Client(game.Player2.ConnectionId).moveMade(new MoveInformation { OpponentName = player.Name, ImagePosition = position, Image = player.Image });

            if (game.Play(symbol, position))
            {
                Remove<Game>(games, game);
                Clients.Client(game.Player1.ConnectionId).gameOver($"The winner is {player.Name}");
                Clients.Client(game.Player2.ConnectionId).gameOver($"The winner is {player.Name}");
                player.IsPlaying = false;
                player.Opponent.IsPlaying = false;
                this.Clients.Client(player.ConnectionId).registrationComplete();
                this.Clients.Client(player.Opponent.ConnectionId).registrationComplete();
            }

            if (game.IsOver && game.IsDraw)
            {
                Remove<Game>(games, game);
                Clients.Client(game.Player1.ConnectionId).gameOver("Its a tame draw!!!");
                Clients.Client(game.Player2.ConnectionId).gameOver("Its a tame draw!!!");
                player.IsPlaying = false;
                player.Opponent.IsPlaying = false;
                this.Clients.Client(player.ConnectionId).registrationComplete();
                this.Clients.Client(player.Opponent.ConnectionId).registrationComplete();
            }

            if (!game.IsOver)
            {
                player.WaitingForMove = !player.WaitingForMove;
                player.Opponent.WaitingForMove = !player.Opponent.WaitingForMove;

                Clients.Client(player.Opponent.ConnectionId).waitingForOpponent(player.Opponent.Name);
                Clients.Client(player.ConnectionId).waitingForOpponent(player.Opponent.Name);
            }
        }
        public void InitializeGame(int size)
        {
            games = new ConcurrentBag<Game>();
            var player = players.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                return;
            }
            player.IsSearchingOpponent = true;
            player.Image = "/Content/O.png";
            player.Opponent = null;
            var opponent = players.Where(x => x.ConnectionId != Context.ConnectionId && !x.IsPlaying).OrderBy(x => x.RegisterTime).FirstOrDefault();
            var gtemp = new Game { Player1 = player, Player2 = opponent, size = size, GameId = DateTime.Now.Ticks.ToString() };
            gtemp.Init();
            games.Add(gtemp);
            if (opponent == null)
            {
                Clients.Client(Context.ConnectionId).opponentNotFound();
                return;
            }
            opponent.Opponent = null;
            this.OnRegisterationComplete(opponent.ConnectionId);

        }

        public void SelectGame(string id)
        {
            var player = players.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                return;
            }
            player.IsSearchingOpponent = true;
            player.IsPlaying = true;
            player.IsSearchingOpponent = false;
            player.Image = "/Content/X.png";
            var game = games.Where(x => x.GameId == id).FirstOrDefault();

            Clients.Client(Context.ConnectionId).opponentFound(game.Player1.Name, game.Player1.Image);
            Clients.Client(game.Player1.ConnectionId).opponentFound(player.Name, player.Image);

            if (toss.Next(0, 1) == 0)
            {
                player.WaitingForMove = false;
                game.Player1.WaitingForMove = true;
                Clients.Client(player.ConnectionId).waitingForMove(game.Player1.Name);
                Clients.Client(game.Player1.ConnectionId).waitingForMove(player.Name);
            }
            else
            {
                player.WaitingForMove = true;
                game.Player1.WaitingForMove = false;
                Clients.Client(game.Player1.ConnectionId).waitingForMove(player.Name);
                Clients.Client(player.ConnectionId).waitingForMove(game.Player1.Name);
            }
            game.Player1.IsPlaying = true;
            game.Player1.IsSearchingOpponent = false;
            game.Player1.Opponent = player;
            player.Opponent = game.Player1;
            game.Player2 = player;
        }

        private void Remove<T>(ConcurrentBag<T> players, T playerWithoutGame)
        {
            players = new ConcurrentBag<T>(players?.Except(new[] { playerWithoutGame }));
        }
    }
}