//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {
    #region ctor

        // domyslny konstruktor z parametrem null
    public BusinessLogicImplementation() : this(null)
    { }

        // konstruktor wewnetrzny, ktory moze przyjac instancje z API
    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
            // jesli instancja jest null to wywolaj fabryke danych
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
    }

    #endregion ctor

    #region BusinessLogicAbstractAPI

        // zarzadzanie zasobami
    public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      layerBellow.Dispose();
      Disposed = true;
    }

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
    {
      // sprawdza czy obiekt nie zostal zwolniony i czy nie zostal przekazany null
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));
      // jesli wyjatki nie zostaly rzucone to przyjmij argumenty startingPosition i databall (warstaw dancyh)
      // utworz obiekt Position (BusinessLogic.Position)
      // utworz obiekt Ball przekazujac jako argument databall z warstwy danych
      // wywolaj upperLayerHandler przez ktory informacje ida do warstwy prezentacji
      layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.x, startingPosition.x), new Ball(databall)));
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

    private readonly UnderneathLayerAPI layerBellow;

    #endregion private

    #region TestingInfrastructure

        // sprawdza w trybie debug czy zwolil sie obiekt
    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

        #endregion TestingInfrastructure

        #region public

        public override void SetScreenSize(double width, double height)
        {
            layerBellow?.SetScreenSize(width, height);
        }

        #endregion public
    }
}