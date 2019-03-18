using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoboWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        TimeSpan speedOfGame = TimeSpan.FromMilliseconds(200);      // скорость игры. По уиолчанию - 200 миллисекунд 
        Game g;                                                     // Объект игры    
        bool state = true;                                          // флаг переключатель для остановки и запуска игры

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)// при загрузке формы
        {
            g = new Game(speedOfGame, this);                        // создаётся объект игры
            this.Button_Click((object)this,new RoutedEventArgs());  // идёт запуск игры при помощи имитации нажатия на кнопку
        }

        private void Button_Click(object sender, RoutedEventArgs e) // метод нажания на кнопку
        {
            if (!g.CheckTheGame())                                  // если игра не прошла проверку
            {
                MessageBox.Show("Игра построена неверно. Пожалуйста, сделайте так, чтобы объекты не пересекались между собой.");
                return;                                             // выйти из метода и вернуть соответствующее сообщение
            }
            if (state)                                              // если режим "Готовности перейти в старт"
            {
                g.Start();                                          // запускаем игру
                state = !(state);                                   // переходим в режим "Готовности перейти в стоп"    
            }
            else
            {
                g.Stop();                                           // останавливаем игру
                state = !(state);                                   // переходим в режим "Готовности перейти в старт" 
            }
        }
        /// <summary>
        /// Метод изменения slider'а
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!(g == null))                        // если игра была создана
            {
                if(e.NewValue <= 0)                 // если новое значение, принятое данным методом меньше или равно нулю
                {
                    if (g.IsGameStart())            // если игра запущена
                    {
                        g.Stop();                   // то останавливаем её
                    }
                }
                else                                // если значение, присылаемое от слайдера валидное
                {
                    if (!g.IsGameStart())           // если игра не была запущена
                    {
                        g.Start();                  // то запускаем игру
                    }
                    TimeSpan newSpeed = TimeSpan.FromSeconds(GameObject._speed / e.NewValue);   // определяем новую скорость игры
                    g.ChangeSpeedOfGame(newSpeed);                                              // задаём новую скорость игры    
                }
            }
        }
    }
}
