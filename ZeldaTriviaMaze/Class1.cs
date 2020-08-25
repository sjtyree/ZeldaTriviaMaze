using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaTriviaMaze
{
    [Serializable]
    class GameSaveState
    {
        private string name;
        private string difficulty;
        private int life;
        public int Life { get { return life; } set { life = value; } }
        private int[,] position;

        public GameSaveState(string name, string difficulty)
        {
            this.name = name;
            this.difficulty = difficulty;
            this.life = 3;
            this.position=new int[,]{{1,0,0,0},
                                     {0,0,0,0},
                                     {0,0,0,0},
                                     {0,0,0,0}};

        }
    }
}
