//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region fields
        private int screenWidth;
        private int screenHeight;

        private bool Disposed = false;

        private readonly Timer MoveTimer;
        private Random RandomGenerator = new();
        private List<Ball> BallsList = new();
        private readonly object _lock = new object();

        #endregion Fields
        #region ctor

        public DataImplementation()
        {
            // Ustawiamy timer, który wywołuje metodę Move co 16 ms (~60 FPS).
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                // pozycja poczatkowa, musi byc w obrebie min 10% od marginesu
                Vector startingPosition = new Vector(
                  random.Next((int)(screenWidth * 0.1), screenWidth - (int)(screenWidth * 0.1)),
                  random.Next((int)(screenHeight * 0.1), screenHeight - (int)(screenHeight * 0.1))
                );

                // podstawowa predkosc kulek
                Vector velocity = new Vector(
                  (random.NextDouble() - 0.5) * 6,
                  (random.NextDouble() - 0.5) * 6
                );

                // tworzenie kulki z ustalona pozycja i predkoscia
                Ball newBall = new Ball(startingPosition, velocity);
                upperLayerHandler(startingPosition, newBall);
                lock (_lock)
                {
                    BallsList.Add(newBall);
                }
            }
        }

        public override void SetScreenSize(double width, double height)
        {
            screenWidth = (int)width;
            screenHeight = (int)height;
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    MoveTimer.Dispose();
                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        // srednica kulki widziana ze strony logicznej != wyswietlana)
        private const double BallDiameter = 40.0;

        /// <summary>
        /// Metoda pomocnicza do obliczania masy kulki.
        /// Masa kulki zalezna jest od srednicy kulki. 
        /// Przyjmujemy, ze masa jest rowna srednicy do potegi 2.
        /// </summary>
        private double CalculateMass(double diameter)
        {
            double mass = 0;
            mass = (double)Math.Pow(diameter, 2);
            return mass;
        }

        /// <summary>
        /// Metoda wywoływana przy każdym takcie timera.
        /// Przemieszcza każdą kulkę zgodnie z jej prędkością,
        /// odwracając kierunek ruchu, gdy kulka napotka brzeg ekranu.
        /// </summary>
        private void Move(object? state)
        {
            lock (_lock)
            {

                for (int i = 0; i < BallsList.Count; i++)
                {
                    Ball ballCurrent = BallsList[i];
                    // pobierz obecna pozycje i predkosc
                    double posX = ballCurrent.getPosition.x;
                    double posY = ballCurrent.getPosition.y;
                    double predkoscX = ballCurrent.Velocity.x;
                    double predkoscY = ballCurrent.Velocity.y;

                    // oblicz nowa pozycje
                    double positionX = posX + predkoscX;
                    double positionY = posY + predkoscY;

                    // sprawdz czy nie uderza w sciane boczna
                    if (positionX < 0)
                    {
                        positionX = 0;
                        predkoscX = -predkoscX;
                    }
                    else if (positionX > screenWidth - BallDiameter)
                    {
                        positionX = screenWidth - BallDiameter;
                        predkoscX = -predkoscX;
                    }

                    // sprawdz czy nie uderza w sufit
                    if (positionY < 0)
                    {
                        positionY = 0;
                        predkoscY = -predkoscY;
                    }
                    else if (positionY > screenHeight - BallDiameter)
                    {
                        positionY = screenHeight - BallDiameter;
                        predkoscY = -predkoscY;
                    }

                    // aktualizacja predkosci jesli zostala odwrocona
                    ballCurrent.Velocity = new Vector(predkoscX, predkoscY);

                    // obliczenie delty, czyli przesuniecia
                    double deltaX = positionX - posX;
                    double deltaY = positionY - posY;

                    // zapisz przesuniecie w wektorze
                    Vector delta = new Vector(deltaX, deltaY);

                    // wywolanie Move ktore przesuwa kulke
                    ballCurrent.Move(delta);

                    // sprawdz czy kulka wchodzi w kolizje z innymi kulkami
                    for (int j = i + 1; j < BallsList.Count; j++)
                    {
                        Ball ballNext = BallsList[j];
                        HandleBallCollision(ballCurrent, ballNext);
                    }
                }
            }   
        }
        private void HandleBallCollision(Ball ballFirst, Ball ballNext)
        {
            // odbierz pozycje i predkosc obu kulek
            IVector pos1 = ballFirst.getPosition;
            IVector pos2 = ballNext.getPosition;
            IVector vel1 = ballFirst.Velocity;
            IVector vel2 = ballNext.Velocity;

            // oblicz dystans na plaszczyznie kartezjanskiej miedzy kulkami
            double dx = pos2.x - pos1.x;
            double dy = pos2.y - pos1.y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            // jestli kulki sie zderzaja
            if (distance < BallDiameter)
            {
                // oblicz ich masy (na podstawie ustawionej srednicy)
                double mass1 = CalculateMass(BallDiameter);
                double mass2 = CalculateMass(BallDiameter);
                double nx = dx / distance;
                double ny = dy / distance;

                // oblicz wzgledna predkosc miedzy dwoma kulkami
                double dvx = vel2.x - vel1.x;
                double dvy = vel2.y - vel1.y;

                // oblicz predkosc na wektorze kolizji
                double dotProduct = dvx * nx + dvy * ny;

                // jesli kulki sie od siebie oddalaja to zakoncz
                if (dotProduct > 0)
                    return;

                // oblicz iloczyn skalarny impulsu miedzy dwoma kulkami
                double impulse = (2 * dotProduct) / (mass1 + mass2);

                // zaktualizuj predkosci obu kulek na podstawie tego impulsu
                double impulsedBallOneX = vel1.x + impulse * mass2 * nx;
                double impulsedBallOneY = vel1.y + impulse * mass2 * ny;

                double impulsedBallTwoX = vel2.x - impulse * mass1 * nx;
                double impulsedBallTwoY = vel2.y - impulse * mass1 * ny;

                vel1 = new Vector(impulsedBallOneX, impulsedBallOneY);
                vel2 = new Vector(impulsedBallTwoX, impulsedBallTwoY);

                // przydziel odpowiednie predkosci tym kulkom
                ballFirst.Velocity = vel1;
                ballNext.Velocity = vel2;

                // uniknij nakladanie sie kulek na siebie
                double overlap = BallDiameter - distance;
                double separationX = nx * overlap / 2;
                double separationY = ny * overlap / 2;
                ballFirst.Move(new Vector(-separationX, -separationY));
                ballNext.Move(new Vector(separationX, separationY));
            }
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}
