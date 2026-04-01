using System;
using System.Collections.Generic;
using System.Threading;

enum FoodType
{
    Normal,   // обычная: +1 очко, +1 длина
    Golden,   // золотая: +5 очков, +1 длина
    Rotten    // червивая: -1 очко, -1 длина (но не менее 3 сегментов)
}

class Program
{
    static void Main()
    {
        // Настройка консоли
        Console.Title = "Змейка";
        Console.CursorVisible = false;
        Console.SetWindowSize(80, 25);
        Console.SetBufferSize(80, 25);

        // Переменные змейки
        int headX = 40;
        int headY = 12;
        int directionX = 1;
        int directionY = 0;
        List<(int X, int Y)> body = new List<(int X, int Y)>();
        int bodyLength = 3;
        for (int i = 1; i <= bodyLength; i++)
            body.Add((headX - i, headY));

        // Еда
        Random random = new Random();
        int foodX = 0, foodY = 0;
        FoodType currentFoodType;
        int score = 0;

        // Препятствия
        List<(int X, int Y)> obstacles = new List<(int X, int Y)>();
        const int obstacleCount = 5;

        // Скорость
        int delay = 100;
        const int minDelay = 30;

        // Отрисовка статики
        Console.Clear();
        DrawBorder();
        GenerateObstacles();
        GenerateFood();
        DrawSnake();
        DrawFood();
        DrawScore();

        // Игровой цикл
        while (true)
        {
            // Управление
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (directionY != 1) { directionX = 0; directionY = -1; }
                        break;
                    case ConsoleKey.DownArrow:
                        if (directionY != -1) { directionX = 0; directionY = 1; }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (directionX != 1) { directionX = -1; directionY = 0; }
                        break;
                    case ConsoleKey.RightArrow:
                        if (directionX != -1) { directionX = 1; directionY = 0; }
                        break;
                    case ConsoleKey.Escape:
                        return;
                }
            }

            // Движение
            bool ate = false;
            int oldHeadX = headX, oldHeadY = headY;
            headX += directionX;
            headY += directionY;

            // Проверка съедания еды
            if (headX == foodX && headY == foodY)
            {
                ate = true;
                switch (currentFoodType)
                {
                    case FoodType.Normal:
                        score++;
                        bodyLength++;
                        break;
                    case FoodType.Golden:
                        score += 5;
                        bodyLength++;
                        break;
                    case FoodType.Rotten:
                        score--;
                        bodyLength = Math.Max(3, bodyLength - 1);
                        break;
                }
                DrawScore();
                GenerateFood();
                DrawFood();
            }

            // Стираем хвост (если не съели еду)
            if (!ate && body.Count > 0)
            {
                var last = body[body.Count - 1];
                Console.SetCursorPosition(last.X, last.Y);
                if (IsBorder(last.X, last.Y))
                    Console.Write('#');
                else
                    Console.Write(' ');
            }

            // Обновляем тело
            if (body.Count > 0)
            {
                body.Insert(0, (oldHeadX, oldHeadY));
                if (!ate && body.Count > bodyLength)
                    body.RemoveAt(body.Count - 1);
                else if (ate)
                    bodyLength++;
            }
            else if (ate)
            {
                body.Add((oldHeadX, oldHeadY));
            }

            // Проверка столкновений
            if (headX <= 0 || headX >= Console.WindowWidth - 1 ||
                headY <= 0 || headY >= Console.WindowHeight - 1)
            {
                GameOver();
                break;
            }

            foreach (var obs in obstacles)
                if (headX == obs.X && headY == obs.Y)
                {
                    GameOver();
                    break;
                }

            bool collision = false;
            foreach (var seg in body)
                if (headX == seg.X && headY == seg.Y)
                {
                    collision = true;
                    break;
                }
            if (collision)
            {
                GameOver();
                break;
            }

            // Отрисовка
            // Стираем старую голову
            bool wasBody = false;
            foreach (var seg in body)
                if (seg.X == oldHeadX && seg.Y == oldHeadY) { wasBody = true; break; }
            Console.SetCursorPosition(oldHeadX, oldHeadY);
            if (IsBorder(oldHeadX, oldHeadY))
                Console.Write('#');
            else
                Console.Write(wasBody ? 'o' : ' ');

            // Рисуем новую голову
            Console.SetCursorPosition(headX, headY);
            Console.Write('O');

            // Перерисовываем тело
            foreach (var seg in body)
            {
                Console.SetCursorPosition(seg.X, seg.Y);
                Console.Write('o');
            }

            // Управление скоростью
            int targetDelay = Math.Max(minDelay, 100 - (score / 5) * 5);
            if (delay > targetDelay) delay = targetDelay;

            Thread.Sleep(delay);
        }

        // ---- Вспомогательные методы ----

        bool IsBorder(int x, int y)
        {
            return x == 0 || x == Console.WindowWidth - 1 || y == 0 || y == Console.WindowHeight - 1;
        }

        void GenerateObstacles()
        {
            obstacles.Clear();
            for (int i = 0; i < obstacleCount; i++)
            {
                bool valid;
                do
                {
                    valid = true;
                    int x = random.Next(1, Console.WindowWidth - 2);
                    int y = random.Next(1, Console.WindowHeight - 2);

                    if (x == headX && y == headY) valid = false;
                    foreach (var seg in body)
                        if (x == seg.X && y == seg.Y) { valid = false; break; }
                    if (valid && foodX == x && foodY == y) valid = false;
                    foreach (var obs in obstacles)
                        if (obs.X == x && obs.Y == y) { valid = false; break; }

                    if (valid) obstacles.Add((x, y));
                } while (!valid);
            }
            foreach (var obs in obstacles)
            {
                Console.SetCursorPosition(obs.X, obs.Y);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write('#');
                Console.ResetColor();
            }
        }

        void GenerateFood()
        {
            int r = random.Next(100);
            if (r < 70) currentFoodType = FoodType.Normal;
            else if (r < 90) currentFoodType = FoodType.Golden;
            else currentFoodType = FoodType.Rotten;

            bool valid;
            do
            {
                valid = true;
                foodX = random.Next(1, Console.WindowWidth - 2);
                foodY = random.Next(1, Console.WindowHeight - 2);

                if (foodX == headX && foodY == headY) valid = false;
                foreach (var seg in body)
                    if (foodX == seg.X && foodY == seg.Y) { valid = false; break; }
                foreach (var obs in obstacles)
                    if (foodX == obs.X && foodY == obs.Y) { valid = false; break; }
            } while (!valid);
        }

        void DrawBorder()
        {
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.SetCursorPosition(i, 0);
                Console.Write('#');
                Console.SetCursorPosition(i, Console.WindowHeight - 1);
                Console.Write('#');
            }
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
            foreach (var seg in body)
            {
                Console.SetCursorPosition(seg.X, seg.Y);
                Console.Write('o');
            }
            Console.SetCursorPosition(headX, headY);
            Console.Write('O');
        }

        void DrawFood()
        {
            Console.SetCursorPosition(foodX, foodY);
            switch (currentFoodType)
            {
                case FoodType.Normal:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write('$');
                    break;
                case FoodType.Golden:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write('★');
                    break;
                case FoodType.Rotten:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write('☠');
                    break;
            }
            Console.ResetColor();
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
            Environment.Exit(0);
        }
    }
}