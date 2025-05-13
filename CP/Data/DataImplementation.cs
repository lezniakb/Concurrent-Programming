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
        private readonly List<Thread> BallThreads = new();

        private Random RandomGenerator = new();
        private List<Ball> BallsList = new();
        private readonly object _lock = new object();

        #endregion Fields
        #region ctor

        public DataImplementation()
        {
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
                Vector startingPosition = new Vector(
                    random.Next((int)(screenWidth * 0.1), screenWidth - (int)(screenWidth * 0.1)),
                    random.Next((int)(screenHeight * 0.1), screenHeight - (int)(screenHeight * 0.1))
                );

                Vector velocity = new Vector(
                    (random.NextDouble() - 0.5) * 6,
                    (random.NextDouble() - 0.5) * 6
                );

                Ball newBall = new Ball(startingPosition, velocity);
                upperLayerHandler(startingPosition, newBall);

                lock (_lock)
                {
                    BallsList.Add(newBall);
                }

                var worker = new BallWorker(newBall, screenWidth, screenHeight, BallDiameter, _lock, BallsList, () => Disposed);
                Thread thread = new Thread(worker.Run);
                BallThreads.Add(thread);
                thread.Start();
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
                    Disposed = true;

                    foreach (var thread in BallThreads)
                        thread.Join();

                    BallsList.Clear();
                }
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
        

        private class BallWorker
        {
            private readonly Ball _ball;
            private readonly int _screenWidth;
            private readonly int _screenHeight;
            private readonly double _ballDiameter;
            private readonly object _lock;
            private readonly List<Ball> _ballsList;
            private readonly Func<bool> _isDisposed;

            public BallWorker(Ball ball, int screenWidth, int screenHeight, double ballDiameter, object lockObject, List<Ball> ballsList, Func<bool> isDisposed)
            {
                _ball = ball;
                _screenWidth = screenWidth;
                _screenHeight = screenHeight;
                _ballDiameter = ballDiameter;
                _lock = lockObject;
                _ballsList = ballsList;
                _isDisposed = isDisposed;
            }

            public void Run()
            {
                while (!_isDisposed())
                {
                    lock (_lock)
                    {
                        UpdatePosition();
                        HandleCollisions();
                    }
                    Thread.Sleep(16);
                }
            }

            private void UpdatePosition()
            {
                double posX = _ball.getPosition.x;
                double posY = _ball.getPosition.y;
                double velX = _ball.Velocity.x;
                double velY = _ball.Velocity.y;

                double newX = posX + velX;
                double newY = posY + velY;

                if (newX < 0 || newX > _screenWidth - _ballDiameter)
                {
                    velX = -velX;
                    newX = Math.Clamp(newX, 0, _screenWidth - _ballDiameter);
                }

                if (newY < 0 || newY > _screenHeight - _ballDiameter)
                {
                    velY = -velY;
                    newY = Math.Clamp(newY, 0, _screenHeight - _ballDiameter);
                }

                _ball.Velocity = new Vector(velX, velY);
                Vector delta = new Vector(newX - posX, newY - posY);
                _ball.Move(delta);
            }

            private void HandleCollisions()
            {
                foreach (var other in _ballsList)
                {
                    if (other == _ball)
                        continue;
                    double dx = other.getPosition.x - _ball.getPosition.x;
                    double dy = other.getPosition.y - _ball.getPosition.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance < _ballDiameter)
                    {
                        double mass1 = Math.Pow(_ballDiameter, 2);
                        double mass2 = Math.Pow(_ballDiameter, 2);
                        double nx = dx / distance;
                        double ny = dy / distance;

                        double dvx = other.Velocity.x - _ball.Velocity.x;
                        double dvy = other.Velocity.y - _ball.Velocity.y;

                        double dotProduct = dvx * nx + dvy * ny;

                        if (dotProduct > 0) return;

                        double impulse = (2 * dotProduct) / (mass1 + mass2);

                        Vector vel1 = new Vector(
                            _ball.Velocity.x + impulse * mass2 * nx,
                            _ball.Velocity.y + impulse * mass2 * ny);

                        Vector vel2 = new Vector(
                            other.Velocity.x - impulse * mass1 * nx,
                            other.Velocity.y - impulse * mass1 * ny);

                        _ball.Velocity = vel1;
                        other.Velocity = vel2;

                        double overlap = _ballDiameter - distance;
                        double separationX = nx * overlap / 2;
                        double separationY = ny * overlap / 2;

                        _ball.Move(new Vector(-separationX, -separationY));
                        other.Move(new Vector(separationX, separationY));
                    }
                }
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
