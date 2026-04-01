using System;
using System.Collections.Generic;
using System.Threading;

enum FoodType
{
    Normal,
    Golden,
    Rotten
}

class Program
{
    // Размеры поля внутри границ
    static readonly int fieldWidth = 78;
    static readonly int fieldHeight = 23;

    static int headX, headY;
    static int dirX, dirY;
    static List<(int X, int Y)> body = new List<(int X, int Y)>();
    static int bodyLength;

    static Random random = new Random();
    static int foodX, foodY;
    static FoodType currentFoodType;

    static int score = 0;
    static int delay = 50;            // начальная задержка 70 мс (быстрее)
    const int minDelay = 20;          // минимальная 20 мс

    static List<(int X, int Y)> obstacles = new List<(int X, int Y)>();
    const int obstacleCount = 5;

    static char[,] fieldBuffer;

    static void Main()
    {
        Console.Title = "Змейка";
        Console.CursorVisible = false;
        Console.SetWindowSize(80, 25);
        Console.SetBufferSize(80, 25);

        // Змейка
        headX = fieldWidth / 2;
        headY = fieldHeight / 2;
        dirX = 1;
        dirY = 0;
        bodyLength = 3;
        body.Clear();
        for (int i = 1; i <= bodyLength; i++)
            body.Add((headX - i, headY));

        DrawBorder();
        GenerateObstacles();

        fieldBuffer = new char[fieldHeight, fieldWidth];
        for (int y = 0; y < fieldHeight; y++)
            for (int x = 0; x < fieldWidth; x++)
                fieldBuffer[y, x] = ' ';

        GenerateFood();

        while (true)
        {
            // Управление
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (dirY != 1) { dirX = 0; dirY = -1; }
                        break;
                    case ConsoleKey.DownArrow:
                        if (dirY != -1) { dirX = 0; dirY = 1; }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (dirX != 1) { dirX = -1; dirY = 0; }
                        break;
                    case ConsoleKey.RightArrow:
                        if (dirX != -1) { dirX = 1; dirY = 0; }
                        break;
                    case ConsoleKey.Escape:
                        return;
                }
            }

            // Движение
            bool ate = false;
            int oldHeadX = headX, oldHeadY = headY;
            headX += dirX;
            headY += dirY;

            if (headX == foodX && headY == foodY)
            {
                ate = true;
                ApplyFoodEffect();
                score++;
                GenerateFood();
            }

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

            // Столкновения
            if (headX < 0 || headX >= fieldWidth || headY < 0 || headY >= fieldHeight ||
                IsCollisionWithObstacle(headX, headY) || IsCollisionWithSelf(headX, headY))
            {
                GameOver();
                break;
            }

            RedrawField();

            // Ускорение: каждые 3 очка уменьшаем задержку на 5 мс
            int targetDelay = Math.Max(minDelay, 50 - (score / 3) * 5);
            if (delay > targetDelay) delay = targetDelay;
            Thread.Sleep(delay);
        }
    }

    static void DrawBorder()
    {
        for (int x = 0; x < Console.WindowWidth; x++)
        {
            Console.SetCursorPosition(x, 0);
            Console.Write('#');
            Console.SetCursorPosition(x, Console.WindowHeight - 1);
            Console.Write('#');
        }
        for (int y = 1; y < Console.WindowHeight - 1; y++)
        {
            Console.SetCursorPosition(0, y);
            Console.Write('#');
            Console.SetCursorPosition(Console.WindowWidth - 1, y);
            Console.Write('#');
        }
    }

    static void GenerateObstacles()
    {
        obstacles.Clear();
        for (int i = 0; i < obstacleCount; i++)
        {
            bool valid;
            do
            {
                valid = true;
                int x = random.Next(0, fieldWidth);
                int y = random.Next(0, fieldHeight);
                if (x == headX && y == headY) valid = false;
                foreach (var seg in body)
                    if (x == seg.X && y == seg.Y) valid = false;
                if (valid && foodX == x && foodY == y) valid = false;
                foreach (var obs in obstacles)
                    if (obs.X == x && obs.Y == y) valid = false;
                if (valid)
                    obstacles.Add((x, y));
            } while (!valid);
        }
    }

    static void GenerateFood()
    {
        int r = random.Next(100);
        if (r < 70) currentFoodType = FoodType.Normal;
        else if (r < 90) currentFoodType = FoodType.Golden;
        else currentFoodType = FoodType.Rotten;

        bool valid;
        do
        {
            valid = true;
            foodX = random.Next(0, fieldWidth);
            foodY = random.Next(0, fieldHeight);
            if (foodX == headX && foodY == headY) valid = false;
            foreach (var seg in body)
                if (foodX == seg.X && foodY == seg.Y) valid = false;
            foreach (var obs in obstacles)
                if (foodX == obs.X && foodY == obs.Y) valid = false;
        } while (!valid);
    }

    static void ApplyFoodEffect()
    {
        switch (currentFoodType)
        {
            case FoodType.Normal:
                bodyLength++;
                score += 1;
                break;
            case FoodType.Golden:
                bodyLength++;
                score += 5;
                break;
            case FoodType.Rotten:
                bodyLength = Math.Max(3, bodyLength - 1);
                score -= 1;
                break;
        }
    }

    static bool IsCollisionWithObstacle(int x, int y)
    {
        foreach (var obs in obstacles)
            if (obs.X == x && obs.Y == y)
                return true;
        return false;
    }

    static bool IsCollisionWithSelf(int x, int y)
    {
        foreach (var seg in body)
            if (seg.X == x && seg.Y == y)
                return true;
        return false;
    }

    static void RedrawField()
    {
        // Очистка буфера
        for (int y = 0; y < fieldHeight; y++)
            for (int x = 0; x < fieldWidth; x++)
                fieldBuffer[y, x] = ' ';

        // Препятствия
        foreach (var obs in obstacles)
            fieldBuffer[obs.Y, obs.X] = '#';

        // Тело змейки
        foreach (var seg in body)
            fieldBuffer[seg.Y, seg.X] = 'o';

        // Голова
        fieldBuffer[headY, headX] = 'O';

        // Еда
        char foodChar = currentFoodType switch
        {
            FoodType.Normal => '$',
            FoodType.Golden => '★',
            FoodType.Rotten => '☠',
            _ => '$'
        };
        fieldBuffer[foodY, foodX] = foodChar;

        // Вывод строк целиком (быстрее, чем посимвольно)
        for (int y = 0; y < fieldHeight; y++)
        {
            Console.SetCursorPosition(1, y + 1);
            for (int x = 0; x < fieldWidth; x++)
            {
                char c = fieldBuffer[y, x];
                // Устанавливаем цвет только если символ не пробел
                if (c != ' ')
                {
                    if (c == 'O' || c == 'o')
                        Console.ForegroundColor = ConsoleColor.Green;
                    else if (c == '#')
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    else if (c == '★')
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (c == '☠')
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    else if (c == '$')
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(c);
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(c);
                }
            }
        }

        // Счёт
        Console.SetCursorPosition(2, 1);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Score: {score}     ");
        Console.ResetColor();
    }

    static void GameOver()
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