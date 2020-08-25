/*Spencer Tyree, CSCD 371
 * Final Project
 * 
 * This file contains all of the main code and event handlers for the mainwindow, which is basically the title screen.
 * 
 * EXTRA CREDIT: Added Zelda song and sound effects
 *              Added keys, chests, enemies, and a dungeon boss.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZeldaTriviaMaze
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ExitGameButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            NewGameForm newgame = new NewGameForm();
            newgame.Show();
        }


        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            LoadGameWindow loadtehgame = new LoadGameWindow();
            loadtehgame.ShowDialog();
        }



        private void PlaySong(object sender, RoutedEventArgs e)
        {
            ZeldaThemeMediaElement.Play();
        }

       

       

       
        
    }

}
