//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic
{
  // definicja klasy Ball, implementuje interfejs IBall
  internal class Ball : IBall
  {
    // przekazanie obiektu IBall z klasy Data
    public Ball(Data.IBall ball)
    {
      // subskrypcja zdarzenia 
      ball.NewPositionNotification += RaisePositionChangeEvent;
    }

    #region IBall

    public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private

    // gdy zmienia się pozycja wywoływana jest ta funkcja
    // sender to obiekt który zgłosił zmianę, IVector to wektor danych nowej pozycji (x, y)
    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      // znak zapytania czy zdarzenie nie jest null (nie ma subskrybentów)
      // jesli jest null to zdarzenie jest pomijane
      // jesli nie jest null to metoda wywoluje wszystkich subskrybentow
      NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
    }

    #endregion private
  }
}