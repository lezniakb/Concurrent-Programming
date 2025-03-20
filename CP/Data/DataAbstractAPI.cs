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
    // abstrakcyjna klasa implementujaca interfejs Disposable (nie moze byc utworzona wprost)
  public abstract class DataAbstractAPI : IDisposable
  {
    #region Layer Factory

        // umozlwienie dostepu do instancji
    public static DataAbstractAPI GetDataLayer()
    {
      return modelInstance.Value;
    }

    #endregion Layer Factory

    #region public API

        // abstrakcyjna metoda ktora musi byc zaimplementowana w klase pochodnej
    public abstract void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler);

    #endregion public API

    #region IDisposable

        // zarzadzanie zasobami - implementacja interfejsu Disposable
    public abstract void Dispose();

    #endregion IDisposable

    #region private

        // uzycie klasy Lazy, ktora sprawia ze instancja jest tworzona gdy program probuje sie odwolac do modelInstance.
    private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());

    #endregion private
  }

  public interface IVector
  {
    /// <summary>
    /// The X component of the vector.
    /// </summary>
    double x { get; init; }

    /// <summary>
    /// The y component of the vector.
    /// </summary>
    double y { get; init; }
  }

  public interface IBall
  {
        // powiadamianie o zmianie stanu
    event EventHandler<IVector> NewPositionNotification;

    IVector Velocity { get; set; }
  }
}