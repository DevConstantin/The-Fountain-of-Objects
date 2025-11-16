

using System.Data;
using System.Drawing;
using System.Windows.Input;

Game Game = new(4, 4, new Player(), new FountainOfObjects(new Location(0, 2)));

Amarok amarok1 = new Amarok(new Location(0, 1));
Amarok amarok2 = new Amarok(new Location(3, 3));

Game.AddMonster(amarok1);
Game.AddMonster(amarok2);

public class Game
{
    public Player Player { get; init; }
    public FountainOfObjects Fountain { get; init; }
    public Map Map { get; }
    public IMonster[,] Monsters; // Stores the monster type within each room
    public bool GameWon { get; private set; } = false;

    public Game(int rows, int columns, Player player, FountainOfObjects fountain)
    {
        Map = new Map(rows, columns);
        Monsters = new IMonster[rows, columns];
        Player = player;
        Fountain = fountain;
    }

    public void Run()
    {
        while (!GameWon && Player.IsAlive)
        {
            // Log info

            // Request & accept movement input 
            ConsoleHelper.WriteLine("What do you want to do?", ConsoleColor.White);
            int currentRow = Player.Location.Row;
            int currentColumn = Player.Location.Column;
            string choice = Console.ReadLine();
            Location newLocation = choice switch
            {
                "move north" => new Location(currentRow++, currentColumn),
                "move south" => new Location(currentRow--, currentColumn),
                "move west" => new Location(currentRow, currentColumn--),
                "move east" => new Location(currentRow, currentColumn++),
                "enable fountain" => Fountain.Activate(Player),
                _ => new StayCommand()
            };

            // Check if player is in a room with a monster & execute monster attack
            IMonster monster = GetRoomMonster();
            if (monster != null) monster.Execute(Player);
        }
    }

    public void AddMonster(IMonster monster) => Monsters[monster.Location.Row, monster.Location.Column] = monster;

    public IMonster GetRoomMonster() => Monsters[Player.Location.Row, Player.Location.Column];
}
public class Map
{
    public int Rows { get; }
    public int Columns { get; }
    public RoomType[,] Rooms { get; init; }

    public Map(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        Rooms = new RoomType[rows, columns];
    }

    public bool IsWithinMap(Location location) =>
        location.Row >= 0 && location.Row < Rows &&
        location.Column >= 0 && location.Column < Columns;
}

// Instances

public class Player
{
    public Location Location { get; init; } = new(0, 0);
    public bool IsAlive { get; set; } = true;
}

public class FountainOfObjects
{
    public Location Location { get; init; }
    public bool Activated { get; private set; } = false;

    public FountainOfObjects(Location location)
    {
        Location = location;
    }

    public bool Activate(Player player)
    {
        if (player.Location == this.Location)
        {
            Activated = true;
            return true;
        }
        return false;
    }
}

public class Amarok : IMonster
{
    public Location Location { get; init; }

    public Amarok(Location location)
    {
        Location = location;
    }

    public void Execute(Player player) 
    {
        player.IsAlive = false;
    }
}


public interface IMonster
{
    public Location Location { get; init; }

    public void Execute(Player player) {} // Monster does its thing on the player
}

// -- Helpers --

// Provides information about the state of the game
public static class LogHelper
{

}

// Helps with colored output
public static class ConsoleHelper
{
    public static void WriteLine(string text, ConsoleColor? color)
    {

    }

    public static void Write()
    {

    }
}

public record Location(int Row, int Column);
public enum RoomType { Normal, Fountain, Monster}

