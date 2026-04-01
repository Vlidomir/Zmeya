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
    // Размеры игрового поля (внутри границ)
    static int width = 78;   // ширина внутренней области (80 - 2)
    static int height = 23;  // высота внутренней области (25 - 2)

    static int headX, headY;
    static int directionX, directionY;
    static List<(int X, int Y)> body = new List<(int X, int Y)>();
    static int bodyLength;

    static Random random = new Random();
    static int foodX, foodY;
    static FoodType currentFoodType;

    static int score = 0;
    static int delay = 100;      // начальная задержка (мс)
    const int minDelay = 30;

    static List<(int X, int Y)> obstacles = new List<(int X, int Y)>();
    const int obstacleCount = 5;

    // Буфер для внутренней области (символы на каждой строке)
    static char[][] fieldBuffer;

    static void Main()
    {
        // Настройка консоли
        Console.Title = "Змейка";
        Console.CursorVisible = false;
        Console.SetWindowSize(80, 25);
        Console.SetBufferSize(80, 25);

        // Инициализация змейки
        headX = width / 2;
        headY = height / 2;
        directionX = 1;
        directionY = 0;
        bodyLength = 3;
        body.Clear();
        for (int i = 1; i <= bodyLength; i++)
            body.Add((headX - i, headY));

        // Отрисовка границ (один раз)
        DrawBorder();

        // Генерация препятствий
        GenerateObstacles();

        // Инициализация буфера внутренней области
        fieldBuffer = new char[height][];
        for (int i = 0; i < height; i++)
            fieldBuffer[i] = new char[width];

        // Первая генерация еды
        GenerateFood();

        // Главный игровой цикл
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
                ApplyFoodEffect();
                score++;
                GenerateFood();        // новая еда
            }

            // Обновление тела
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
            if (headX < 0 || headX >= width || headY < 0 || headY >= height ||
                IsCollisionWithObstacle(headX, headY) || IsCollisionWithSelf(headX, headY))
            {
                GameOver();
                break;
            }

            // Перерисовка внутренней области
            RedrawField();

            // Управление скоростью (ускорение с ростом счёта)
            int targetDelay = Math.Max(minDelay, 100 - (score / 5) * 5);
            if (delay > targetDelay) delay = targetDelay;
            Thread.Sleep(delay);
        }
    }

    // --- Методы ---

    static void DrawBorder()
    {
        // Верхняя и нижняя границы
        for (int i = 0; i < Console.WindowWidth; i++)
        {
            Console.SetCursorPosition(i, 0);
            Console.Write('#');
            Console.SetCursorPosition(i, Console.WindowHeight - 1);
            Console.Write('#');
        }
        // Левая и правая границы
        for (int i = 1; i < Console.WindowHeight - 1; i++)
        {
            Console.SetCursorPosition(0, i);
            Console.Write('#');
            Console.SetCursorPosition(Console.WindowWidth - 1, i);
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
                int x = random.Next(0, width);
                int y = random.Next(0, height);
                // Не на голове, не на теле, не на еде, не на другом препятствии
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
        // Случайный тип еды
        int r = random.Next(100);
        if (r < 70) currentFoodType = FoodType.Normal;
        else if (r < 90) currentFoodType = FoodType.Golden;
        else currentFoodType = FoodType.Rotten;

        bool valid;
        do
        {
            valid = true;
            foodX = random.Next(0, width);
            foodY = random.Next(0, height);
            // Не на змейке и не на препятствиях
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
                break;
            case FoodType.Golden:
                score += 4;      // золотая даёт +5 очков, но +1 уже добавлено выше
                bodyLength++;
                break;
            case FoodType.Rotten:
                score -= 2;      // червивая отнимает 1 очко и 1 длину (кроме уже +1)
                bodyLength = Math.Max(3, bodyLength - 1);
                break;
        }
        // Базовое увеличение счёта за съеденную еду
        if (currentFoodType == FoodType.Normal)
            score += 1;
        else if (currentFoodType == FoodType.Golden)
            score += 5;          // итого +5, так как выше добавили 4, а здесь 1
        else if (currentFoodType == FoodType.Rotten)
            score -= 1;          // итого -1, так как выше отняли 2, а здесь 1
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
        // Заполняем буфер пробелами
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                fieldBuffer[y][x] = ' ';

        // Размещаем препятствия
        foreach (var obs in obstacles)
            fieldBuffer[obs.Y][obs.X] = '#';

        // Размещаем тело змейки
        foreach (var seg in body)
            fieldBuffer[seg.Y][seg.X] = 'o';

        // Размещаем голову (поверх тела, если нужно)
        fieldBuffer[headY][headX] = 'O';

        // Размещаем еду (символ зависит от типа)
        char foodChar;
        switch (currentFoodType)
        {
            case FoodType.Normal: foodChar = '$'; break;
            case FoodType.Golden: foodChar = '★'; break;
            default: foodChar = '☠'; break;
        }
        fieldBuffer[foodY][foodX] = foodChar;

        // Выводим каждую строку буфера в консоль
        for (int y = 0; y < height; y++)
        {
            Console.SetCursorPosition(1, y + 1);  // +1 из‑за верхней границы
            Console.Write(new string(fieldBuffer[y]));
        }

        // Обновляем счёт (без мерцания)
        Console.SetCursorPosition(2, 1);
        Console.Write($"Score: {score}     ");
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