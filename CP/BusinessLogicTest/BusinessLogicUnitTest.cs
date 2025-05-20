//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
  [TestClass]
  public class BusinessLogicImplementationUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      using (BusinessLogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
      {
        bool newInstanceDisposed = true;
        newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
        Assert.IsFalse(newInstanceDisposed);
      }
    }

    [TestMethod]
    public void DisposeTestMethod()
    {
      DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
      BusinessLogicImplementation newInstance = new(dataLayerFixcure);
      Assert.IsFalse(dataLayerFixcure.Disposed);
      bool newInstanceDisposed = true;
      newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
      Assert.IsFalse(newInstanceDisposed);
      newInstance.Dispose();
      newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
      Assert.IsTrue(newInstanceDisposed);
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
      Assert.IsTrue(dataLayerFixcure.Disposed);
    }

    [TestMethod]
    public void StartTestMethod()
    {
      DataLayerStartFixcure dataLayerFixcure = new();
      using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
      {
        int called = 0;
        int numberOfBalls2Create = 10;
        newInstance.Start(
          numberOfBalls2Create,
          (startingPosition, ball) => { called++; Assert.IsNotNull(startingPosition); Assert.IsNotNull(ball); });
        Assert.AreEqual<int>(1, called);
        Assert.IsTrue(dataLayerFixcure.StartCalled);
        Assert.AreEqual<int>(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
      }
    }

    [TestMethod]
    public void zweryfikujInit()
    {
        DataLayerStartFixcure dataLayerFixcure = new();
        using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
        {
            int kulki = 5;
            newInstance.Start(kulki, (position, ball) =>
            {

                Assert.IsNotNull(position, "Pozycja nie moze byc null");
                Assert.IsNotNull(ball, "Kulka nie moze byc null");
                Assert.IsTrue(position.x >= 0, "Pozycja X jest nieprawidlowa");
                Assert.IsTrue(position.y >= 0, "Pozycja Y jest nieprawidlowa");
            });
            Assert.IsTrue(dataLayerFixcure.StartCalled, "Start nie zostal wezwany przy implementacji Data");
            Assert.AreEqual(kulki, dataLayerFixcure.NumberOfBallseCreated, "Ilosc kulek w wartswie Data jest nieprawidlowa.");
        }
    }
    [TestMethod]
    public void zweryfikujDisposal()
    {
        DataLayerDisposeFixcure disposeFixcure = new();
        BusinessLogicImplementation instancja = new(disposeFixcure);

        // Verify initial state
        Assert.IsFalse(disposeFixcure.Disposed, "Poczatkowo warstwa data nie powinna byc disposed");
        bool czyDisposed = false;
        instancja.CheckObjectDisposed(x => czyDisposed = x);
        Assert.IsFalse(czyDisposed, "Bussiness logic implementation nie powinna byc disposed");
        instancja.Dispose();
        // sprawdz stan obiektu
        instancja.CheckObjectDisposed(x => czyDisposed = x);
        Assert.IsTrue(czyDisposed, "Bussiness logic implementation powinna byc disposed");
        Assert.IsTrue(disposeFixcure.Disposed, "warstwa data powinna byc disposed");

        // Verify that further operations throw exceptions
        Assert.ThrowsException<ObjectDisposedException>(() => instancja.Dispose(), "dispose powinien rzucic wyjatek");
        Assert.ThrowsException<ObjectDisposedException>(() => instancja.Start(0, (position, ball) => { }), "start powinien rzucic wyjatek po dispose");
    }


        #region testing instrumentation

        private class DataLayerConstructorFixcure : Data.DataAbstractAPI
    {
      public override void Dispose()
      { }

            public override void SetScreenSize(double width, double height)
            {
                throw new NotImplementedException();
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
      {
        throw new NotImplementedException();
      }
    }

    private class DataLayerDisposeFixcure : Data.DataAbstractAPI
    {
      internal bool Disposed = false;

      public override void Dispose()
      {
        Disposed = true;
      }

            public override void SetScreenSize(double width, double height)
            {
                throw new NotImplementedException();
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
      {
        throw new NotImplementedException();
      }
    }

    private class DataLayerStartFixcure : Data.DataAbstractAPI
    {
      internal bool StartCalled = false;
      internal int NumberOfBallseCreated = -1;

      public override void Dispose()
      { }

      public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler)
      {
        StartCalled = true;
        NumberOfBallseCreated = numberOfBalls;
        upperLayerHandler(new DataVectorFixture(), new DataBallFixture());
      }

            public override void SetScreenSize(double width, double height)
            {
                throw new NotImplementedException();
            }

            private record DataVectorFixture : Data.IVector
      {
        public double x { get; init; }
        public double y { get; init; }
      }

        private class DataBallFixture : Data.IBall
        {
            public Data.IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public event EventHandler<Data.IVector>? NewPositionNotification = null;

            // Implementing missing interface members
            public int Id => 1;

            // Implemented as a property, not a method
            public Data.IVector getPosition
            {
                get { return new DataVectorFixture(); }
            }
        }

        }

        #endregion testing instrumentation
    }
}