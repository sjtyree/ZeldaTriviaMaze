using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZeldaTriviaMaze
{
    public partial class NewGameForm : Form
    {
        public NewGameForm()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if(NameTextBox.Text=="")
            {
                MessageBox.Show("Please enter in a name");
            }
                //this will probably never run because either one has to always be checked, but just leaving it to be safe
            else if (EasyRadioButton.Checked==false && HardRadioButton.Checked==false)
            {
                MessageBox.Show("Please select a difficulty");
            }
            else
            {
                string diff;
                if (EasyRadioButton.Checked==true)
                    diff="easy";
                else
                    diff="hard";
                GameSaveState playergame = new GameSaveState(NameTextBox.Text, diff);
            }
        }
    }
}
