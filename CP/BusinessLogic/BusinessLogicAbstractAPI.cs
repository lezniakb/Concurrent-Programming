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
   // abstrakcyjna klasa API implementująca interfejs Disposable
  public abstract class BusinessLogicAbstractAPI : IDisposable
  {
    #region Layer Factory

    // fabryka - zwraca instancję warstwy biznesowej
    public static BusinessLogicAbstractAPI GetBusinessLogicLayer()
    {
      return modelInstance.Value;
    }

    #endregion Layer Factory

    #region Layer API

        // statyczna wartosc, rozmiary kulki
    public static readonly Dimensions GetDimensions = new(10.0, 10.0, 10.0);

        // abstrakcyjna metoda, którą implementują klasy pochodne. 
        // int liczba piłek, oraz handler który przekazuje info (poz. i referencję do ballsa) do warstwy prezentacji
    public abstract void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler);

    #region IDisposable

        // implementacja interfejsu Disposable - do zarządzania zasobmi
    public abstract void Dispose();

    #endregion IDisposable

    #endregion Layer API

    #region private

        // statyczna zmienna (prywatna) która tworzy instancję Business Logic Implementation
    private static Lazy<BusinessLogicAbstractAPI> modelInstance = new Lazy<BusinessLogicAbstractAPI>(() => new BusinessLogicImplementation());

    #endregion private
  }
  /// <summary>
  /// Immutable type representing table dimensions
  /// </summary>
  /// <param name="BallDimension"></param>
  /// <param name="TableHeight"></param>
  /// <param name="TableWidth"></param>
  /// <remarks>
  /// Must be abstract
  /// </remarks>
  /// 
  //niemodyfikowalny rekord z wpisanymi wymiarami kulki i wysokosci "okna"
  public record Dimensions(double BallDimension, double TableHeight, double TableWidth);
    
    public interface IPosition
  {
        // zdefiniowanie "init", ze rozmiar mozna ustawic tylko przy inicjalizacji
    double x { get; init; }
    double y { get; init; }
  }

    // okreslenie interfejsu dla kulki ze zdarzeniem NewPositionNotification
    // przy wywolaniu przekazuje informacje (notification) o nowej pozycji
  public interface IBall 
  {
    event EventHandler<IPosition> NewPositionNotification;
  }
}