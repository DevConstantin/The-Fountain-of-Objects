

using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Windows.Input;

int rows = 4;
int columns = 4;
Game Game = new(rows, columns, new Player(), new FountainOfObjects(new Location(0, 2)));

Amarok amarok1 = new Amarok(new Location(0, 1));
Amarok amarok2 = new Amarok(new Location(3, 3));

Game.AddMonster(amarok1);
Game.AddMonster(amarok2);

Game.Run();

public class Game
{
    public Player Player { get; init; }
    public FountainOfObjects Fountain { get; init; }
    public IMonster[,] Monsters; // Stores the monster type within each room
    Map Map;
    public bool GameWon { get; private set; } = false;

    public Game(int rows, int columns, Player player, FountainOfObjects fountain)
    {
        Monsters = new IMonster[rows, columns];
        Player = player;
        Fountain = fountain;
        Map = new(rows, columns);
    }

    public void Run()
    {
        while (!GameWon && Player.IsAlive)
        {
            // Send info to the player about where they are
            InfoHelper.SendInfo(Player, Fountain);

            // Let them know if an amarok is near
            if (NearAmarokRoom()) ConsoleHelper.WriteLine("You can smell the rotten stench of an amarok in a nearby room.", ConsoleColor.DarkRed);

            // Request & accept movement input 
            ConsoleHelper.Write("What do you want to do? ", ConsoleColor.White);
            while (true)
            {
                bool actionSuccess = RunPlayerAction();
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

            if (GameWon)
            {
                ConsoleHelper.WriteLine("The Fountain of Objects has been restored! You win!", ConsoleColor.Magenta);
                break;
            }

            Console.WriteLine("---------------------------------------------------------------------------------------");
        }
    }

    public bool RunPlayerAction()
    {
        Location previousLocation = Player.Location;
        bool atCaveEntrance = Player.AtCaveEntrance();
        string choice = Console.ReadLine();
        switch (choice)
        {
            case "move north":
                MovePlayer(1, 0);
                if (Player.Location == previousLocation) ConsoleHelper.WriteLine("There is a wall here.", ConsoleColor.Red);
                return true;
            case "move south":
                MovePlayer(-1, 0);
                if (Player.Location == previousLocation) ConsoleHelper.WriteLine(atCaveEntrance ? "You cannot exit the cave yet!" : "There is a wall here.", ConsoleColor.Red);
                return true;
            case "move west":
                MovePlayer(0, -1);
                if (Player.Location == previousLocation) ConsoleHelper.WriteLine(atCaveEntrance ? "You cannot exit the cave yet!" : "There is a wall here.", ConsoleColor.Red);
                return true;
            case "move east":
                MovePlayer(0, 1);
                if (Player.Location == previousLocation) ConsoleHelper.WriteLine("There is a wall here.", ConsoleColor.Red);
                return true;
            case "enable fountain":
                Fountain.Activate(Player);
                if (!Fountain.Activated) ConsoleHelper.WriteLine("The Fountain of Objects is not in this room.", ConsoleColor.Red);
                return true;
            default:
                return false;
        }
    }

    private void MovePlayer(int rowOffset, int ColumnOffset)
    {
        Location newLocation = new(Player.Location.Row + rowOffset, Player.Location.Column + ColumnOffset);
        if (Map.IsWithinMap(newLocation)) Player.Location = newLocation;
    }

    public bool NearAmarokRoom()
    {
        int row = Player.Location.Row;
        int column = Player.Location.Column;

        //Console.WriteLine("Debug | Current row:" + row);
        // Check if there is a monster above
        if (Map.IsAmarokRoom(row + 1, column - 1) || Map.IsAmarokRoom(row + 1, column) || Map.IsAmarokRoom(row + 1, column + 1))
        {
            return true;
        }

        // Check if there is a monster to the left
        else if (Map.IsAmarokRoom(row, column - 1))
        {
            return true;
        }

        // Check if there is a monster to the right
        else if (Map.IsAmarokRoom(row, column + 1))
        {
            return true;
        }

        // Check if there is a monster below
        else if (Map.IsAmarokRoom(row - 1, column - 1) || Map.IsAmarokRoom(row - 1, column) || Map.IsAmarokRoom(row - 1, column + 1))
        {
            return true;
        }
        return false;
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

    public RoomType GetRoomType(int row, int column)
    {
        if (!IsWithinMap(row, column))
            return RoomType.OutOfBounds;
        return Rooms[row, column];
    }

    public bool IsWithinMap(int row, int column) => row >= 0 && row < Rows && column >= 0 && column < Columns;
    public bool IsWithinMap(Location location) => IsWithinMap(location.Row, location.Column);

    public void SetRoomType(int row, int column, RoomType roomType)
    {
        Rooms[row, column] = roomType;
    }
}

// Instances

public class Player
{
    public Location Location { get; set; } = new(0, 0);
    public bool IsAlive { get; set; } = true;

    public bool AtCaveEntrance() => Location.Row == 0 && Location.Column == 0;
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

