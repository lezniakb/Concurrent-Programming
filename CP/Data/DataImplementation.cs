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

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
        #region ctor

        public DataImplementation()
    {
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
        Vector startingPosition = new(random.Next((int)(screenWidth * 0.1), screenWidth - (int)(screenWidth * 0.1)), random.Next((int)(screenHeight * 0.1), screenHeight - (int)(screenHeight * 0.1)));
        //Vector startingPosition = new(random.Next(19, 20), random.Next(19, 20));
                //Vector startingPosition = new(0, 0);
                Ball newBall = new(startingPosition, startingPosition);
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
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

        #endregion IDisposable

        #region private

        private int screenWidth;
        private int screenHeight;

        //private bool disposedValue;
        private bool Disposed = false;

    private readonly Timer MoveTimer;
    private Random RandomGenerator = new();
    private List<Ball> BallsList = [];

    private void Move(object? x)
    {

      foreach (Ball item in BallsList)
        {
        // Przemieszczamy kulę o mały krok od -5 do 5
        double deltaX = (RandomGenerator.NextDouble() - 0.5) * 10; // Zmiana X
        double deltaY = (RandomGenerator.NextDouble() - 0.5) * 10; // Zmiana Y

        //Zmiana pozycji i średnica kuli
        double positionX = item.getPosition.x + deltaX;
        double positionY = item.getPosition.y + deltaY;
        double diameter = 20.0;

        // Sprawdzenie, czy kula nie wychodzi poza granice
        // Jeśli kula wyjdzie poza obszar, odbij ją
        // Punktem odniesienia obiektu jest lewy górny róg

        if (positionY < 0 || positionY > screenHeight - diameter)
        {
            // Zmiana kierunku na przeciwny, jeśli kula jest poza dolną lub górną krawędzią
            deltaY = -deltaY;  // Odbicie w osi Y
        }

        if (positionX < 0 || positionX > screenWidth - diameter)
        {
            // Zmiana kierunku na przeciwny, jeśli kula jest poza lewą lub prawą krawędzią
            deltaX = -deltaX;  // Odbicie w osi X
        }

        // Ustawienie nowej pozycji z uwzględnieniem odbicia
        item.Move(new Vector(deltaX, deltaY));
        }

            /*
              foreach (Ball item in BallsList)
                item.Move(new Vector((RandomGenerator.NextDouble() - 0.5) * 10, (RandomGenerator.NextDouble() - 0.5) * 10));
            */
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