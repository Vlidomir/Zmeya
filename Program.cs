using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main()
    {
        // 1. НАСТРОЙКА КОНСОЛИ
        Console.Title = "Змейка";
        Console.CursorVisible = false;
        Console.SetWindowSize(80, 25);
        Console.SetBufferSize(80, 25);
        
        // 2. ПЕРЕМЕННЫЕ ДЛЯ ЗМЕЙКИ
        int headX = 40;
        int headY = 12;
        int directionX = 1;
        int directionY = 0;
        
        // Список для хранения тела змейки
        List<(int X, int Y)> body = new List<(int X, int Y)>();
        int bodyLength = 3;
        
        // Инициализируем начальное тело
        for (int i = 1; i <= bodyLength; i++)
        {
            body.Add((headX - i, headY));
        }
        
        // 3. ЕДА
        Random random = new Random();
        int foodX, foodY;
        int score = 0;
        
        // Очищаем консоль и рисуем статичные элементы
        Console.Clear();
        DrawBorder();
        
        // Генерируем первую еду
        GenerateFood();
        
        // Рисуем начальное состояние
        DrawSnake();
        DrawFood();
        DrawScore();
        
        // 4. ИГРОВОЙ ЦИКЛ
        while (true)
        {
            // === ЧАСТЬ 1: УПРАВЛЕНИЕ ===
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (directionY != 1) // Нельзя двигаться вниз, если движемся вверх
                        {
                            directionX = 0;
                            directionY = -1;
                        }
                        break;
                        
                    case ConsoleKey.DownArrow:
                        if (directionY != -1) // Нельзя двигаться вверх, если движемся вниз
                        {
                            directionX = 0;
                            directionY = 1;
                        }
                        break;
                        
                    case ConsoleKey.LeftArrow:
                        if (directionX != 1) // Нельзя двигаться вправо, если движемся влево
                        {
                            directionX = -1;
                            directionY = 0;
                        }
                        break;
                        
                    case ConsoleKey.RightArrow:
                        if (directionX != -1) // Нельзя двигаться влево, если движемся вправо
                        {
                            directionX = 1;
                            directionY = 0;
                        }
                        break;
                        
                    case ConsoleKey.Escape:
                        return;
                }
            }
            
            // === ЧАСТЬ 2: ДВИЖЕНИЕ ===
            // Стираем последний сегмент хвоста (если не съели еду)
            bool ate = false;
            
            // Сохраняем старую позицию головы для стирания
            int oldHeadX = headX;
            int oldHeadY = headY;
            
            // Двигаем голову
            headX += directionX;
            headY += directionY;
            
            // Проверка на съедание еды ДО перемещения тела
            if (headX == foodX && headY == foodY)
            {
                ate = true;
                score++;
                bodyLength++;
            }
            
            // Стираем хвост (только если не съели еду)
            if (!ate && body.Count > 0)
            {
                var lastSegment = body[body.Count - 1];
                Console.SetCursorPosition(lastSegment.X, lastSegment.Y);
                Console.Write(' ');
            }
            
            // Сохраняем старую голову в тело
            if (body.Count > 0)
            {
                // Добавляем старую голову в начало тела
                body.Insert(0, (oldHeadX, oldHeadY));
                
                // Если не съели еду, удаляем последний сегмент
                if (!ate && body.Count > bodyLength)
                {
                    body.RemoveAt(body.Count - 1);
                }
                // Если съели, просто увеличиваем длину
                else if (ate)
                {
                    bodyLength++;
                }
            }
            else if (ate)
            {
                // Если тела нет, добавляем первый сегмент
                body.Add((oldHeadX, oldHeadY));
            }
            
            // === ПРОВЕРКА СТОЛКНОВЕНИЙ ===
            // Со стенами
            if (headX <= 0 || headX >= Console.WindowWidth - 1 || 
                headY <= 0 || headY >= Console.WindowHeight - 1)
            {
                GameOver();
                break;
            }
            
            // С самим собой
            bool collision = false;
            foreach (var segment in body)
            {
                if (headX == segment.X && headY == segment.Y)
                {
                    collision = true;
                    break;
                }
            }
            
            if (collision)
            {
                GameOver();
                break;
            }
            
            // === ЧАСТЬ 3: ОТРИСОВКА ===
            // Стираем старую голову
            Console.SetCursorPosition(oldHeadX, oldHeadY);
            // Проверяем, не была ли старая голова частью тела (если да, то рисуем тело)
            bool wasBody = false;
            foreach (var segment in body)
            {
                if (segment.X == oldHeadX && segment.Y == oldHeadY)
                {
                    wasBody = true;
                    break;
                }
            }
            
            if (!wasBody)
            {
                Console.Write(' ');
            }
            else
            {
                Console.Write('o');
            }
            
            // Рисуем новую голову
            Console.SetCursorPosition(headX, headY);
            Console.Write('O');
            
            // Перерисовываем тело (только измененные сегменты)
            for (int i = 0; i < body.Count; i++)
            {
                Console.SetCursorPosition(body[i].X, body[i].Y);
                Console.Write('o');
            }
            
            // Если съели еду, генерируем новую и обновляем счет
            if (ate)
            {
                GenerateFood();
                DrawFood();
                DrawScore();
            }
            
            // === ЧАСТЬ 4: ЗАДЕРЖКА ===
            Thread.Sleep(100);
        }
        
        void GenerateFood()
        {
            bool validPosition;
            do
            {
                validPosition = true;
                foodX = random.Next(1, Console.WindowWidth - 2);
                foodY = random.Next(1, Console.WindowHeight - 2);
                
                // Проверяем, не попала ли еда на змейку
                if (foodX == headX && foodY == headY)
                    validPosition = false;
                
                foreach (var segment in body)
                {
                    if (foodX == segment.X && foodY == segment.Y)
                    {
                        validPosition = false;
                        break;
                    }
                }
            } while (!validPosition);
        }
        
        void DrawBorder()
        {
            // Верхняя и нижняя граница
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.SetCursorPosition(i, 0);
                Console.Write('#');
                
                Console.SetCursorPosition(i, Console.WindowHeight - 1);
                Console.Write('#');
            }
            
            // Левая и правая граница
            for (int i = 1; i < Console.WindowHeight - 1; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write('#');
                
                Console.SetCursorPosition(Console.WindowWidth - 1, i);
                Console.Write('#');
            }
        }
        
        void DrawSnake()
        {
            // Рисуем тело
            foreach (var segment in body)
            {
                Console.SetCursorPosition(segment.X, segment.Y);
                Console.Write('o');
            }
            
            // Рисуем голову
            Console.SetCursorPosition(headX, headY);
            Console.Write('O');
        }
        
        void DrawFood()
        {
            Console.SetCursorPosition(foodX, foodY);
            Console.Write('$');
        }
        
        void DrawScore()
        {
            Console.SetCursorPosition(2, 1);
            Console.Write($"Score: {score}     ");
        }
        
        void GameOver()
        {
            Console.Clear();
            Console.SetCursorPosition(Console.WindowWidth / 2 - 10, Console.WindowHeight / 2);
            Console.WriteLine($"GAME OVER! Score: {score}");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 15, Console.WindowHeight / 2 + 1);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}