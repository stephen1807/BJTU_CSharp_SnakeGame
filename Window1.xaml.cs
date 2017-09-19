using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApplication1
{
    using System.Data;
    using System.Diagnostics;
    using System.Windows.Threading;
    using WpfApplication2;

    public partial class Window1 : Window
    {
        // This list describes the Bonus Red pieces of Food on the Canvas

        private List<Point> bonusPoints = new List<Point>();

        // This list describes the body of the snake on the Canvas
        private List<Point> snakePoints = new List<Point>();

        private Brush snakeColor = Brushes.Green;
        private Brush foodColor = Brushes.Red;
        private enum SIZE
        {
            THIN = 4,
            NORMAL = 6,
            THICK = 8
        };
        private enum MOVINGDIRECTION
        {
            UPWARDS = 8,
            DOWNWARDS = 2,
            TOLEFT = 4,
            TORIGHT = 6
        };

        private TimeSpan HARD = new TimeSpan(10000);
        private TimeSpan MEDIUM = new TimeSpan(300000);
        private TimeSpan EASY = new TimeSpan(500000);



        private Point startingPoint = new Point(100, 100);
        private Point currentPosition = new Point();

        // Movement direction initialisation
        private int direction = 6;

        /* Placeholder for the previous movement direction
         * The snake needs this to avoid its own body.  */
        private int previousDirection = 0;

        /* Here user can change the size of the snake. 
         * Possible sizes are THIN, NORMAL and THICK */
        private int headSize = (int)SIZE.THICK;


        private int length = 100;
        private int score = 0;
        private int score_increment = 10;
        private Random rnd = new Random();

        DispatcherTimer timer = new DispatcherTimer();

        private Stopwatch stopwatch = new Stopwatch();
        private bool isPaused = false;
        private bool isStarted = false;
        private DataTable highScores;
        private HighScoreIO highScoreIO = new HighScoreIO();

        public Window1()
        {
            InitializeComponent();

            loadHighScores();

            timer.Tick += new EventHandler(timer_Tick);

            /* Here user can change the speed of the snake. 
             * Possible difficulty are EASY, MEDIUM, HARD, IMPOSSIBLE */

            paintFood();

            this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
            paintSnake(startingPoint);
            currentPosition = startingPoint;

            timer.Interval = MEDIUM;
        }

        private void paintSnake(Point currentposition)
        {

            /* This method is used to paint a frame of the snake´s body
             * each time it is called. */

            Ellipse newEllipse = new Ellipse();
            newEllipse.Fill = snakeColor;
            newEllipse.Width = headSize;
            newEllipse.Height = headSize;

            Canvas.SetTop(newEllipse, currentposition.Y);
            Canvas.SetLeft(newEllipse, currentposition.X);

            int count = paintCanvas.Children.Count;
            paintCanvas.Children.Add(newEllipse);
            snakePoints.Add(currentposition);


            // Restrict the tail of the snake
            if (count >= length)
            {
                paintCanvas.Children.RemoveAt(count - length + 9);
                snakePoints.RemoveAt(count - length);
            }
        }


        private void paintBonus(int index)
        {
            Point bonusPoint = new Point(rnd.Next(5, 630), rnd.Next(5, 470));

            Ellipse newEllipse = new Ellipse();
            newEllipse.Fill = foodColor;
            newEllipse.Width = headSize;
            newEllipse.Height = headSize;

            Canvas.SetTop(newEllipse, bonusPoint.Y);
            Canvas.SetLeft(newEllipse, bonusPoint.X);
            paintCanvas.Children.Insert(index, newEllipse);
            bonusPoints.Insert(index, bonusPoint);

        }


        private void timer_Tick(object sender, EventArgs e)
        {
            // Expand the body of the snake to the direction of movement

            switch (direction)
            {
                case (int)MOVINGDIRECTION.DOWNWARDS:
                    currentPosition.Y += 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.UPWARDS:
                    currentPosition.Y -= 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.TOLEFT:
                    currentPosition.X -= 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.TORIGHT:
                    currentPosition.X += 1;
                    paintSnake(currentPosition);
                    break;
            }

            // Restrict to boundaries of the Canvas
            if ((currentPosition.X < 0) || (currentPosition.X > paintCanvas.Width - 10) ||
                (currentPosition.Y < 0) || (currentPosition.Y > paintCanvas.Height - 10))
                GameOver();

            // Hitting a bonus Point causes the lengthen-Snake Effect
            int n = 0;
            foreach (Point point in bonusPoints)
            {

                if ((Math.Abs(point.X - currentPosition.X) < headSize) &&
                    (Math.Abs(point.Y - currentPosition.Y) < headSize))
                {
                    length += 10;
                    score += score_increment;
                    ScoreLabel.Text = score.ToString();

                    // In the case of food consumption, erase the food object
                    // from the list of bonuses as well as from the canvas
                    bonusPoints.RemoveAt(n);
                    paintCanvas.Children.RemoveAt(n);
                    paintBonus(n);
                    break;
                }
                n++;
            }

            // Restrict hits to body of Snake

            //TODO
            for (int q = 0; q < (snakePoints.Count - headSize * 2); q++)
            {
                Point point = new Point(snakePoints[q].X, snakePoints[q].Y);
                if ((Math.Abs(point.X - currentPosition.X) < (headSize)) &&
                     (Math.Abs(point.Y - currentPosition.Y) < (headSize)))
                {
                    GameOver();
                    break;
                }

            }
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (previousDirection != (int)MOVINGDIRECTION.UPWARDS && !isPaused)
                        direction = (int)MOVINGDIRECTION.DOWNWARDS;
                    break;
                case Key.Up:
                    if (previousDirection != (int)MOVINGDIRECTION.DOWNWARDS && !isPaused)
                        direction = (int)MOVINGDIRECTION.UPWARDS;
                    break;
                case Key.Left:
                    if (previousDirection != (int)MOVINGDIRECTION.TORIGHT && !isPaused)
                        direction = (int)MOVINGDIRECTION.TOLEFT;
                    break;
                case Key.Right:
                    if (previousDirection != (int)MOVINGDIRECTION.TOLEFT && !isPaused)
                        direction = (int)MOVINGDIRECTION.TORIGHT;
                    break;
                case Key.P:
                    if (!isPaused)
                    {
                        isPaused = true;
                        timer.Stop();
                        stopwatch.Stop();
                    }
                    else
                    {
                        isPaused = false;
                        timer.Start();
                        stopwatch.Start();
                    } break;
                case Key.Enter:
                    if (!isStarted)
                    {
                        StartLabel.Visibility = Visibility.Collapsed;
                        timer.Start();
                        stopwatch.Start();
                    }
                    break;
            }
            previousDirection = direction;
        }

        private void repaintFood()
        {
            for (int k = 0; k < 10; k++)
            {
                try
                {
                    bonusPoints.RemoveAt(k);
                    paintCanvas.Children.RemoveAt(k);
                    paintBonus(k);
                }
                catch (System.ArgumentOutOfRangeException) { }
            }

        }

        private void paintFood()
        {
            for (int n = 0; n < 10; n++)
            {
                paintBonus(n);
            }
        }

        private void GameOver()
        {
            timer.Stop();
            stopwatch.Stop();

            //Show name input dialog
            AddName ad = new AddName();
            ad.ShowDialog();

            string name;

            //Check if any string entered
            if (ad.ResponseText.Length > 0)
            {
                name = ad.ResponseText.Trim();
            }
            else
            {
                name = "No name";
            }
            //Write to file
            highScoreIO.writeHighScore(new HighScore(name, score, (float)stopwatch.Elapsed.TotalSeconds));

            MessageBoxResult dialogResult = MessageBox.Show("You Lose! Score: " + score.ToString() + "\nTime: " + stopwatch.Elapsed.TotalSeconds.ToString("n2") + " secs" + "\nPlay again?", "Game Over", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            //Check if want to continue
            if (dialogResult == MessageBoxResult.Yes)
            {
                Restart();
            }
            else if (dialogResult == MessageBoxResult.No)
            {
                this.Close();
            }
        }

        private void Restart()
        {
            //Restart variables
            length = 100;
            direction = 6;
            score = 0;

            //Restart stopwatch
            stopwatch.Reset();

            //Remove points
            snakePoints = new List<Point>();
            paintCanvas.Children.RemoveRange(0, paintCanvas.Children.Capacity);

            //Repaint snake & bonus
            Point newStartingPoint = new Point(rnd.Next(50, (int)paintCanvas.Width - 50), rnd.Next(50, (int)paintCanvas.Height - 50));
            paintSnake(newStartingPoint);
            currentPosition = newStartingPoint;
            paintFood();

            //Reload high scores
            loadHighScores();

            isStarted = false;
            StartLabel.Visibility = Visibility.Visible;
        }

        private void loadHighScores()
        {
            highScores = highScoreIO.readHighScoresAsDataTable();
            HighScoreListView.DataContext = highScores;
            HighScoreGridView.Columns.Clear();

            foreach (DataColumn column in highScores.Columns)
            {
                GridViewColumn gvcolumn = new GridViewColumn();
                gvcolumn.DisplayMemberBinding = new Binding(column.ColumnName);
                gvcolumn.Header = column.ColumnName;
                HighScoreGridView.Columns.Add(gvcolumn);
            }

            HighScoreListView.ItemsSource = highScores.DefaultView;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AboutTeamDialog_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Developed by:" + "\n12304011 - Stephen" + "\n12304007 - Vincent" + "\n12304010 - Mario" + "\n12304015 - Reza" + "\n12304017 - Eka", "Aboout Team", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region color setting
        // Change Background Color function
        private void RedBG_Click(object sender, RoutedEventArgs e)
        {
            if(foodColor!=Brushes.Red && snakeColor!=Brushes.Red)
            paintCanvas.Background = System.Windows.Media.Brushes.OrangeRed;
            
        }
        private void BlueBG_Click(object sender, RoutedEventArgs e)
        {
            if (foodColor != Brushes.Aqua && snakeColor != Brushes.Blue)
            paintCanvas.Background = System.Windows.Media.Brushes.Aqua;
        }
        private void GreenBG_Click(object sender, RoutedEventArgs e)
        {
           if (foodColor != Brushes.Green && snakeColor != Brushes.Green)
           paintCanvas.Background = System.Windows.Media.Brushes.LightGreen;
        }
        private void YellowBG_Click(object sender, RoutedEventArgs e)
        {
            if (foodColor != Brushes.Yellow && snakeColor != Brushes.Yellow)
            paintCanvas.Background = System.Windows.Media.Brushes.Yellow;
        }
        private void BlackBG_Click(object sender, RoutedEventArgs e)
        {
            if (foodColor != Brushes.Black && snakeColor != Brushes.Black)
            paintCanvas.Background = System.Windows.Media.Brushes.Black;
        }
        private void WhiteBG_Click(object sender, RoutedEventArgs e)
        {
            if (foodColor != Brushes.White && snakeColor != Brushes.White)
            paintCanvas.Background = System.Windows.Media.Brushes.White;
        }
        private void PurpleBG_Click(object sender, RoutedEventArgs e)
        {
            if (foodColor != Brushes.Purple && snakeColor != Brushes.Purple)
            paintCanvas.Background = System.Windows.Media.Brushes.Purple;
        }

        // Function to Change Food's Color
        private void RedFood_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Red)
            {
                foodColor = Brushes.Red;
                repaintFood();
            }
        }
        private void BlueFood_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Aqua)
            {
                foodColor = Brushes.Aqua;
                repaintFood();
            }

        }
        private void GreenFood_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Green)
            {
                foodColor = Brushes.Green;
                repaintFood();
            }

        }
        private void YellowFood_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Yellow)
            {
                foodColor = Brushes.Yellow;
                repaintFood();
            }

        }
        private void BlackFood_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Black)
            {
                foodColor = Brushes.Black;
                repaintFood();
            }
        }
        private void WhiteFood_Click(object sender, RoutedEventArgs e)
        {

            if (paintCanvas.Background != Brushes.White)
            {
                foodColor = Brushes.White;
                repaintFood();
            }

        }
        private void PurpleFood_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Purple)
            {
                foodColor = Brushes.Purple;
                repaintFood();
            }
        }

        // Function to Change Snakes's Color
        private void RedSnake_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.OrangeRed)
            {
                snakeColor = Brushes.Red;
                paintSnake(currentPosition);
            }
        }
        private void BlueSnake_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Aqua)
            {
                snakeColor = Brushes.Blue;
                paintSnake(currentPosition);
            }
        }
        private void GreenSnake_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.LightGreen)
            {
                snakeColor = Brushes.Green;
                paintSnake(currentPosition);
            }
        }
        private void YellowSnake_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Yellow)
            {
                snakeColor = Brushes.Yellow;
                paintSnake(currentPosition);
            }
        }
        private void BlackSnake_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Black)
            {
                snakeColor = Brushes.Black;
                paintSnake(currentPosition);
            }
        }
        private void WhiteSnake_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.White)
            {
                snakeColor = Brushes.White;
                paintSnake(currentPosition);
            }
        }
        private void PurpleSnake_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Background != Brushes.Purple)
            {
                snakeColor = Brushes.Purple;
                paintSnake(currentPosition);
            }
        }

        #endregion

        #region difficulty change
        //Change Difficulty
        private void Easy_Click(object sender, RoutedEventArgs e)
        {
            timer.Interval = EASY;
            score_increment = 5;
        }
        private void Medium_Click(object sender, RoutedEventArgs e)
        {
            timer.Interval = MEDIUM;
            score_increment = 10;
        }
        private void Hard_Click(object sender, RoutedEventArgs e)
        {
            timer.Interval = HARD;
            score_increment = 20;
        }
        #endregion
    }
}

