using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        #region ctor

        public MainWindowViewModel() : this(null)
        { }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI;
            CreateBallsCommand = new RelayCommand(() => Start(InitialBallsNumber));
        }

        #endregion ctor

        #region public API

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));

            if (ModelLayer == null)
            {
                ModelLayer = ModelAbstractApi.CreateModel();
            }

            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));

            ModelLayer.Start(numberOfBalls);

        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        public int InitialBallsNumber
        {
            get { 
                return _initialBallsNumber; 
            }
            set { 
                _initialBallsNumber = value; RaisePropertyChanged("InitialBallsNumber"); 
            }
        }

        public ICommand CreateBallsCommand { get; private set; }

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer?.Dispose();
                    ModelLayer?.Dispose();
                }

                Disposed = true;
            }
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer = null;
        private bool Disposed = false;
        private int _initialBallsNumber;

        #endregion private

        #region public

        public void SetScreenSize(double width, double height)
        {
            ModelLayer?.SetScreenSize(width, height);
        }

        #endregion public
    }
}
