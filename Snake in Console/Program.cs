using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    // ====== GAME SETTINGS ======
    const int WIDTH = 24;          // this is the inner play area width (without borders)
    const int HEIGHT = 16;         // and this one is the inner play area height (without borders)
    const int TICK_MS = 150;       // the lower the value, the faster the snake
    const char BORDER = '#';
    const char EMPTY = ' ';
    const char FOOD = '*';
    const char HEAD = 'O';
    const char BODY = 'o';

    static readonly Random rng = new Random();

    // These are the directions in which the snake will go
    enum Dir { Up, Down, Left, Right }

    // This is A simple integer point
    readonly record struct P(int X, int Y);

    static void Main()
    {
        Console.CursorVisible = false;

        bool playAgain = true;
        while (playAgain)
        {
            int score = RunOneGame();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nHa! You're gay! Score: {score}");
            Console.ResetColor();
            Console.Write("Play again? (Y/N): ");
            playAgain = (Console.ReadLine()?.Trim().ToUpper() == "Y");
        }

        Console.CursorVisible = true;
    }

    // One the game is over, it has to show the score (full game)
    static int RunOneGame()
    {
        // Snake state
        var snake = new LinkedList<P>();
        var start = new P(WIDTH / 2, HEIGHT / 2);
        snake.AddFirst(start);
        snake.AddLast(new P(start.X - 1, start.Y));
        snake.AddLast(new P(start.X - 2, start.Y));
        Dir dir = Dir.Right;
        int score = 0;

        // This is how the food will be spaned onto the board
        P food = SpawnFood(snake);

        // Draw the board
        DrawFrame(snake, food, score);

        // This creates a loop once you're dead
        var lastTick = DateTime.UtcNow;
        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                dir = NextDirection(dir, key);
                // This is toavoid extra keys, so there's less input lag (optimizing)
                while (Console.KeyAvailable) Console.ReadKey(true);
            }

            // 2) The tick of the game is based on time
            var now = DateTime.UtcNow;
            if ((now - lastTick).TotalMilliseconds < TICK_MS)
            {
                Thread.Sleep(1);
                continue;
            }
            lastTick = now;

            // 3) This is how the snake is going to move, it creates a new head forward
            var head = snake.First!.Value;
            var newHead = dir switch
            {
                Dir.Up => new P(head.X, head.Y - 1),
                Dir.Down => new P(head.X, head.Y + 1),
                Dir.Left => new P(head.X - 1, head.Y),
                Dir.Right => new P(head.X + 1, head.Y),
                _ => head
            };

            // 4) What happens if we hit a wall?
            if (newHead.X < 0 || newHead.X >= WIDTH || newHead.Y < 0 || newHead.Y >= HEIGHT)
                break; // game over

            // 5) Or what will happen if we hit ourselves?
            foreach (var seg in snake)
                if (seg.Equals(newHead)) // this hits our own body
                    goto EndGame;

            // This is how we advance, by adding a new head onto the board
            snake.AddFirst(newHead);

            // 7) Either we eat or move forward
            if (newHead.Equals(food))
            {
                score += 10;
                food = SpawnFood(snake);
                // If we don't remove tail, the tail grows
            }
            else
            {
                // If we do, the tail rests the same
                snake.RemoveLast();
            }

            // 8) Redraw everything
            DrawFrame(snake, food, score);
        }

    EndGame:
        return score;
    }

    // We do this so we don't do 180-degrees move otherwise the game will be kinda :/
    static Dir NextDirection(Dir current, ConsoleKey key)
    {
        return key switch
        {
            ConsoleKey.UpArrow or ConsoleKey.W => current == Dir.Down ? current : Dir.Up,
            ConsoleKey.DownArrow or ConsoleKey.S => current == Dir.Up ? current : Dir.Down,
            ConsoleKey.LeftArrow or ConsoleKey.A => current == Dir.Right ? current : Dir.Left,
            ConsoleKey.RightArrow or ConsoleKey.D => current == Dir.Left ? current : Dir.Right,
            _ => current
        };
    }

    // And this is made so the food doesn't spawn on the snake
    static P SpawnFood(LinkedList<P> snake)
    {
        while (true)
        {
            var p = new P(rng.Next(WIDTH), rng.Next(HEIGHT));
            bool onSnake = false;
            foreach (var seg in snake)
                if (seg.Equals(p)) { onSnake = true; break; }
            if (!onSnake) return p;
        }
    }

    // Draw everything (borders + play area)
    static void DrawFrame(LinkedList<P> snake, P food, int score)
    {
        Console.SetCursorPosition(0, 0);

        // The top border
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(BORDER);
        for (int x = 0; x < WIDTH; x++) Console.Write(BORDER);
        Console.WriteLine(BORDER);

        for (int y = 0; y < HEIGHT; y++)
        {
            Console.Write(BORDER); // The left border
            for (int x = 0; x < WIDTH; x++)
            {
                char ch = EMPTY;

                // the food spawner
                if (x == food.X && y == food.Y)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    ch = FOOD;
                }

                // snake (if overlap with food, then we draw the head over)
                foreach (var (sx, sy) in snake)
                {
                    if (sx == x && sy == y)
                    {
                        bool isHead = snake.First!.Value.X == x && snake.First!.Value.Y == y;
                        Console.ForegroundColor = isHead ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                        ch = isHead ? HEAD : BODY;
                        break;
                    }
                }

                if (ch == EMPTY)
                {
                    Console.ForegroundColor = ConsoleColor.Black; // Background color
                    Console.Write(' ');
                }
                else
                {
                    Console.Write(ch);
                }

                Console.ResetColor();
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(BORDER); // The right border
        }

        // The bottom border
        Console.Write(BORDER);
        for (int x = 0; x < WIDTH; x++) Console.Write(BORDER);
        Console.WriteLine(BORDER);

        // And finally, the HUD
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Score: {score}   Use Arrow Keys or WASD to move.  If you hit any walls or your huge balls, you're gay!");
        Console.ResetColor();
    }
}
