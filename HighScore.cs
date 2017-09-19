using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WpfApplication2
{
    class HighScore
    {
        private string _name;
        private int _score;
        private float _time;

        public string Name { get { return _name; } set { _name = value; } }
        public int Score { get { return _score; } set { _score = value; } }
        public float Time { get { return _time; } set { _time = value; } }

        public HighScore(string name, int score, float time)
        {
            _name = name;
            _score = score;
            _time = time;
        }
    }

    class HighScoreIO
    {
        public List<HighScore> readHighScores()
        {
            List<HighScore> result = new List<HighScore>();

            string line;
            System.IO.StreamReader file;
            try
            {
                file = new System.IO.StreamReader("highscore.txt");
            }
            catch (System.IO.FileNotFoundException) { return result; }
            while ((line = file.ReadLine()) != null)
            {
                //Format
                string[] fields = line.Split(new string[] { "###" }, StringSplitOptions.None);
                result.Add(new HighScore(fields[0], int.Parse(fields[1]), float.Parse(fields[2])));
            }


            file.Close();

            //Sort desc highscores
            result.Sort(delegate(HighScore a, HighScore b) { return -1 * a.Score.CompareTo(b.Score); });

            return result;
        }

        public void writeHighScore(HighScore newHighScore)
        {
            string line = newHighScore.Name + "###" + newHighScore.Score + "###" + newHighScore.Time;

            System.IO.StreamWriter file = new System.IO.StreamWriter("highscore.txt", true);
            file.WriteLine(line);

            file.Close();
        }

        public DataTable readHighScoresAsDataTable()
        {
            return ToDataTable<HighScore>(readHighScores());
        }

        private DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}
