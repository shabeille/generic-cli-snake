﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CLI_Snake
{
    internal class Program
    {
        private const int framerate = 30;

        static void Main(string[] args)
        {
            int numberOfApples;


            if (args.Length == 0)
            {
                string input;

                do
                {
                    Console.Write("Enter how many apples: ");
                    input = Console.ReadLine();
                } while (int.TryParse(input, out numberOfApples) == false);
            }
            else
            {
                try
                {
                    numberOfApples = int.Parse(args[0]);
                }
                catch (FormatException)
                {
                    Console.WriteLine("Args must be an integer input");
                    return;
                }
            }

            Console.Clear();

            List<Apple> apples = new List<Apple>();

            for (int i = 0; i < numberOfApples; i++)
                apples.Add(new Apple('A'));

            Snake snake = new Snake((10, 5), "right", 'O');

            int variableFramerate = framerate;
            bool gameRunning = true;
            int score = 0;

            // game loop
            while (gameRunning)
            {
                gameRunning = snake.MoveSnake();
                Thread.Sleep(1000 / variableFramerate);

                if (Console.KeyAvailable) // check if there has been a key input
                    snake.GetDirectionInput();

                foreach (Apple apple in apples)
                {
                    if (snake.CheckHeadPointProximity(apple.currentPosition))
                    {
                        score++;
                        apple.MoveToRandomCoordinates(snake.points);
                        snake.AddPoint();

                        Console.SetCursorPosition(0, 0);
                        Console.Write($"Score: {score}");

                        break; // we don't care if there are more apples in one spot, that can cause us to crash
                    }
                }


                // this is needed because theres more vertical spacing than horizontal spacing on terminal window
                // we do this to get a more consistent speed across both axis
                if (snake.direction == "up" || snake.direction == "down")
                    variableFramerate = framerate / 2;
                else
                    variableFramerate = framerate;


            }

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Score: {score}. Game Over");

            Thread.Sleep(1000);
            Console.ResetColor();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            Console.Clear();

        }


        public static (int, int) GetConsoleDimensions()
        {
            return (Console.WindowWidth, Console.WindowHeight);
        }


    }


    class Snake
    {
        public List<Point> points;
        public string direction;
        private readonly char character;

        public Snake((int, int) startingPoint, string direction, char character)
        {
            points = new List<Point> { new Point(startingPoint, direction) };

            this.direction = direction;
            this.character = character;
        }

        public bool MoveSnake()
        {
            (int X, int Y) newCoordinates = GetNextPosition(points.Last().coordinate, direction, 1);

            foreach (var point in points)
            {
                if (point.coordinate == newCoordinates)
                    return false; // check that we didnt run into ourselves
            }

            try
            {
                Console.SetCursorPosition(newCoordinates.X, newCoordinates.Y);
            }
            catch (ArgumentOutOfRangeException) // thrown when snake goes out of bounds
            {
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(character);
            Console.ResetColor();


            points.Add(new Point(newCoordinates, direction));


            Console.SetCursorPosition(points[0].coordinate.X, points[0].coordinate.Y);
            Console.Write(' ');
            points.Remove(points[0]);

            return true;
        }


        private (int, int) GetNextPosition((int X, int Y) currentPosition, string passedDirection, int increment)
        {
            switch (passedDirection)
            {
                case "left":
                    return (currentPosition.X - increment, currentPosition.Y);

                case "right":
                    return (currentPosition.X + increment, currentPosition.Y);

                case "up":
                    return (currentPosition.X, currentPosition.Y - increment);

                case "down":
                    return (currentPosition.X, currentPosition.Y + increment);

                default:
                    return currentPosition; // something has gone terribly wrong if this happens
            }
        }


        public void GetDirectionInput()
        {
            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (direction != "right")
                        direction = "left"; return;

                case ConsoleKey.RightArrow:
                    if (direction != "left")
                        direction = "right"; return;

                case ConsoleKey.UpArrow:
                    if (direction != "down")
                        direction = "up"; return;

                case ConsoleKey.DownArrow:
                    if (direction != "up")
                        direction = "down"; return;
            }
        }


        public bool CheckHeadPointProximity((int, int) coordinates)
        {
            if (points.Last().coordinate == coordinates)
                return true;
            else
                return false;
        }

        public void AddPoint()
        {
            var lastPoint = points[0];
            (int X, int Y) newCoordinates = GetNextPosition(lastPoint.coordinate, lastPoint.direction, -1);

            points.Insert(0, new Point(newCoordinates, lastPoint.direction));
            Console.SetCursorPosition(lastPoint.coordinate.X, lastPoint.coordinate.Y);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(character);
            Console.ResetColor();

        }
    }


    class Apple
    {
        public (int Left, int Top) currentPosition;
        private readonly char character;

        private static readonly Random random = new Random();


        public Apple(char character)
        {

            this.character = character;

            MoveToRandomCoordinates();

        }


        public void MoveToCoordinates((int Left, int Top) coordinate)
        {
            Console.SetCursorPosition(coordinate.Left, coordinate.Top);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(character); // write the new apple position
            Console.ResetColor();

            currentPosition = coordinate;
        }


        public void MoveToRandomCoordinates(List<Point> snakePoints = null)
        {

            if (snakePoints == null)
                snakePoints = new List<Point>();

            (int width, int height) = Program.GetConsoleDimensions();
            int randomX;
            int randomY;

            do
            {
                randomX = random.Next(0, width);
                randomY = random.Next(0, height);
            } while (snakePoints.Any(p => p.coordinate == (randomX, randomY)));

            MoveToCoordinates((randomX, randomY));

        }

    }


    class Point
    {
        public (int X, int Y) coordinate;
        public string direction;

        public Point((int, int) coordinate, string direction)
        {
            this.coordinate = coordinate;
            this.direction = direction;
        }
    }
}
