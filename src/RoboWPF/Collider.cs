using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboWPF
{
    class Collider
    {
        List<GameObject> gameObjects = new List<GameObject>();              // список объектов игры
        List<List<GameObject>> collisions = new List<List<GameObject>>();   // коллизии - список списков объектов игры. Каждый список определяет, с какими из объектов столкнулся объект, который имеет тот же индекс, что и индекс элемента списка.
        int CountOfCollisions = 0;                                          // количество коллизий    
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="objects">объекты игры</param>
        public Collider(List<GameObject> objects = default(List<GameObject>))
        {
            gameObjects = objects;
        }
        /// <summary>
        ///  Распознать все коллизии среди объектов gameObjects
        /// </summary>
        private void DetectCollisions()
        {
            collisions.Clear();
            CountOfCollisions = 0;
            for( int i = 0; i < gameObjects.Count; i++)
            {
                List<GameObject> bufferList = new List<GameObject>();
                collisions.Add(new List<GameObject>());
                for(int j = 0; j < gameObjects.Count; j++)
                {
                    if (j != i)
                    {
                        if (gameObjects[i].Interaction(gameObjects[j]))
                        {
                            bufferList.Add(gameObjects[j]);
                            CountOfCollisions++;
                        }
                    }
                }
                collisions[i] = bufferList;
            }
        }
        private void DetectCollisions(int index)
        {
            CountOfCollisions -= collisions[index].Count;
            collisions[index].Clear();
            List<GameObject> bufferList = new List<GameObject>();
            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (i != index)
                {
                    if (gameObjects[index].Interaction(gameObjects[i]))
                    {
                        bufferList.Add(gameObjects[i]);
                        CountOfCollisions++;
                    }
                }                
            }
            collisions[index] = bufferList;
        }
        /// <summary>
        /// Шаг в сторону обратную стороне вектора скорости с возвратом вектора скорости в исходное положение
        /// </summary>
        /// <param name="gameObject">Объект, который необходимо подвинуть</param>
        private void StepBehind(GameObject gameObject)
        {
            gameObject.ChangeDirection();
            gameObject.Step();
            gameObject.ChangeDirection();
        }
        /// <summary>
        /// Шаг вперёд для объекта по направлению его вектора скорости
        /// </summary>
        /// <param name="gameObject">Объект, который необходимо передвинуть</param>
        private void StepForward(GameObject gameObject)
        {
            gameObject.Step();
        }
        /// <summary>
        /// Разрешить коллизию
        /// </summary>
        /// <param name="i"> номер коллизии в списке collisions</param>
        private void Uncollide(int i)
        {
            int oldSpeed = GameObject._speed;
            GameObject._speed = 1;
            List<GameObject> bufferList = new List<GameObject>();
            bufferList = collisions[i];
            // передвигаем основной объект на шаг назадS
            StepBehind(gameObjects[i]);
            // проверяем, исчезла ли при это коллизия
            DetectCollisions();
            if(collisions[i].Count != 0)
            {
                StepForward(gameObjects[i]);
            }
            //если коллизия не исчезла
            while(collisions[i].Count != 0)
            {
                bufferList = collisions[i];
                // сохраняем предыдущее значение коллизий
                int oldCollision = CountOfCollisions;
                // делаем шаг назад для каждого из объектов (включая все объекты коллизии)
                foreach (GameObject g in bufferList)
                {
                    StepBehind(g);
                }
                StepBehind(gameObjects[i]);
                // повторная проверка на отсутствие коллизий
                DetectCollisions();
                // если после разрешения коллизии мы создали ещё больше коллизий
                if (CountOfCollisions > oldCollision)
                {
                    // то отменяем наши действия
                    foreach (GameObject g in bufferList)
                    {
                        StepForward(g);
                    }
                    StepForward(gameObjects[i]);
                    GameObject._speed = oldSpeed;
                    // и выходим из функции
                    return;
                }
            }
            // если нам удалось разрешить коллизию
            // то теперь наши объекты будут двигаться с новым вектором
            foreach (GameObject g in bufferList)
            {
                    g.ReflectionDirection(gameObjects[i]);
            }
            // а сам наш объект также будет впредь двигаться по-другому
            if (bufferList.Count != 0)
                gameObjects[i].ReflectionDirection(bufferList[0]);
            GameObject._speed = oldSpeed;
        }
        /// <summary>
        /// Найти коллизию, которая имеет максимальное количество объектов
        /// </summary>
        /// <returns>вовзвращает номер списка в списке collisions, который имеет максимальную длину</returns>
        private int FindMaxCollision()
        {
            int result = 0;
            int maxCount = 0;
            for (int i = 0; i < collisions.Count; i++)
            {
                if(collisions[i].Count > maxCount)
                {
                    maxCount = collisions[i].Count;
                    result = i;
                }
            }
            return result;
        }
        /// <summary>
        ///  Устанавливает в качестве объектов список объектов, передаваемый в качестве параметра
        /// </summary>
        /// <param name="a"> список объектов, который необходимо присвоить </param>
        public void SetObjects(List<GameObject> a)
        {
            gameObjects = a;
        }
        public void Work()
        {
            DetectCollisions();
            while (CountOfCollisions != 0)
            {
                // сохраняем предыдущее значение коллизий
                int oldCollision = CountOfCollisions;
                // пробуем развернуть максимальную коллизию
                Uncollide(FindMaxCollision());
                // если улучшений нет по результатам наших действий, то выходим
                if(oldCollision <= CountOfCollisions)
                {
                    return;
                }
            }
        }
    }
}
