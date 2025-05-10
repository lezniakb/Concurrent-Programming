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

        [TestMethod]
        public void VelocityPropertyTest()
        {
            Vector poczatkowaPozycja = new Vector(5.0, 5.0);
            Vector poczatkowaPredkosc = new Vector(1.0, 2.0);
            Ball kulka = new Ball(poczatkowaPozycja, poczatkowaPredkosc);
            Assert.AreEqual(1.0, kulka.Velocity.x, "Początkowa prędkość X nie jest dobrze ustawiona");
            Assert.AreEqual(2.0, kulka.Velocity.y, "Początkowa prędkość Y nie jest dobrze ustawiona");
            Vector nowaPredkosc = new Vector(-3.0, 4.0);
            kulka.Velocity = nowaPredkosc;
            Assert.AreEqual(-3.0, kulka.Velocity.x, "Nowa prędkość X nie została zaktualizowana jak powinna");
            Assert.AreEqual(4.0, kulka.Velocity.y, "Nowa prędkość Y nie została zaktualizowana jak powinna");
        }

        [TestMethod]
        public void MultipleEventHandlersTest()
        {
            Vector poczatkowaPozycja = new Vector(0.0, 0.0);
            Vector predkosc = new Vector(1.0, 1.0);
            Ball kulka = new Ball(poczatkowaPozycja, predkosc);
            int licznikWywolan1 = 0;
            int licznikWywolan2 = 0;
            IVector pozycjaZHandler1 = null;
            IVector pozycjaZHandler2 = null;
            kulka.NewPositionNotification += (sender, pozycja) =>
            {
                licznikWywolan1++;
                pozycjaZHandler1 = pozycja;
                Assert.IsNotNull(sender, "Nadawca nie może być null");
            };
            kulka.NewPositionNotification += (sender, pozycja) =>
            {
                licznikWywolan2++;
                pozycjaZHandler2 = pozycja;
                Assert.IsNotNull(sender, "Nadawca nie może być null");
            };
            Vector delta = new Vector(5.0, 7.0);
            kulka.Move(delta);
            Assert.AreEqual(1, licznikWywolan1, "Pierwszy handler nie jest wywołany poprawnie");
            Assert.AreEqual(1, licznikWywolan2, "Drugi handler nie jest wywołany poprawnie");
            Assert.IsNotNull(pozycjaZHandler1, "Pozycja z pierwszego handlera jest null");
            Assert.IsNotNull(pozycjaZHandler2, "Pozycja z drugiego handlera jest null");
            Assert.AreEqual(5.0, pozycjaZHandler1.x, "Pozycja X z pierwszego handlera jest niewlasciwa");
            Assert.AreEqual(7.0, pozycjaZHandler1.y, "Pozycja Y z pierwszego handlera jest niewlasciwa");
            Assert.AreEqual(5.0, pozycjaZHandler2.x, "Pozycja X z drugiego handlera jest niewlasciwa");
            Assert.AreEqual(7.0, pozycjaZHandler2.y, "Pozycja Y z drugiego handlera jest niewlasciwa");
        }
    }
}