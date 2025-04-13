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
                BallsList.Add(newBall);
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

        private int screenWidth;
        private int screenHeight;

        private bool Disposed = false;

        private readonly Timer MoveTimer;
        private Random RandomGenerator = new();
        private List<Ball> BallsList = new();

        // srednica kulki
        private const double BallDiameter = 20.0;

        /// <summary>
        /// Metoda wywoływana przy każdym takcie timera.
        /// Przemieszcza każdą kulkę zgodnie z jej prędkością,
        /// odwracając kierunek ruchu, gdy kulka napotka brzeg ekranu.
        /// </summary>
        private void Move(object? state)
        {
            foreach (Ball item in BallsList)
            {
                // pobierz obecna pozycje i predkosc
                double posX = item.getPosition.x;
                double posY = item.getPosition.y;
                double predkoscX = item.Velocity.x;
                double predkoscY = item.Velocity.y;

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
                item.Velocity = new Vector(predkoscX, predkoscY);

                // obliczenie delty, czyli przesuniecia
                double deltaX = positionX - posX;
                double deltaY = positionY - posY;
                // wywolanie Move ktore przesuwa kulke
                item.Move(new Vector(deltaX, deltaY));
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
