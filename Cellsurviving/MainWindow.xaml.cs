using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cellsurviving
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Timers.Timer timer;

        public static MainWindow instance;

        public List<Ellipse> zombies = new List<Ellipse>();

        public Random random = new Random();

        bool gameStopped = false;

        double zombieSize = 20;
        int maxZombies = 50;

        double characterSize = 20;

        double lifetime = 0;

        /// <summary>
        /// Percentage of the chance to spawn a zombie
        /// </summary>
        double zombieSpawnChance = 3;

        double maxMovePixelsLifetimeUsage = 1;

        public MainWindow()
        {
            InitializeComponent();
            restoreElements();

            instance = this;
            timer = new System.Timers.Timer(50);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void restoreElements()
        {
            character.Height = characterSize;
            character.Width = characterSize;
            character.Margin = new Thickness((playgrid.DesiredSize.Width / 2) - (characterSize/2), (playgrid.DesiredSize.Height / 2) - (characterSize / 2), 0, 0);
            liveBar.Value = 100;
            foreach (var zombie in zombies)
            {
                playgrid.Children.Remove(zombie);
            }
            zombies = new List<Ellipse>();
            random = new Random();
            lifetime = 0;
            endgrid.Visibility = Visibility.Collapsed;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int randomInt = instance.random.Next((int)(1/(zombieSpawnChance/100)) - 1);
            if (randomInt == 0)
            {
                if (instance.zombies.Count < maxZombies)
                {
                    instance.Dispatcher.Invoke(() =>
                    {
                        var ellipse = getNewZombie();
                        playgrid.Children.Add(ellipse);
                        zombies.Add(ellipse);
                    });
                }
            }

            Parallel.ForEach(zombies, (zombie) =>
            {
                instance.Dispatcher.Invoke(() =>
                {
                    var thickness = new Thickness(zombie.Margin.Left, zombie.Margin.Top, zombie.Margin.Right, zombie.Margin.Bottom);
                    var rndLeft = (instance.random.NextDouble() * 2 - 1) * (lifetime / maxMovePixelsLifetimeUsage);
                    var newLeft = thickness.Left + rndLeft;
                    if (newLeft > grid.DesiredSize.Width || newLeft < 0)
                    {
                        newLeft = thickness.Left;
                    }
                    thickness.Left = newLeft;

                    var rndTop = (instance.random.NextDouble() * 2 - 1) * (lifetime / maxMovePixelsLifetimeUsage);
                    var newTop = thickness.Top + rndTop;
                    if (newTop > grid.DesiredSize.Height || newTop < 0)
                    {
                        newTop = thickness.Top;
                    }
                    thickness.Top = newTop;
                    zombie.Margin = thickness;
                });
            });

            Parallel.Invoke(() =>
            {
                instance.Dispatcher.Invoke(() =>
                {
                    var zombiecount = getCollidingZombies();
                    if (zombiecount > 0)
                    {
                        liveBar.Value = liveBar.Value - zombiecount / 2d;
                    }
                });
            });

            lifetime += 0.05;
        }

        public Ellipse getNewZombie()
        {
            var ellipse = new Ellipse()
            {
                Fill = Brushes.Green,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = zombieSize,
                Width = zombieSize,
                Margin = new Thickness(50, 50, 0, 0)
            };
            return ellipse;
        }

        public int getCollidingZombies()
        {
            int zombiecount = 0;
            var charPoint = character.TranslatePoint(new Point(0, 0), playgrid);
            foreach (var zombie in zombies)
            {
                var zombiePoint = zombie.TranslatePoint(new Point(0, 0), playgrid);
                double distance = Math.Sqrt(Math.Pow(zombiePoint.X - charPoint.X, 2) + Math.Pow(zombiePoint.Y - charPoint.Y, 2));
                
                if (distance < (zombieSize/2 + characterSize/2))
                {
                    zombiecount++;
                    zombie.Fill = Brushes.Red;
                }
                else
                {
                    zombie.Fill = Brushes.Green;
                }
            }

            return zombiecount;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            timer = null;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (gameStopped) return;
            var point = e.GetPosition(playgrid);
            character.Margin = new Thickness(point.X - 10, point.Y - 10, 0, 0);
        }

        private void liveBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue > 0) return;
            if (gameStopped) return;

            timer.Stop();
            gameStopped = true;
            endgrid_title.Content = "You died!";
            endgrid_text.Content = "You survived for " + Math.Round(lifetime, 2) + " seconds";
            endgrid.Visibility = Visibility.Visible;
        }

        private void endgrid_restart_Click(object sender, RoutedEventArgs e)
        {
            restoreElements();
            gameStopped = false;
            timer.Start();
        }

        private void endgrid_close_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            Application.Current.Shutdown();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }
    }

}
