using System;
using System.Timers;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboWPF
{
    public class Game
    {
        private DispatcherTimer _gameTimer = new DispatcherTimer();                 // Таймер для запуска основных вычислений игры   
        private TimeSpan _stepOfGame;                                               // Время, через которое такт игры будет повторяться    

        private GameObject Ground = new GameObject();                               // Поверхность - определяет выход за границы игрового поля - может быть заменена на 4 элемента (deprecated)

        private GameObject UpGround = new GameObject();                             // Верхняя граница игрового поля
        private GameObject DownGround = new GameObject();                           // Нижняя граница игрового поля
        private GameObject LeftGround = new GameObject();                           // Левая граница игрового поля
        private GameObject RightGround = new GameObject();                          // Правая граница игрового поля

        private List<GameObject> Obstacle = new List<GameObject>();                 // Список препятствий
        private List<GameObject> Tanks = new List<GameObject>();                    // Список танков

        private Collider collider = new Collider();                                 // Обработчик коллизий (столкновений)

        private List<Button> ObstacleButton = new List<Button>();                   // Список кнопок для считывания положения и записи нового положения
        private List<TextBlock> TanksTextBlock = new List<TextBlock>();             // Список текстовых блоков, удовлетворяющих требованию
        private MainWindow handleW = default(MainWindow);                           // Окно, которое является игровым полем
        /// <summary>
        ///  Конструктор игры. Задаёт начальные параметры, заполняет списки препятствий, танков, границ полей, такт и прочее
        /// </summary>
        /// <param name="msec">время одного такта в миллисекундах</param>
        /// <param name="h">окно, которое представляет собой игровое поле</param>
        /// <param name="_obst">список препятствий (если таковой имеется), хотя теперь уже не нужен</param>
        /// <param name="_tanks">список танков (если таковой имеется), хотя теперь уже не нужен</param>
        public Game(TimeSpan msec = default(TimeSpan), MainWindow h = default(MainWindow),
            List<GameObject> _obst = default(List<GameObject>),
            List<GameObject> _tanks = default(List<GameObject>))
        {
            _stepOfGame = msec == default(TimeSpan) ? TimeSpan.FromMilliseconds(1000): msec;
            Obstacle = _obst == default(List<GameObject>) ? new List<GameObject>() : _obst;
            Tanks = _tanks == default(List<GameObject>) ? new List<GameObject>() : _obst;
            LoadObjectsFromForm(h);
        }
        /// <summary>
        /// Запускает игру. Запускает таймер и устанавливает частоту вызова такта
        /// </summary>
        public void Start()
        {
            _gameTimer = new DispatcherTimer();
            _gameTimer.Tick += new EventHandler(OnTimerEvent);
            _gameTimer.Interval = _stepOfGame;
            _gameTimer.Start();
        }
        /// <summary>
        /// Метод для определения, запущен ли таймер игры
        /// </summary>
        /// <returns>возвращает true, если таймер запущен. False - в противном случае </returns>
        public bool IsGameStart()
        {
            return _gameTimer.IsEnabled;
        }
        /// <summary>
        /// Метод изменяет частоту запуска такта игры
        /// </summary>
        /// <param name="newT">Новое время запуска в формате TimeSpan</param>
        public void ChangeSpeedOfGame(TimeSpan newT)
        {
            _stepOfGame = newT;
            _gameTimer.Interval = _stepOfGame;
        }
        /// <summary>
        /// Останавливает игры. Происходит остановка таймера
        /// </summary>
        public void Stop()
        {
            _gameTimer.Stop();
        }
        /// <summary>
        /// Работа таймера
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void OnTimerEvent(object source, EventArgs e)
        {
            try
            {
                this.ReadPositionFromForm();                    // считывание данных с формы
                this.Step();                                    // выполнение шага игры
                this.WritePositionToForm();                     // запись результата на форму
            }
            catch (Exception ee)                                // если поймали исключение,то
            {
                MessageBox.Show(ee.ToString());                 // показываем его
            }                                                   // TODO: заменить на логирование
        }
        /// <summary>
        /// Основной такт игры
        /// </summary>
        public void Step()
        {
            for (int i=0; i < Tanks.Count(); i++)               // производим шаг каждого танка
            {
                Tanks[i].Step();
            }
            List<GameObject> allObjects = new List<GameObject>  // составляем список всех объектов для передачи коллайдеру    
            {
                UpGround,
                DownGround,
                LeftGround,
                RightGround
            };
            allObjects.AddRange(Tanks);
            allObjects.AddRange(Obstacle);
            collider.SetObjects(allObjects);                    // установка списка объектов для распознавания коллизий
            collider.Work();                                    // обработка коллизий
        }
        /// <summary>
        /// Функция считывает позиции с формы и заносит в поля объекта игры, обновляя информацию
        /// </summary>
        private void ReadPositionFromForm()
        {
                lock (handleW)
                {
                Ground.Rect_ = new Rect(0, 0,
                               handleW.grid.ActualWidth,
                               handleW.grid.ActualHeight);
                UpGround.Rect_ = new Rect(0, -2 * GameObject._speed,
                handleW.grid.ActualWidth, 0);
                DownGround.Rect_ =new Rect(0, handleW.grid.ActualHeight,
                               handleW.grid.ActualWidth, 2 * GameObject._speed + handleW.grid.ActualHeight);
                LeftGround.Rect_ = new Rect(-2 * GameObject._speed, 0,
                               0, handleW.grid.ActualHeight);
                RightGround.Rect_ = new Rect(handleW.grid.ActualWidth, 0,
                               handleW.grid.ActualWidth + 2 * GameObject._speed, handleW.grid.ActualHeight-2);
                }
                for (int i = 0; i < Tanks.Count(); i++)
                {
                    lock (TanksTextBlock[i])
                    {
                        TextBlock t = TanksTextBlock[i];
                        Tanks[i].Rect_ = new Rect(t.Margin.Left, t.Margin.Top,
                                    t.ActualWidth, t.ActualHeight);
                    }
                }
                for (int i = 0; i < Obstacle.Count(); i++)
                {
                    lock (ObstacleButton[i])
                    {
                        Button b = ObstacleButton[i];
                        Obstacle[i].Rect_ = new Rect(b.Margin.Left, b.Margin.Top,
                                b.ActualWidth, b.ActualHeight);
                    }
                }
        }
        /// <summary>
        /// Запись содержимого полей списков объекта игра на форму
        /// </summary>
        private void WritePositionToForm()
        {
            for (int i = 0; i < TanksTextBlock.Count(); i++)
            {
                TanksTextBlock[i].Margin = new Thickness()
                {
                    Left = Tanks[i].Rect_.X,
                    Top = Tanks[i].Rect_.Y,
                    Bottom = 0,
                    Right = 0
                };
            }
            for (int i = 0; i < Obstacle.Count(); i++)
            {
                ObstacleButton[i].Margin = new Thickness()
                {
                    Left = Obstacle[i].Rect_.X,
                    Top = Obstacle[i].Rect_.Y,
                    Bottom = 0,
                    Right = 0
                };
            }
        }
        /// <summary>
        /// Функция проверяет корректность положения объектов на форме
        /// </summary>
        /// <returns>возвращает true, если объекты расположены на форме корректно.</returns>
        public bool CheckTheGame()
        {
            List<GameObject> allObject = new List<GameObject>();        // TODO: заменить на collider
            allObject.AddRange(Tanks);
            allObject.AddRange(Obstacle);
            for (int i = 0; i < allObject.Count(); i++)
            {
                for (int j = i + 1; j < allObject.Count(); j++)
                {
                    if (allObject[i].Interaction(allObject[j]))
                    {
                        return false;
                    }
                }
                if (!Ground.Rect_.Contains(allObject[i].Rect_))
                {
                    return false;
                }
            }
            return allObject.Count == 0 ? false : true;
        }
        /// <summary>
        /// Загружает объекты с формы, заполняя при этом поля объекта игры
        /// </summary>
        /// <param name="w">форма</param>
        public void LoadObjectsFromForm(MainWindow w)
        {
            Obstacle.Clear();
            Tanks.Clear();
            ObstacleButton.Clear();
            TanksTextBlock.Clear();
            handleW = w;
            Random rnd = new Random((int)DateTime.Now.Ticks);
            Ground = new GameObject(new Rect(0, 0,
                            w.grid.ActualWidth, w.grid.ActualHeight));

            UpGround = new GameObject(new Rect(0, -2 * GameObject._speed,
                           w.grid.ActualWidth, 0));
            DownGround = new GameObject(new Rect(0, w.grid.ActualHeight,
                           w.grid.ActualWidth, 2 * GameObject._speed + w.grid.ActualHeight));
            LeftGround = new GameObject(new Rect(-2 * GameObject._speed, 0,
                           0, w.grid.ActualHeight));
            RightGround = new GameObject(new Rect(w.grid.ActualWidth, 0,
                           w.grid.ActualWidth + 2 * GameObject._speed, w.grid.ActualHeight-2));
            foreach (FrameworkElement fe in w.grid.Children)
            {
                if (fe is TextBlock)
                {
                    double rndx = rnd.Next(-GameObject.AbsMaxVelocity, GameObject.AbsMaxVelocity);
                    double rndy = rnd.Next(-GameObject.AbsMaxVelocity, GameObject.AbsMaxVelocity);
                    TextBlock t = (TextBlock)fe;
                    if (t.Text == "T")
                    {
                        Tanks.Add(new GameObject(new Rect(t.Margin.Left, t.Margin.Top,
                            t.ActualWidth, t.ActualHeight), new Point()
                            {
                                X = rndx > 0 ? rndx+=2 : rndx-=2,
                                Y = rndy > 0 ? rndy+=2 : rndy-=2
                            }));
                        TanksTextBlock.Add(t);
                    }
                    
                }
                else if (fe is Button)
                {
                    Button b = (Button)fe;
                    GameObject _gameObject = new GameObject(new Rect(b.Margin.Left, b.Margin.Top, 
                        b.ActualWidth,b.ActualHeight));
                    Obstacle.Add(_gameObject);
                    ObstacleButton.Add(b);
                }
                
            }
        }
    }
}
