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
  // implementacja klasy Ball używającej interfejsu IBall
  // internal oznacza, że ta klasa dostępna jest tylko dla tego Assembly (Info.cs)
  internal class Ball : IBall
  {
    #region ctor

        // konstruktor klasy - przyjmuje wektor pozycji i wektor poczatkowej predkosci
    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

        // definicja zdarzenia powiadamiajacaego, ktore wywolywane jest gdy pozycja kulki sie zmieni
    public event EventHandler<IVector>? NewPositionNotification;

    public IVector Velocity { get; set; }

    #endregion IBall

    #region private

        // wektor pola kulki
    private Vector Position;
        
        // handler do mowienia innym wartstwom o zmiany pozycji
    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

        // definicja poruszania sie kulki
    internal void Move(Vector delta)
    {
      Position = new Vector(Position.x + delta.x, Position.y + delta.y);
      RaiseNewPositionChangeNotification();
    }

    #endregion private

    #region public

    // Właściwość publiczna dostępna tylko do odczytu
    public IVector getPosition
    {
        get { return Position; }
    }

    #endregion public
    }
}