

using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Windows.Input;

Console.Write("What map size do you want to run? small / medium / large: ");
string choice = Console.ReadLine();
switch (choice)
{
    case "small":
        RunSmallGame();
        break;
    case "medium":
        RunMediumGame();
        break;
    case "large":
        RunLargeGame();
        break;
    default:
        Console.WriteLine("Invalid input. Defaulting to medium.");
        RunMediumGame();
        break;
}

// Methods to initialize & run different game sizes

/* 

SETUP INFO
1. Placements (player, start location, fountain, etc.) must be 1 less than the total number of rows and columns. 
        Example: 4x4 map, the starting location is limited to 3 for rows & 3 for columns. (3, 3) will be the top-right corner to create the entrance, (0, 0) will be bottom-right.
2. Amaroks must not be placed on the start location or fountain
3. Fountian must not be placed at the start location
4. The player is initialized in the game class itself
*/

void RunSmallGame()
{
    int rows = 4;
    int columns = 4;

    Location startLocation = new Location(0, 0);
    FountainOfObjects fountain = new(new Location(0, 2));

    Game Game = new(rows, columns, startLocation, fountain);
    Game.Run();
}

void RunMediumGame()
{
    int rows = 6;
    int columns = 6;

    Location startLocation = new Location(5, 0);
    FountainOfObjects fountain = new(new Location(0, 5));
    Amarok amarok1 = new Amarok(new Location(0, 1));
    Amarok amarok2 = new Amarok(new Location(5, 5));

    Game Game = new(rows, columns, startLocation, fountain);
    Game.AddMonster(amarok1);
    Game.AddMonster(amarok2);
    Game.Run();
}

void RunLargeGame()
{
    int rows = 8;
    int columns = 8;

    Location startLocation = new Location(7, 7);
    FountainOfObjects fountain = new(new Location(0, 5));
    Amarok amarok1 = new Amarok(new Location(0, 1));
    Amarok amarok2 = new Amarok(new Location(5, 7));
    Amarok amarok3 = new Amarok(new Location(5, 7));

    Game Game = new(rows, columns, startLocation, fountain);
    Game.AddMonster(amarok1);
    Game.AddMonster(amarok2);
    Game.AddMonster(amarok3);
    Game.Run();
}

// Main game handler
public class Game
{
    public Player Player { get; init; }
    Map Map { get; init; }
    public FountainOfObjects Fountain { get; init; }
    public IMonster[,] Monsters; // Stores the monster type within each room
    Location StartLocation = new(0, 0);
    public bool GameWon { get; private set; } = false;

    public Game(int rows, int columns, Location startLocation, FountainOfObjects fountain)
    {
        Monsters = new IMonster[rows, columns];
        StartLocation = startLocation;
        Fountain = fountain;
        Map = new(rows, columns);
        Player = new(startLocation, Map, fountain);
    }

    public void Run()
    {
        while (!GameWon && Player.IsAlive)
        {
            // Send info to the player about where they are
            InfoHelper.SendInfo(Player, Fountain);

            // Let them know if an amarok is near
            if (Map.NearAmarokRoom(Player.Location)) ConsoleHelper.WriteLine("You can smell the rotten stench of an amarok in a nearby room.", ConsoleColor.DarkRed);

            // Request & accept movement input 
            ConsoleHelper.Write("What do you want to do? ", ConsoleColor.White);
            while (true)
            {
                bool actionSuccess = Player.RunPlayerAction();
                if (actionSuccess) break;
            }

            // Check if player is in a room with a monster & execute monster attack
            IMonster monster = GetRoomMonster(Player.Location);
            if (monster != null) monster.Execute(Player);

            // Check if player is alive. End game if player is dead
            if (!Player.IsAlive)
            {
                ConsoleHelper.WriteLine("YOU DIED", ConsoleColor.Red);
                break;
            }

            if (Fountain.Activated && Player.Location == StartLocation)
            {
                GameWon = true;
                ConsoleHelper.WriteLine("The Fountain of Objects has been restored! You win!", ConsoleColor.Magenta);
                break;
            }

            Console.WriteLine("---------------------------------------------------------------------------------------");
        }
    }

    public void AddMonster(IMonster monster) {
        Monsters[monster.Location.Row, monster.Location.Column] = monster;
        Map.SetRoomType(monster.Location.Row, monster.Location.Column, RoomType.Amarok);
    }

    public IMonster GetRoomMonster(Location location) => Monsters[location.Row, location.Column];
}

// Map-related functions
public class Map
{
    public int Rows { get; }
    public int Columns { get; }
    public RoomType[,] Rooms { get; init; }

    public Map(int rows, int columns)
    {
        Rooms = new RoomType[rows, columns];
        Rows = rows ;
        Columns = columns;
    }

    public bool NearAmarokRoom(Location location)
    {
        int row = location.Row;
        int column = location.Column;

        //Console.WriteLine("Debug | Current row:" + row);
        // Check if there is a monster above
        if (IsAmarokRoom(row + 1, column - 1) || IsAmarokRoom(row + 1, column) || IsAmarokRoom(row + 1, column + 1))
        {
            return true;
        }

        // Check if there is a monster to the left
        else if (IsAmarokRoom(row, column - 1))
        {
            return true;
        }

        // Check if there is a monster to the right
        else if (IsAmarokRoom(row, column + 1))
        {
            return true;
        }

        // Check if there is a monster below
        else if (IsAmarokRoom(row - 1, column - 1) || IsAmarokRoom(row - 1, column) || IsAmarokRoom(row - 1, column + 1))
        {
            return true;
        }
        return false;
    }

    public RoomType GetRoomType(int row, int column)
    {
        //Console.WriteLine($"Debug: Checking if row {row} and column {column} are within the map: {IsWithinMap(row, column)}.");
        if (!IsWithinMap(row, column))
            return RoomType.OutOfBounds;
        return Rooms[row, column];
    }

    public void SetRoomType(int row, int column, RoomType roomType)
    {
        Rooms[row, column] = roomType;
    }

    public bool IsWithinMap(int row, int column) => row >= 0 && row < Rows && column >= 0 && column < Columns;
    public bool IsWithinMap(Location location) => IsWithinMap(location.Row, location.Column);
    public bool IsAmarokRoom(int row, int column) => GetRoomType(row, column) == RoomType.Amarok;
}

// Instances

public class Player
{
    public Location StartLocation { get; init; }
    public Location Location { get; set; }
    Map Map { get; init; }
    public FountainOfObjects Fountain { get; init; }
    public bool IsAlive { get; set; } = true;

    public Player(Location startLocation, Map map, FountainOfObjects fountain)
    {
        StartLocation = startLocation;
        Location = startLocation;
        Map = map;
        Fountain = fountain;
    }

    public bool RunPlayerAction()
    {
        Location previousLocation = Location;
        bool atCaveEntrance = AtCaveEntrance();
        string choice = Console.ReadLine();
        switch (choice)
        {
            case "move north":
                MovePlayer(1, 0);
                if (Location == previousLocation) ConsoleHelper.WriteLine(atCaveEntrance ? "You cannot exit the cave yet!" : "There is a wall here.", ConsoleColor.Red);
                return true;
            case "move south":
                MovePlayer(-1, 0);
                if (Location == previousLocation) ConsoleHelper.WriteLine(atCaveEntrance ? "You cannot exit the cave yet!" : "There is a wall here.", ConsoleColor.Red);
                return true;
            case "move west":
                MovePlayer(0, -1);
                if (Location == previousLocation) ConsoleHelper.WriteLine(atCaveEntrance ? "You cannot exit the cave yet!" : "There is a wall here.", ConsoleColor.Red);
                return true;
            case "move east":
                MovePlayer(0, 1);
                if (Location == previousLocation) ConsoleHelper.WriteLine(atCaveEntrance ? "You cannot exit the cave yet!" : "There is a wall here.", ConsoleColor.Red);
                return true;
            case "activate fountain":
                Fountain.Activate(this);
                if (!Fountain.Activated) ConsoleHelper.WriteLine("The Fountain of Objects is not in this room.", ConsoleColor.Red);
                return true;
            default:
                return false;
        }
    }
    private void MovePlayer(int rowOffset, int ColumnOffset)
    {
        Location newLocation = new(Location.Row + rowOffset, Location.Column + ColumnOffset);
        if (Map.IsWithinMap(newLocation)) Location = newLocation;
    }

    public bool AtCaveEntrance() => Location == StartLocation;
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

    public void Execute(Player player) { } // Monster does its thing on the player
}

// -- Helpers --

// Provides information about the state of the game
public static class InfoHelper
{
    public static void SendInfo(Player player, FountainOfObjects fountain)
    {
        ConsoleHelper.WriteLine($"You are in the room at (Row={player.Location.Row}, Column={player.Location.Column}).", ConsoleColor.White);
        if (player.AtCaveEntrance())
        {
            ConsoleHelper.WriteLine("You see light coming from the cavern entrance.", ConsoleColor.Yellow);
        }
        else if (player.Location == fountain.Location)
        {
            string fountainMessage = !fountain.Activated ? "You hear water dripping in this room. The Fountain of Objects is here!" : "You hear the rushing waters from the Fountain of Objects. It has been reactivated!";
            ConsoleHelper.WriteLine(fountainMessage, ConsoleColor.Blue);
        }
    }
}

// Helps with colored output
public static class ConsoleHelper
{
    public static void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
    }

    public static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
    }
}

public record Location(int Row, int Column);
public enum RoomType { Normal, OutOfBounds, Fountain, Amarok }

