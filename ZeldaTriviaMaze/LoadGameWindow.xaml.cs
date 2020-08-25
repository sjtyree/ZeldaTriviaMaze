/*Spencer Tyree, CSCD 371
 * Final Project
 * 
 * This file contains all of the stuff for the LoadGameWindow
 * 
 * EXTRA CREDIT: Added Zelda song and sound effects
 *              Added keys, chests, enemies, and a dungeon boss.
 */

using System;
using System.IO;
using System.Collections.Generic;
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
    /// Interaction logic for LoadGameWindow.xaml
    /// </summary>
    public partial class LoadGameWindow : Window
    {
        private string filename;
        private string selectedSave=null;
        public LoadGameWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSave == null)
                MessageBox.Show("Please choose a file");
            else//deserialize
            {
                BinaryFormatter bin = new BinaryFormatter();
                using (Stream fstream=File.OpenRead(selectedSave))
                {
                    GameSaveState loadedGame = (GameSaveState)bin.Deserialize(fstream);
                    MainGameWindow newWindow = new MainGameWindow();
                    newWindow.Gamestate = loadedGame;
                    newWindow.Show();
                }
            }
            this.Close();
                
        }

        private void LoadFiles(object sender, RoutedEventArgs e)
        {
            String[] saves = Directory.GetFiles(".", "*.dat");
            foreach(string save in saves)
            {
                ListBoxItem newitem = new ListBoxItem();
                newitem.Selected += Selected_Click;
                newitem.Content = save.Substring(2, save.Length - 6);
                GameSavesListBox.Items.Add(newitem);
            }
        }

        private void Selected_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem name = e.Source as ListBoxItem;
            if (name != null)
            {
                selectedSave = name.Content.ToString()+".dat";
            }
           
        }
    }
}
