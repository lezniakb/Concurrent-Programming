//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
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
        }

        #endregion ctor

        #region IBall

        // Zdarzenie powiadamiające o zmianie pozycji kulki.
        public event EventHandler<IVector>? NewPositionNotification;

        // Aktualna prędkość (która będzie używana przez symulację).
        public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        // Prywatne przechowywanie aktualnej pozycji kulki.
        private Vector Position;

        // Metoda powiadamiająca subskrybentów o naszej nowej pozycji.
        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        // Metoda Move przyjmuje delta (różnicę) i dodaje ją do aktualnej pozycji.
        internal void Move(Vector delta)
        {
            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            RaiseNewPositionChangeNotification();
        }

        #endregion private

        #region public

        // Właściwość zwracająca aktualną pozycję (tylko do odczytu).
        public IVector getPosition
        {
            get { return Position; }
        }

        #endregion public
    }
}
