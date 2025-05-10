//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                IEnumerable<IBall>? ballsList = null;
                newInstance.CheckBallsList(x => ballsList = x);
                Assert.IsNotNull(ballsList);
                int numberOfBalls = 0;
                newInstance.CheckNumberOfBalls(x => numberOfBalls = x);
                Assert.AreEqual<int>(0, numberOfBalls);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataImplementation newInstance = new DataImplementation();
            bool newInstanceDisposed = false;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            IEnumerable<IBall>? ballsList = null;
            newInstance.CheckBallsList(x => ballsList = x);
            Assert.IsNotNull(ballsList);
            newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(0, x));
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
        }

        [TestMethod]
        public void StartTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int numberOfCallbackInvoked = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                  numberOfBalls2Create,
                  (startingPosition, ball) =>
                  {
                      numberOfCallbackInvoked++;
                      Assert.IsTrue(startingPosition.x >= 0);
                      Assert.IsTrue(startingPosition.y >= 0);
                      Assert.IsNotNull(ball);
                  });
                Assert.AreEqual<int>(numberOfBalls2Create, numberOfCallbackInvoked);
                newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(10, x));
            }
        }

        [TestMethod]
        public void VerifyBallCountAfterStart()
        {
            using (DataImplementation dataImplementation = new DataImplementation())
            {
                int kulki = 15;
                dataImplementation.Start(kulki, (startingPosition, ball) => { });
                // sprawdz ilosc kulek
                dataImplementation.CheckNumberOfBalls(count =>
                {
                    Assert.AreEqual(kulki, count, "ilosc kulek nie rowna sie utworzonej ilosci");
                });
            }
        }

        [TestMethod]
        public void SetScreenSizeTestMethod()
        {
            using (DataImplementation dataImplementation = new DataImplementation())
            {
                double width = 1280;
                double height = 720;
                dataImplementation.SetScreenSize(width, height);

                int ballCount = 5;
                List<IBall> createdBalls = new List<IBall>();

                dataImplementation.Start(ballCount, (position, ball) => {
                    createdBalls.Add(ball);
                    Assert.IsTrue(position.x >= width * 0.1);
                    Assert.IsTrue(position.x <= width * 0.9);
                    Assert.IsTrue(position.y >= height * 0.1);
                    Assert.IsTrue(position.y <= height * 0.9);
                });
                dataImplementation.CheckNumberOfBalls(count => Assert.AreEqual(ballCount, count));
            }
        }

        [TestMethod]
        public void StartWithNullHandlerThrowsException()
        {
            using (DataImplementation dataImplementation = new DataImplementation())
            {
                Assert.ThrowsException<ArgumentNullException>(() => dataImplementation.Start(5, null));
            }
        }

        [TestMethod]
        public void VerifyBallsMove()
        {
            using (DataImplementation dataImplementation = new DataImplementation())
            {
                dataImplementation.SetScreenSize(1000, 800);
                Dictionary<IBall, IVector> initialPositions = new Dictionary<IBall, IVector>();

                dataImplementation.Start(1, (position, ball) => {
                    initialPositions.Add(ball, new Vector(position.x, position.y));
                });
                Thread.Sleep(100);
                dataImplementation.CheckBallsList(balls => {
                    foreach (IBall ball in balls)
                    {
                        IVector initialPos = initialPositions[ball];
                        Ball concreteBall = ball as Ball;
                        Assert.IsNotNull(concreteBall, "Ball powinno byc castowane");

                        IVector currentPos = concreteBall.getPosition;
                        Assert.IsTrue(
                            initialPos.x != currentPos.x || initialPos.y != currentPos.y,
                            "Kulka nie ruszyla sie ze swojej pozycji.."
                        );
                    }
                });
            }
        }
    }
}