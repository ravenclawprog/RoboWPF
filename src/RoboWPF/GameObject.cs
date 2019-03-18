using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboWPF
{
    public class GameObject
    {
        private Point _velocity;                                // направление скорости  - вектор с началом в начале координат и концом в точке _velocity
        private Rect _rect;                                     // область,занимаемая танком(прямоугольник)
        private double _mass = 0;                               // масса
        static public int _speed = 1;                           // скорость перемещения за один шаг
        static public int AbsMaxVelocity = 8;                   // максимальное значение для вектора направления скорости

        public Rect Rect_ { get => _rect; set => _rect = value; }// свойство Rect доступно для чтения и записи
        public Point velocity { get => _velocity; }             // свойство направления скорости доступно только для чтения
        
        /// <summary>
        /// Конструктор игрового объекта
        /// </summary>
        /// <param name="r">Прямоугольная область, являющаяся самим объектом</param>
        /// <param name="velocity">Направление скорости</param>
        public GameObject(Rect r = default(Rect),
                   Point velocity = default(Point))
        {
            if (!(r == default(Rect)))                      // если параметр r был передан
            {
                _rect = r;                                  // присвоить этот параметр
            }
            else
            {
                _rect = new Rect();                         // иначе - создаем новый прямоугольник    
            }

            if (!(velocity == default(Point)))              // если начальная скорость была передана
            {
                _velocity = velocity;                       // присвоить значение начальной скорости
                if (_velocity.X > AbsMaxVelocity) _velocity.X = AbsMaxVelocity;     // и проверить её на выход за пределы
                if (_velocity.X < -AbsMaxVelocity) _velocity.X = -AbsMaxVelocity;
                if (_velocity.Y > AbsMaxVelocity) _velocity.Y = AbsMaxVelocity;
                if (_velocity.Y < -AbsMaxVelocity) _velocity.Y = -AbsMaxVelocity;
            }
            else
            {
                _velocity = new Point(0, 0);                // создаем скорость без направления
            }
        }
        /// <summary>
        /// Шаг объекта. Объект делает шаг в направлении вектора _velocity на величину, определяемую скоростью _speed
        /// </summary>
        public void Step()
        {
            Point _newVelocity = new Point(0, 0);
            double currentSpeed = Math.Sqrt(_velocity.X* _velocity.X + _velocity.Y* _velocity.Y);
            _newVelocity.Y = currentSpeed == 0? 0 : (_speed / currentSpeed)*_velocity.Y;
            _newVelocity.X = _velocity.Y == 0 ? 0 : (_velocity.X / _velocity.Y) * _newVelocity.Y;
            this._rect.X += _newVelocity.X;
            this._rect.Y += _newVelocity.Y;
        }
        /// <summary>
        /// Изменяет направление вектора скорости на противоположное относительно начала координат
        /// </summary>
        public void ChangeDirection()
        {
            _velocity.X = -_velocity.X;
            _velocity.Y = -_velocity.Y;
        }
        /// <summary>
        /// Изменяет вектор скорости в соответствии с пересечением с объектом g
        /// Метод не рекомендуется использовать без предварительного определения пересечения текущего объекта с объектом g
        /// </summary>
        /// <param name="g">Объект, относительно которого вычисляется направление вектора скорости</param>
        public void ReflectionDirection(GameObject g)
        {
            /// будем использовать эвристику следующего вида - имеется 4 области и 4 линии. 
            /// В зависимости от того, где будет лежать ближайшая точка относительно прямоугольника g,
            /// Будет применена операция изменения направления (не самый лучший вариант, но все же)
            // Расчет коррдинат центра объекта, с которым сталкиваемся
            Point centerOfg = new Point(0, 0);
            centerOfg.X = (g.Rect_.BottomRight.X + g.Rect_.TopLeft.X) / 2;
            centerOfg.Y = (g.Rect_.BottomRight.Y + g.Rect_.TopLeft.Y) / 2;
            Point controlPoint;
            List<Point> allPoints = new List<Point>();
            allPoints.Add(_rect.TopLeft);
            allPoints.Add(_rect.TopRight);
            allPoints.Add(_rect.BottomLeft);
            allPoints.Add(_rect.BottomRight);
            controlPoint = allPoints[0];
            double distance = SquareDistanceBetweenPoints(controlPoint, centerOfg);     
            // определение точки прямоугольника текущего объекта, которая максимально приближена к центру объекта g
            foreach (Point p in allPoints)
            {
                if (SquareDistanceBetweenPoints(p, centerOfg) < distance)
                {
                    distance = SquareDistanceBetweenPoints(p, centerOfg);
                    controlPoint = p;
                }
            }
            if (controlPoint.Y < g.Rect_.Top)
            {
                if (controlPoint.X > g.Rect_.Left && controlPoint.X < g.Rect_.Right)    // прямоугольник подобрался сверху
                {
                    this._velocity.Y = -this._velocity.Y;
                }
                else
                {
                    this.ChangeDirection();
                }
            }
            else if (controlPoint.Y > g.Rect_.Bottom)
            {
                if (controlPoint.X > g.Rect_.Left && controlPoint.X < g.Rect_.Right)    // прямоугольник подобрался сверху
                {
                    this._velocity.Y = -this._velocity.Y;
                }
                else
                {
                    this.ChangeDirection();
                }
            }
            else
            {
                if (controlPoint.X < g.Rect_.Left || controlPoint.X > g.Rect_.Right)
                {
                    this._velocity.X = -this._velocity.X;
                }
                else
                {
                    ChangeDirection();
                }
            }
        }
        /// <summary>
        /// Функция расчета квадрата евклидова расстояния между двумя точками
        /// </summary>
        /// <param name="p1">Точка номер 1</param>
        /// <param name="p2">Точка номер 2</param>
        /// <returns>Квадрат расстояния в формате double</returns>
        private double SquareDistanceBetweenPoints(Point p1, Point p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }
        /// <summary>
        /// Метод определяет, пересекается ли текущий объект с объектом g
        /// </summary>
        /// <param name="g">Объект с которым может пересекаться текущий объект</param>
        /// <returns>true если объекты пересекаются. False - в противном случае</returns>
        public bool Interaction(GameObject g)
        {
            bool result = _rect.IntersectsWith(g.Rect_);
            return result;
        }
    }
}
