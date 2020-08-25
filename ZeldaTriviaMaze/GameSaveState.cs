/*Spencer Tyree, CSCD 371
 * Final Project
 * 
 * This file contains all of the gamesavestate class, which keeps track of all the player's stats(And saves them to)
 * This file also contains the room and door classes, which help in the construction of the maze.
 * 
 * EXTRA CREDIT: Added Zelda song and sound effects
 *              Added keys, chests, enemies, and a dungeon boss.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaTriviaMaze
{
    [Serializable]
    public class GameSaveState
    {
        private string name;
        public string Name { get { return this.name; } set { this.name = value; } }
        private string difficulty;
        public string Difficulty { get { return this.difficulty; } set { this.difficulty = value; } }
        private int life;
        public int Life { get { return life; } set { life = value; } }
        private Room[,] maze;
        public Room[,] Maze { get { return this.maze; } set { this.maze = value; } }
        private int playerx;
        public int Playerx { get { return this.playerx; } set { this.playerx = value; } }
        private int playery;
        public int Playery { get { return this.playery; } set { this.playery = value; } }
        private bool hasBow;
        public bool HasBow { get { return hasBow; } set { this.hasBow = value; } }
        private bool hasBombs;
        public bool HasBombs { get { return hasBombs; } set { this.hasBombs = value; } }
        private int keys;
        public int Keys { get { return keys; } set { if(keys!=MAX_KEYS)keys = value; } }
        private const int MAX_KEYS = 3;

        public GameSaveState(string name, string difficulty)
        {
            this.name = name;
            this.difficulty = difficulty;
            this.life = 3;
            this.maze = InitializeMaze();
            this.playerx = 0;
            this.playery = 0;
            this.hasBombs = false;
            if (difficulty == "easy")
            {
                this.hasBow = true;
                keys = 2;
            }
            else
            {
                this.hasBow = false;
                keys = 0;
            }

        }
        
        private Room[,] InitializeMaze()
        {
            Room[,] maze = new Room[4, 4];
            Door northdoor, eastdoor, southdoor, westdoor; 
            Boolean playerOccupied, hasBoss, visited;
            for (int x=0; x<4;  x++)
            {
                for(int y=0; y<4; y++)
                {
                    //beginning room
                    if(y==0 && x==0)
                    {
                        playerOccupied = true;
                        visited = true;
                    }
                    else
                    {
                        playerOccupied = false;
                        visited = false;
                    }
                    //west side room
                    if (y==0)
                    {
                        westdoor = new Door(null, null);
                    }
                    else
                    {
                        westdoor = new Door("locked", "West");
                    }
                    //east side room
                    if (y==3)
                    {
                        eastdoor = new Door(null, null);
                    }
                    else
                    {
                        eastdoor = new Door("locked", "East");
                    }
                    //north side room
                    if (x==0)
                    {
                        northdoor = new Door(null, null);
                    }
                    else
                    {
                        northdoor = new Door("locked", "North"); ;
                    }
                    //south side room
                    if(x==3)
                    {
                        southdoor = new Door(null, null);
                    }
                    else
                    {
                        southdoor = new Door("locked", "South") ;
                    }
                    //ending boss room
                    if (x==3 && y==3)
                    {
                        hasBoss = true;
                    }
                    else
                    {
                        hasBoss = false;
                    }
                    maze[x, y] = new Room(northdoor, eastdoor, southdoor, westdoor, hasBoss, playerOccupied, visited);

                }
            }
            GenerateEnemies(maze);
            GenerateChests(maze);
            SetOtherDoors(maze);
            return maze;
        }

        
        private void SetOtherDoors(Room[,] maze)
        {
            //sets the references to the doors on the other side
            for(int x=0; x<4; x++)
            {
                for(int y=0; y<4; y++)
                {
                    if(x==3 && y!=3)
                    {
                        maze[x, y].EastDoor.Otherend = maze[x, y + 1].WestDoor;
                        maze[x, y + 1].WestDoor.Otherend=maze[x, y].EastDoor;
                    }
                    else if (y==3 && x!=3)
                    {
                        maze[x, y].SouthDoor.Otherend = maze[x + 1, y].NorthDoor;
                        maze[x + 1, y].NorthDoor.Otherend = maze[x, y].SouthDoor;
                    }
                    else if(x!=3 && y!=3)
                    {
                        maze[x, y].EastDoor.Otherend = maze[x, y + 1].WestDoor;
                        maze[x, y + 1].WestDoor.Otherend = maze[x, y].EastDoor;

                        maze[x, y].SouthDoor.Otherend = maze[x + 1, y].NorthDoor;
                        maze[x + 1, y].NorthDoor.Otherend = maze[x, y].SouthDoor;
                    }
                }
            }
        }

        private void GenerateChests(Room[,] maze)
        {
            Random r1 = new Random();
            int x1;
            int x2;
            Boolean needroom=true;
            for (int i = 0; i < 3; i++)
            {
                while (needroom)
                {
                    x1 = r1.Next(4);
                    x2 = r1.Next(4);
                    if (RoomIsClear(maze, x1, x2))
                    {
                        maze[x1, x2].HasChest = true;
                        needroom = false;
                    }
                }
                needroom=true;
            }
        }

        private static Boolean RoomIsClear(Room[,] maze, int x1, int x2)
        {
            return maze[x1, x2].PlayerOccupied == false && maze[x1, x2].HasChest == false && maze[x1, x2].HasEnemy == false && maze[x1, x2].HasBoss == false;
        }

        private void GenerateEnemies(Room[,] maze)
        {
            Random r1 = new Random();
            int x1;
            int x2;
            Boolean needroom = true;
            for (int i = 0; i < 3; i++)
            {
                while (needroom)
                {
                    x1 = r1.Next(4);
                    x2 = r1.Next(4);
                    if (RoomIsClear(maze, x1, x2))
                    {
                        maze[x1, x2].HasEnemy = true;
                        needroom = false;
                    }
                }
                needroom = true;
            }
        }
    }
    [Serializable]
    public class Room
    {

        private Door northdoor;
        private Door eastdoor;
        private Door southdoor;
        private Door westdoor;
        private Boolean hasEnemy;
        private Boolean hasChest;
        private Boolean hasBoss;
        private Boolean playerOccupied;
        private Boolean visited;
        public Door NorthDoor { get { return this.northdoor; } set { this.northdoor = value; } }
        public Door SouthDoor { get { return this.southdoor; } set { this.southdoor = value; } }
        public Door EastDoor { get { return this.eastdoor; } set { this.eastdoor = value; } }
        public Door WestDoor { get { return this.westdoor; } set { this.westdoor = value; } }
        public Boolean HasEnemy { get { return this.hasEnemy; } set { this.hasEnemy = value; } }
        public Boolean HasChest { get { return this.hasChest; } set { this.hasChest = value; } }
        public Boolean HasBoss { get { return this.hasBoss; } set { this.hasBoss = value; } }
        public Boolean PlayerOccupied { get { return this.playerOccupied; } set { this.playerOccupied = value; } }
        public Boolean Visited { get { return this.visited; } set { this.visited = value; } }

        public Room(Door northdoor, Door eastdoor, Door southdoor, Door westdoor, Boolean hasBoss, Boolean playerOccupied, Boolean visited)
        {
            this.northdoor = northdoor;
            this.eastdoor = eastdoor;
            this.southdoor = southdoor;
            this.westdoor = westdoor;
            this.hasBoss = hasBoss;
            this.playerOccupied = playerOccupied;
            this.hasEnemy = false;
            this.hasChest = false;
            this.visited = visited;
        }
    }
    [Serializable]
    public class Door
    {
        private string status;
        public string Status { get { return this.status; } set { this.status = value; } }

        private string direction;
        public string Direction { get { return this.direction; } set { this.direction = value; } }
        private Door otherend;
        public Door Otherend { get { return this.otherend; } set { this.otherend = value; } }
        public Door(string status, string direction)
        {
            this.status = status;
            this.direction=direction;
            this.otherend = null;
        }
        public Boolean Exists()
        {
            return status != null;
        }
        public Boolean IsOpen()
        {
            return status == "open";
        }
        public Boolean IsWelded()
        {
            return status == "welded"; 
        }
        public Boolean IsLocked()
        {
            return status == "locked";
        }
    }
}
