/*Spencer Tyree, CSCD 371
 * Final Project
 * 
 * This file contains all of the game logic and code for the main game window.
 * 
 * EXTRA CREDIT: Added Zelda song and sound effects
 *              Added keys, chests, enemies, and a dungeon boss.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZeldaTriviaMaze
{
    /// <summary>
    /// Interaction logic for MainGameWindow.xaml
    /// </summary>
    public partial class MainGameWindow : Window
    {
        private GameSaveState gamestate;
        public GameSaveState Gamestate { get { return gamestate; } set { this.gamestate = value; } }

        private SQLiteConnection conn=null;
        private SQLiteCommand comm;
        private SQLiteDataReader datareader;
        private string rightanswerG;
        private List<string> shortanswers;
        private Door doorToMoveThrough;
        private Boolean hasBeenSaved=true;
        private Boolean isEnemyQuestion = false;
        private Boolean isFinalBoss = false;
        private Boolean isNormalQuestion = false;
        public MainGameWindow()
        {
            InitializeComponent();
        }

       
        private void HowToMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("To play Zelda Trivia maze, first select the direction you would like to move. If the door is locked(black), you will have to answer a "+
                            "trivia question to pass through it. If you answer correctly, the door will be unlocked(green) and you can pass safely through. If you answer"+
                            " incorrectly, however, the door will be welded shut(red), and you will not be able to pass through it. "+
                            "\nAlong the way, you may encounter enemies. If you encounter one, you must answer a trivia question correctly to defeat it. Answer the question"+
                            " Incorrectly, and you will lose life. You may also encounter chests in the dungeon, these will give you either a useful item, such as A Bow or "+
                            "Bombs(which can be used to defeat an enemy instantly) or a key(which can be used to open a door without answering a question). The chest may also "+
                            "contain hearts, which will refill your life. To use an item, click its icon at the top of the window.\nThe game ends when either you reach the exit of the dungeon, you cannot make it to the exit(because all"+
                            " the possible paths to the exit are welded shut), or you run out of life. A boss awaits you at the end! You can also save your game, and resume it later.\nHave fun! :)", "How To Play", MessageBoxButton.OK);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Spencer Tyree\nVersion 1.0\nScrollbar help taken from http://stackoverflow.com/questions/736153/enabling-scrollbar-in-wpf \nStatus bar help taken from http://www.wpf-tutorial.com/common-interface-controls/statusbar-control/ \nPlease don't sue me Nintendo");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (conn != null)
                conn.Close();
            this.Close();
        }

        private void Save_Prompt(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!hasBeenSaved)
            {
                MessageBoxResult result= MessageBox.Show("Do you want to save your game before quitting?", "Save", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
                else if (result == MessageBoxResult.Yes)
                    SaveGame();
            }
        }

        private void SaveGame()
        {
            BinaryFormatter bin = new BinaryFormatter();
            string filename = this.Gamestate.Name + ".dat";
            using(Stream fstream=new FileStream(filename,FileMode.Create, FileAccess.Write))
            {
                bin.Serialize(fstream, this.gamestate);
            }
            hasBeenSaved = true;
        }

        private void SaveGame_Click(object sender, RoutedEventArgs e)
        {
            SaveGame();
        }

        private void SetUpGame(object sender, RoutedEventArgs e)
        {
            PlayerNameLabel.Content = gamestate.Name;
            if(gamestate.Keys>=1)
            {
                KeyImage1.Visibility = Visibility.Visible;
                if(gamestate.Keys>=2)
                {
                    KeyImage2.Visibility = Visibility.Visible;
                    if(gamestate.Keys==3)
                    {
                        KeyImage3.Visibility = Visibility.Visible;
                    }
                }
            }
            if(gamestate.HasBombs)
            {
                BombsImage.Visibility = Visibility.Visible;
            }
            if(gamestate.HasBow)
            {
                BowImage.Visibility = Visibility.Visible;
            }
            if (gamestate.Life==2)
            {
                EmptyHeart1.Visibility = Visibility.Visible;
                if(gamestate.Life==1)
                {
                    EmptyHeart2.Visibility = Visibility.Visible;
                }
            }
            DrawMaze();
            Canvas.SetLeft(Link, 105 + (109 * gamestate.Playery));
            Canvas.SetTop(Link, 100 + (110 * gamestate.Playerx));
            EnableArrows();
            CurrentPosition.Text = "Room " + gamestate.Playery + ", " + gamestate.Playerx;
        }

        private void DrawMaze()
        {
            for(int x=0; x<4; x++)
            {
                for (int y=0; y<4; y++)
                {
                    if (gamestate.Maze[x,y].Visited==true && gamestate.Maze[x, y].EastDoor.Exists())
                    {
                        if (gamestate.Maze[x, y].EastDoor.IsOpen())
                        {
                            //draw door
                            DrawDoor(x, y, Brushes.Green, "East");
                        }
                        else if(gamestate.Maze[x,y].EastDoor.IsWelded())
                        {
                            DrawDoor(x, y, Brushes.Red, "East");
                        }
                        else
                        {
                            DrawDoor(x, y, Brushes.Black, "East");
                        }
                    }
                    if (gamestate.Maze[x, y].Visited == true && gamestate.Maze[x, y].SouthDoor.Exists())
                    {
                        if (gamestate.Maze[x, y].SouthDoor.IsOpen())
                        {
                            //draw door
                            DrawDoor(x, y, Brushes.Green, "South");
                        }
                        else if (gamestate.Maze[x, y].SouthDoor.IsWelded())
                        {
                            DrawDoor(x, y, Brushes.Red, "South");
                        }
                        else
                        {
                            DrawDoor(x, y, Brushes.Black, "South");
                        }
                    }
                }
            }
            
        }
            
        private void DrawDoor(int x, int y, SolidColorBrush solidColorBrush, String whichDoor)
        {
            //<Rectangle Fill="White" Height="2" Canvas.Left="186" Stroke="Black" Canvas.Top="122" Width="36"/>
            //<Rectangle Fill="White" Height="2" Canvas.Left="186" Stroke="Black" Canvas.Top="157" Width="36"/>
            int l1Left, l1Top, l2Left, l2Top;
            if (whichDoor=="East" || whichDoor=="West")
            {
                Line l1 = new Line()
                {
                    Stroke = solidColorBrush,
                    StrokeThickness = 2,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 36,
                    Y2 = 0
                };
                Line l2 = new Line()
                {
                    Stroke = solidColorBrush,
                    StrokeThickness = 2,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 36,
                    Y2 = 0
                };
                if (whichDoor == "East")
                {
                    l1Left = (186 + (109 * y));
                    l1Top = (122 + (109 * x));
                    l2Left = (186 + (109 * y));
                    l2Top = (157 + (110 * x));
                }
                else
                {
                    l1Left = (186 + (109 * (y-1)));
                    l1Top = (122 + (109 * x));
                    l2Left = (186 + (109 * (y-1)));
                    l2Top = (157 + (110 * x));
                }
                //set top line
                Canvas.SetLeft(l1, l1Left);
                Canvas.SetTop(l1, l1Top);

                //set bottem line
                Canvas.SetLeft(l2, l2Left);
                Canvas.SetTop(l2, l2Top);

                DungeonCanvas.Children.Add(l1);
                DungeonCanvas.Children.Add(l2);

            }
              //  <Rectangle Fill="White" Height="36" Width="2" Stroke="Black" Canvas.Left="167" Canvas.Top="177"/>
            //<Rectangle Fill="White" Height="36" Width="2" Stroke="Black" Canvas.Left="135" Canvas.Top="177"/>
            //southdoor or northdoor
            else
            {
                Line l1 = new Line()
                {
                    Stroke = solidColorBrush,
                    StrokeThickness = 2,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = 36
                };
                Line l2 = new Line()
                {
                    Stroke = solidColorBrush,
                    StrokeThickness = 2,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 0,
                    Y2 = 36
                };
                if (whichDoor == "South")
                {
                    l1Left = (135 + (110 * y));
                    l1Top = (177 + (110 * x));
                    l2Left = (167 + (110 * y));
                    l2Top = (177 + (110 * x));
                }
                else
                {
                    l1Left = (135 + (110 * y));
                    l1Top = (177 + (110 * (x-1)));
                    l2Left = (167 + (110 * y));
                    l2Top = (177 + (110 * (x-1)));
                }
                //set top line
                Canvas.SetLeft(l1, l1Left);
                Canvas.SetTop(l1, l1Top);

                //set bottem line
                Canvas.SetLeft(l2, l2Left);
                Canvas.SetTop(l2, l2Top);

                DungeonCanvas.Children.Add(l1);
                DungeonCanvas.Children.Add(l2);
            }
        }

        private void UpButton_Click(object sender, MouseButtonEventArgs e)
        {
            hasBeenSaved = false;
            int x = gamestate.Playerx, y = gamestate.Playery;
            if (gamestate.Maze[x, y].NorthDoor.Exists())
            {
                if (gamestate.Maze[x, y].NorthDoor.IsOpen())
                {
                    QuestionLabel.Content = "";
                    //move link and change values
                    gamestate.Playerx -= 1;
                    gamestate.Maze[x, y].PlayerOccupied = false;
                    gamestate.Maze[x - 1, y].PlayerOccupied = true;
                    gamestate.Maze[x - 1, y].Visited = true;
                    Canvas.SetTop(Link, 100 + (110 * x) - 110);
                    CheckForNewDoors();
                    EnableArrows();
                    EnemyEncounter();
                    FoundChest();
                }
                else if (gamestate.Maze[x, y].NorthDoor.IsWelded())
                {
                    MessageBox.Show("Door is welded");
                }
                else
                {
                    doorToMoveThrough = gamestate.Maze[x, y].NorthDoor;
                    //ask question
                    isNormalQuestion = true;
                    AskQuestion();
                }
            }
            else
            {
                MessageBox.Show("Can't move that way");
            }
            CurrentPosition.Text = "Room " + gamestate.Playery + ", " + gamestate.Playerx;
        }

        private void CheckForNewDoors()
        {
            int x = gamestate.Playerx, y = gamestate.Playery;
            if(gamestate.Maze[x,y].EastDoor.IsLocked())
            {
                DrawDoor(x, y, Brushes.Black, "East");
            }
            if(gamestate.Maze[x,y].SouthDoor.IsLocked())
            {
                DrawDoor(x, y, Brushes.Black, "South");
            }
            if (gamestate.Maze[x, y].WestDoor.IsLocked())
            {
                DrawDoor(x, y, Brushes.Black, "West");
            }
            if (gamestate.Maze[x, y].NorthDoor.IsLocked())
            {
                DrawDoor(x, y, Brushes.Black, "North");
            }
        }

        private void FoundChest()
        {
            int x = gamestate.Playerx, y = gamestate.Playery, num;
            Boolean notfound=true;
            Random rand=new Random();
            if(gamestate.Maze[x,y].HasChest)
            {
                while(notfound)
                {
                    FanfareMediaElement.Stop();
                    num = rand.Next(7);
                    if(num <= 3 && gamestate.Keys < 3)
                    {
                        MessageBox.Show("Found a key in a chest!");
                        gamestate.Keys++;
                        if(KeyImage1.Visibility==Visibility.Hidden)
                        {
                            KeyImage1.Visibility = Visibility.Visible;
                        }
                        else if (KeyImage2.Visibility == Visibility.Hidden)
                        {
                            KeyImage2.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            KeyImage3.Visibility = Visibility.Visible;
                        }
                        notfound = false;
                    }
                    else if(num==4 && gamestate.HasBombs==false)
                    {
                        MessageBox.Show("Found Bombs in a Chest!");
                        BombsImage.Visibility = Visibility.Visible;
                        gamestate.HasBombs=true;
                        notfound = false;
                    }
                    else if(num==5 && gamestate.HasBow==false)
                    {
                        MessageBox.Show("Found a bow and an arrow in a chest!");
                        BowImage.Visibility = Visibility.Visible;
                        gamestate.HasBow = true;
                        notfound = false;
                    }
                    else if(num==6)
                    {
                        MessageBox.Show("Found a heart in a chest! Life up!");
                        if (gamestate.Life < 3)
                            gamestate.Life++;
                        if (EmptyHeart2.Visibility == Visibility.Visible)
                            EmptyHeart2.Visibility = Visibility.Hidden;
                        else if (EmptyHeart3.Visibility == Visibility.Visible)
                            EmptyHeart3.Visibility = Visibility.Hidden;
                        notfound = false;
                    }
                }
                FanfareMediaElement.Play();
                gamestate.Maze[x, y].HasChest = false;
            }
            
        }

        private void EnemyEncounter()
        {
            int x = gamestate.Playerx, y = gamestate.Playery;
            if(gamestate.Maze[x,y].HasEnemy)
            {
                MessageBox.Show("Enemy Encounter! Answer a question correctly to defeat it!");
                isEnemyQuestion = true;
                AskQuestion();
                gamestate.Maze[x, y].HasEnemy = false;
            }
        }

        private void RightButton_Click(object sender, MouseButtonEventArgs e)
        {
            hasBeenSaved = false;
            int x=gamestate.Playerx, y=gamestate.Playery;
            if (gamestate.Maze[x, y].EastDoor.Exists())
            {
                if (gamestate.Maze[x,y].EastDoor.IsOpen())
                {
                    QuestionLabel.Content = "";
                    //move link and change values
                    gamestate.Playery += 1;
                    gamestate.Maze[x, y].PlayerOccupied = false;
                    gamestate.Maze[x, y + 1].PlayerOccupied = true;
                    gamestate.Maze[x, y + 1].Visited = true;
                    Canvas.SetLeft(Link, 105 + (109 * y) + 109);
                    CheckForNewDoors();
                    EnableArrows();
                    EnemyEncounter();
                    FoundChest();
                    FinalBoss();
                }
                else if (gamestate.Maze[x,y].EastDoor.IsWelded())
                {
                    MessageBox.Show("Door is welded");
                }
                else
                {
                    isNormalQuestion = true;
                    doorToMoveThrough = gamestate.Maze[x, y].EastDoor;
                    //ask question
                    AskQuestion();
                }
            }
            else
            {
                MessageBox.Show("Can't move that way");
            }
            CurrentPosition.Text = "Room " + gamestate.Playery + ", " + gamestate.Playerx;
        }

        
        private void AskQuestion()
        {
            
            CheckSQLiteConnection();
            Random ran = new Random();
            int questionid=ran.Next(38);
            String question, type, option1, option2, option3, option4;
            comm.CommandText = "select * from Questions where QuestionID='" + questionid + "';";//"' and Difficulty='"+gamestate.Difficulty+"';";
            datareader = comm.ExecuteReader();
            if(datareader.Read())
            {
                //set question
                question = (String)datareader["Question"];
                QuestionLabel.Content = question;

                type = (String)datareader["Question_Type"];
                datareader.Close();
                if(type=="TrueFalse")
                {
                    comm.CommandText = "select * from TFAnswers where QuestionID='" + questionid + "';";
                    datareader = comm.ExecuteReader();
                    if(datareader.Read())
                    {
                        rightanswerG = (String)datareader["RightAnswer"];
                        Option1Button.IsEnabled = true;
                        Option1Button.Content = "True";

                        Option2Button.IsEnabled = true;
                        Option2Button.Content = "False";

                        Option3Button.IsEnabled = false;
                        Option3Button.Content = "";

                        Option4Button.IsEnabled = false;
                        Option4Button.Content = "";

                        ShortAnswerTextBox.Visibility = Visibility.Hidden;
                        DisableArrows();
                    }
                }
                else if (type=="MultipleChoice")
                {
                    comm.CommandText = "select * from MultChoiceAnswers where QuestionID='" + questionid + "';";
                    datareader = comm.ExecuteReader();
                    if(datareader.Read())
                    {
                        option1 = (String)datareader["AnswerA"];
                        option2 = (String)datareader["AnswerB"];
                        option3 = (String)datareader["AnswerC"];
                        option4 = (String)datareader["AnswerD"];
                        rightanswerG = (String)datareader["RightAnswer"];

                        Option1Button.IsEnabled = true;
                        Option1Button.Content = option1;

                        Option2Button.IsEnabled = true;
                        Option2Button.Content = option2;

                        Option3Button.IsEnabled = true;
                        Option3Button.Content = option3;

                        Option4Button.IsEnabled = true;
                        Option4Button.Content = option4;

                        ShortAnswerTextBox.Visibility = Visibility.Hidden;
                        DisableArrows();
                    }
                }
                else //shortanswer
                {
                    ShortAnswerTextBox.Visibility = Visibility.Visible;
                    Option1Button.Visibility = Visibility.Hidden;
                    Option2Button.Visibility = Visibility.Hidden;
                    Option3Button.IsEnabled = true;
                    Option3Button.Content = "Enter";
                    Option4Button.IsEnabled = false;
                    shortanswers=new List<string>(7);
                    comm.CommandText = "select * from ShortAnswers where QuestionID='" + questionid + "';";
                    datareader = comm.ExecuteReader();
                    if (datareader.Read())
                    {
                        shortanswers.Add((String)datareader["Answer1"]);
                        shortanswers.Add((String)datareader["Answer2"]);
                        shortanswers.Add((String)datareader["Answer3"]);
                        shortanswers.Add((String)datareader["Answer4"]);
                        shortanswers.Add((String)datareader["Answer5"]);
                        shortanswers.Add((String)datareader["Answer6"]);
                        shortanswers.Add((String)datareader["Answer7"]);
                        DisableArrows();
                    }
                }
            }

            datareader.Close();
            
        }

        private void DisableArrows()
        {
            UpButton.IsEnabled = false;
            UpButton.Opacity = .5;
            DownButton.IsEnabled = false;
            DownButton.Opacity = .5;
            RightButton.IsEnabled = false;
            RightButton.Opacity = .5;
            LeftButton.IsEnabled = false;
            LeftButton.Opacity = .5;
        }

        private void EnableArrows()
        {
            if (gamestate.Playerx==0)
            {
                UpButton.IsEnabled = false;
                UpButton.Opacity = .5;
            }
            else
            {
                UpButton.IsEnabled = true;
                UpButton.Opacity = 1.0;
            }
            if(gamestate.Playerx==3)
            {
                DownButton.IsEnabled = false;
                DownButton.Opacity = .5;
            }
            else
            {
                DownButton.IsEnabled = true;
                DownButton.Opacity = 1.0;
            }
            if (gamestate.Playery == 0)
            {
                LeftButton.IsEnabled = false;
                LeftButton.Opacity = .5;
            }
            else
            {
                LeftButton.IsEnabled = true;
                LeftButton.Opacity = 1.0;
            }
            if (gamestate.Playery==3)
            {
                RightButton.IsEnabled = false;
                RightButton.Opacity = .5;
            }
            else
            {
                RightButton.IsEnabled = true;
                RightButton.Opacity = 1.0;
            }
            isNormalQuestion = false;
        }
        private void CheckSQLiteConnection()
        {
            if(conn==null)
            {
                conn = new SQLiteConnection("Data Source=zelda_questions.db;Version=3;New=True;Compress=True;");
                conn.Open();
                comm = conn.CreateCommand();
            }
        }

        private void DownButton_Click(object sender, MouseButtonEventArgs e)
        {
            hasBeenSaved = false;
            int x = gamestate.Playerx, y = gamestate.Playery;
            if (gamestate.Maze[x, y].SouthDoor.Exists())
            {
                if (gamestate.Maze[x, y].SouthDoor.IsOpen())
                {
                    QuestionLabel.Content = "";
                    //move link and change values
                    gamestate.Playerx += 1;
                    gamestate.Maze[x, y].PlayerOccupied = false;
                    gamestate.Maze[x+1, y].PlayerOccupied = true;
                    gamestate.Maze[x+1, y].Visited = true;
                    Canvas.SetTop(Link, 100 + (110 * x) + 110);
                    CheckForNewDoors();
                    EnableArrows();
                    EnemyEncounter();
                    FoundChest();
                    FinalBoss();
                }
                else if (gamestate.Maze[x, y].SouthDoor.IsWelded())
                {
                    MessageBox.Show("Door is welded");
                }
                else
                {
                    doorToMoveThrough = gamestate.Maze[x, y].SouthDoor;
                    isNormalQuestion = true;
                    //ask question
                    AskQuestion();
                }
            }
            else
            {
                MessageBox.Show("Can't move that way");
            }
            CurrentPosition.Text = "Room " + gamestate.Playery + ", " + gamestate.Playerx;
        }

        private void FinalBoss()
        {
            if (gamestate.Playerx==3 && gamestate.Playery==3)
            {
                MessageBox.Show("Dungeon Boss! Answer one final question correctly to defeat it!");
                isFinalBoss = true;
                isNormalQuestion = false;
                AskQuestion();
            }
        }

        private void LeftButton_Click(object sender, MouseButtonEventArgs e)
        {
            hasBeenSaved = false;
           int x=gamestate.Playerx, y=gamestate.Playery;
            if (gamestate.Maze[x, y].WestDoor.Exists())
            {
                if (gamestate.Maze[x,y].WestDoor.IsOpen())
                {
                    QuestionLabel.Content = "";
                    //move link and change values
                    gamestate.Playery -= 1;
                    gamestate.Maze[x, y].PlayerOccupied = false;
                    gamestate.Maze[x, y - 1].PlayerOccupied = true;
                    gamestate.Maze[x, y - 1].Visited = true;
                    Canvas.SetLeft(Link, 105 + (109 * y) - 109);
                    CheckForNewDoors();
                    EnableArrows();
                    EnemyEncounter();
                    FoundChest();
                }
                else if (gamestate.Maze[x,y].WestDoor.IsWelded())
                {
                    MessageBox.Show("Door is welded");
                }
                else
                {
                    doorToMoveThrough = gamestate.Maze[x, y].WestDoor;
                    isNormalQuestion = true;
                    //ask question
                    AskQuestion();
                }
            }
            else
            {
                MessageBox.Show("Can't move that way");
            }
            CurrentPosition.Text = "Room " + gamestate.Playery + ", " + gamestate.Playerx;
        }

        private void Option1_Click(object sender, RoutedEventArgs e)
        {
            if (rightanswerG=="A" || rightanswerG=="True")
            {
                if (isEnemyQuestion)
                {
                    QuestionLabel.Content = "You defeated Ganon's evil minion!";
                }
                else if(isFinalBoss)
                {
                    BossVictory();
                }
                else
                {
                    QuestionCorrect();
                }
            }
            else
            {
                if (isEnemyQuestion)
                {
                    LoseEnemyEncounter();
                }
                else if (isFinalBoss)
                {
                    MessageBox.Show("Wrong! Lose Life!");
                    SubtractLife();
                    AskQuestion();
                }
                else
                {
                    QuestionWrong();
                }
            }
            if (!isFinalBoss)
            {
                isEnemyQuestion = false;
                DisableAllButtons();
                EnableArrows();
            }
        }

        private void BossVictory()
        {
            hasBeenSaved = true;
            QuestionLabel.Content = "VICTORY! YOU WIN!!!";
            MessageBox.Show("You have defeated the dungeon's boss, and made it out of the dungeon alive.\n\n Congratulatons! You are truly the hero of legend!","A winner is you!");
            this.Close();
        }

        private void QuestionWrong()
        {
            QuestionLabel.Content = "Wrong! The door is welded shut!";
            doorToMoveThrough.Status = "welded";
            doorToMoveThrough.Otherend.Status = "welded";
            DrawDoor(gamestate.Playerx, gamestate.Playery, Brushes.Red, doorToMoveThrough.Direction);
            CheckForPath();
        }

        private void CheckForPath()
        {
            int[,] visitedarray = new int[4, 4];
            Boolean isPath = CheckForPathR(visitedarray, 0, 0);
            if(!isPath)
            {
                GameOver("All paths to the exit have been blocked! you lose!");
            }
        }

        private void GameOver(String gameovermessage)
        {
            hasBeenSaved = true;
            GanonLaughMediaElement.Play();
            MessageBoxResult result = MessageBox.Show(gameovermessage+"\nRETURN OF GANON\n\nReload game?", "", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                LoadGameWindow newwindow = new LoadGameWindow();
                newwindow.ShowDialog();
                this.Close();
            }
            else
            {
                this.Close();
            }
        }

        private bool CheckForPathR(int [,]visitedarray, int x, int y)
        {
            visitedarray[x, y] = 1;
            Boolean ispath;
            if (x == 3 && y == 3)
                ispath=true;
            else if(gamestate.Maze[x,y].EastDoor.Exists() && !gamestate.Maze[x,y].EastDoor.IsWelded() && visitedarray[x, y+1]==0)
            {
                ispath = CheckForPathR(visitedarray, x, y + 1);
            }
            else ispath=false;
            if(!ispath && gamestate.Maze[x,y].SouthDoor.Exists() && !gamestate.Maze[x,y].SouthDoor.IsWelded() && visitedarray[x+1,y]==0)
            {
                ispath = CheckForPathR(visitedarray, x + 1, y);
            }
            if (!ispath && gamestate.Maze[x, y].WestDoor.Exists() && !gamestate.Maze[x, y].WestDoor.IsWelded() && visitedarray[x, y-1] == 0)
            {
                ispath = CheckForPathR(visitedarray, x, y - 1);
            }
            if(!ispath && gamestate.Maze[x,y].NorthDoor.Exists() && !gamestate.Maze[x,y].NorthDoor.IsWelded() && visitedarray[x-1, y]==0)
            {
                ispath = CheckForPathR(visitedarray, x -1 , y);
            }
            return ispath;
        }

        private void DisableAllButtons()
        {
            ShortAnswerTextBox.Visibility = Visibility.Hidden;
            ShortAnswerTextBox.Text = "";

            Option1Button.IsEnabled = false;
            Option1Button.Content = "";
            Option1Button.Visibility = Visibility.Visible;

            Option2Button.IsEnabled = false;
            Option2Button.Content = "";
            Option2Button.Visibility = Visibility.Visible;

            Option3Button.IsEnabled = false;
            Option3Button.Content = "";

            Option4Button.IsEnabled = false;
            Option4Button.Content = "";

            rightanswerG = "";
        }

        private void Option2Button_Click(object sender, RoutedEventArgs e)
        {
            if (rightanswerG == "B" || rightanswerG == "False")
            {
                if (isEnemyQuestion)
                {
                    QuestionLabel.Content = "You defeated Ganon's evil minion!";
                }
                else if (isFinalBoss)
                {
                    BossVictory();
                }
                else
                {
                    QuestionCorrect();
                }
            }
            else
            {
                if (isEnemyQuestion)
                {
                    LoseEnemyEncounter();
                }
                else if (isFinalBoss)
                {
                    MessageBox.Show("Wrong! Lose Life!");
                    SubtractLife();
                    AskQuestion();
                }
                else
                {
                    QuestionWrong();
                }
            }
            if (!isFinalBoss)
            {
                isEnemyQuestion = false;
                DisableAllButtons();
                EnableArrows();
            }
        }

        private void Option3Button_Click(object sender, RoutedEventArgs e)
        {
            if (rightanswerG == "C" || CheckShortAnswers())
            {
                if (isEnemyQuestion)
                {
                    QuestionLabel.Content = "You defeated Ganon's evil minion!";
                }
                else if (isFinalBoss)
                {
                    BossVictory();
                }
                else
                {
                    QuestionCorrect();
                }
            }
            else
            {
                if (isEnemyQuestion)
                {
                    LoseEnemyEncounter();
                }
                else if (isFinalBoss)
                {
                    MessageBox.Show("Wrong! Lose Life!");
                    SubtractLife();
                    AskQuestion();
                }
                else
                {
                    QuestionWrong();
                }
            }
            if (!isFinalBoss)
            {
                isEnemyQuestion = false;
                DisableAllButtons();
                EnableArrows();
            }
        }

        private bool CheckShortAnswers()
        {
            String answer=ShortAnswerTextBox.Text;
            if(answer!="" && ShortAnswerTextBox.Visibility==Visibility.Visible)
            {
                answer = answer.ToLower();
                foreach(String s in shortanswers)
                {
                    string correct = s.ToLower();
                    if(answer==correct)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Option4Button_Click(object sender, RoutedEventArgs e)
        {
            if (rightanswerG == "D")
            {
                if (isEnemyQuestion)
                {
                    QuestionLabel.Content = "You defeated Ganon's evil minion!";
                }
                else if (isFinalBoss)
                {
                    BossVictory();
                }
                else
                {
                    QuestionCorrect();
                }
            }
            else
            {
                if(isEnemyQuestion)
                {
                    LoseEnemyEncounter();
                }
                else if (isFinalBoss)
                {
                    MessageBox.Show("Wrong! Lose Life!");
                    SubtractLife();
                    AskQuestion();
                }
                else
                {
                    QuestionWrong(); 
	            }
            }
            if (!isFinalBoss)
            {
                isEnemyQuestion = false;
                DisableAllButtons();
                EnableArrows();
            }
        }

        private void QuestionCorrect()
        {
            QuestionLabel.Content = "Correct! The door is open!";
            //open door
            doorToMoveThrough.Status = "open";
            doorToMoveThrough.Otherend.Status = "open";
            //drawDoor
            DrawDoor(gamestate.Playerx, gamestate.Playery, Brushes.Green, doorToMoveThrough.Direction);
        }

        private void LoseEnemyEncounter()
        {
            QuestionLabel.Content = "Wrong! You lose a heart!";
            SubtractLife();
        }

        private void SubtractLife()
        {
            gamestate.Life--;
            if (gamestate.Life == 2)
            {
                EmptyHeart1.Visibility = Visibility.Visible;
            }
            else if (gamestate.Life == 1)
            {
                EmptyHeart2.Visibility = Visibility.Visible;
            }
            else
            {
                GameOver("Your life has been depleted! You lose!");
            }
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGameForm form1 = new NewGameForm();
            form1.Show();
            this.Close();
        }

        private void Bow_Click(object sender, MouseButtonEventArgs e)
        {
            if(isEnemyQuestion)
            {
                QuestionLabel.Content = "The powerful Hero's Bow defeats Ganon's minion!";
                BowImage.Visibility = Visibility.Hidden;
                gamestate.HasBow = false;
                isEnemyQuestion = false;
                DisableAllButtons();
                EnableArrows();
            }
            else if(isFinalBoss)
            {
                QuestionLabel.Content = "The dungeon boss is defeated by the Hero's Bow!";
                BossVictory();
            }
            else
            {
                MessageBox.Show("Can only use weapons during battles!");
            }
        }

        private void Bombs_Click(object sender, MouseButtonEventArgs e)
        {
            if(isEnemyQuestion)
            {
                QuestionLabel.Content = "A powerful bomb is used to obliterate Ganon's minion!";
                BombsImage.Visibility = Visibility.Hidden;
                gamestate.HasBombs = false;
                isEnemyQuestion = false;
                DisableAllButtons();
                EnableArrows();
            }
            else if(isFinalBoss)
            {
                BossVictory();
            }
            else
            {
                MessageBox.Show("Can only use weapons during battle!");
            }
        }

        private void Key_Click(object sender, MouseButtonEventArgs e)
        {
            Image key = (Image)sender;
            if(isNormalQuestion)
            {
                QuestionLabel.Content = "You used a key to get through a door!";
                String name = key.Name;
                if (name=="KeyImage1")
                {
                    KeyImage1.Visibility = Visibility.Hidden;
                }
                else if (name=="KeyImage2")
                {
                    KeyImage2.Visibility = Visibility.Hidden;
                }
                else
                {
                    KeyImage3.Visibility = Visibility.Hidden;
                }
                doorToMoveThrough.Status = "open";
                doorToMoveThrough.Otherend.Status = "open";
                //drawDoor
                gamestate.Keys--;
                DrawDoor(gamestate.Playerx, gamestate.Playery, Brushes.Green, doorToMoveThrough.Direction);
                isNormalQuestion = false;

                DisableAllButtons();
                EnableArrows();
            }
            else
            {
                MessageBox.Show("You can only use keys when trying to open a door!");
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            LoadGameWindow fiisannoying = new LoadGameWindow();
            fiisannoying.Show();
            this.Close();
        }

        private void MouseOver(object sender, MouseEventArgs e)
        {
            MenuItem gogreaselightninyoureburninupthequartermile = (MenuItem)sender;
            OptionsInfo.Text = gogreaselightninyoureburninupthequartermile.ToolTip.ToString() ;
        }

        private void MouseGoAway(object sender, MouseEventArgs e)
        {
            OptionsInfo.Text = "";
        }

        private void MouseOverButtons(object sender, MouseEventArgs e)
        {
            Button gogreaselightninyourecoastinthroughtheheatlaptrial = (Button)sender;
            OptionsInfo.Text = gogreaselightninyourecoastinthroughtheheatlaptrial.ToolTip.ToString();
        }
    }
}
 