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
    // Rozszerzenie interfejsu IBall o identyfikator kuli

    // Implementacja klasy Ball korzystającej z interfejsu IBall
    // Internal oznacza, że ta klasa jest dostępna tylko w tym Assembly.
    internal class Ball : IBall
    {
        #region ctor
        // Konstruktor przyjmuje wektor pozycji początkowej i wektor początkowej prędkości.
        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            Position = initialPosition;
            Velocity = initialVelocity;

            // Przydzielenie unikalnego ID z zabezpieczeniem przed konfliktem wątków
            lock (_idLock)
            {
                Id = _nextId++;
            }

            // Inicjalizacja czasomierza dla tej kuli
            _creationTime = Stopwatch.GetTimestamp();
        }
        #endregion ctor

        #region IBall
        // Zdarzenie powiadamiające o zmianie pozycji kulki.
        public event EventHandler<IVector>? NewPositionNotification;

        // Aktualna prędkość (która będzie używana przez symulację).
        public IVector Velocity { get; set; }

        // Unikalny identyfikator kuli
        public int Id { get; private set; }
        #endregion IBall

        #region private
        // Statyczny licznik do generowania unikalnych ID
        private static int _nextId = 0;
        private static readonly object _idLock = new object();

        // Prywatne przechowywanie aktualnej pozycji kulki.
        private Vector Position;

        // Czas utworzenia kuli (w tikach stopera)
        private readonly long _creationTime;

        // Licznik ruchów kuli - do celów diagnostycznych
        private int _moveCount = 0;

        // Metoda powiadamiająca subskrybentów o naszej nowej pozycji.
        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        // Metoda Move przyjmuje delta (różnicę) i dodaje ją do aktualnej pozycji.
        internal void Move(Vector delta)
        {
            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            _moveCount++;
            RaiseNewPositionChangeNotification();
        }
        #endregion private

        #region public
        // Właściwość zwracająca aktualną pozycję (tylko do odczytu).
        public IVector getPosition
        {
            get { return Position; }
        }

        // Właściwość zwracająca czas życia kuli w milisekundach
        public double LifeTimeMs
        {
            get
            {
                long currentTime = Stopwatch.GetTimestamp();
                return (currentTime - _creationTime) * 1000.0 / Stopwatch.Frequency;
            }
        }

        // Metoda diagnostyczna zwracająca dane o kuli w formie tekstu ASCII
        public string GetDiagnosticData()
        {
            return $"Ball[{Id}] pos:({Position.x:F2},{Position.y:F2}) vel:({Velocity.x:F2},{Velocity.y:F2}) " +
                   $"moves:{_moveCount} lifetime:{LifeTimeMs:F1}ms";
        }
        #endregion public
    }
}