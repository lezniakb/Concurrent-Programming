//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class BallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      Vector testinVector = new Vector(0.0, 0.0);
      Ball newInstance = new(testinVector, testinVector);
    }

    [TestMethod]
    public void MoveTestMethod()
    {
      Vector initialPosition = new(10.0, 10.0);
      Ball newInstance = new(initialPosition, new Vector(0.0, 0.0));
      IVector curentPosition = new Vector(0.0, 0.0);
      int numberOfCallBackCalled = 0;
      newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };
      newInstance.Move(new Vector(0.0, 0.0));
      Assert.AreEqual<int>(1, numberOfCallBackCalled);
      Assert.AreEqual<IVector>(initialPosition, curentPosition);
    }
    [TestMethod]
    public void BallMovementConsistencyTest()
    {
        Vector pozycja = new Vector(10.0, 10.0);
        Vector predkosc = new Vector(2.0, 3.0);
        Ball kulka = new Ball(pozycja, predkosc);

        IVector updatedPosition = null;
        kulka.NewPositionNotification += (sender, position) => updatedPosition = position;

        kulka.Move(predkosc);

        Assert.IsNotNull(updatedPosition, "Update sie nie udal");
        Assert.AreEqual(12.0, updatedPosition.x, "Pozcja X niepoprawnie sie aktualizuje");
        Assert.AreEqual(13.0, updatedPosition.y, "Pozcja Y niepoprawnie sie aktualizuje");
    }

    }
}