using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region fields
        // rozmiar ekranu
        private int screenWidth;
        private int screenHeight;
        private bool Disposed = false;
        // wątki z kulkami
        private readonly List<Thread> BallThreads = new();
        private List<Ball> BallsList = new();
        private readonly object lockObject = new object();
        // diagnostyka
        private readonly ConcurrentQueue<string> diagnosticsQueue = new ConcurrentQueue<string>();
        private readonly Task diagnosticsWriterTask;
        private readonly string diagnosticsFilePath = "../../../../DaneDiagnostyczne.txt";
        private readonly int maxQueueSize = 1000;
        private readonly ManualResetEvent diagnosticsEvent = new ManualResetEvent(false);
        // pomiar czasu tylko do diagnostyki
        private readonly Stopwatch globalTimer = new Stopwatch();
        #endregion Fields

        #region ctor
        public DataImplementation()
        {
            // Uruchamiamy stopwatch do diagnostyki
            globalTimer.Start();
            diagnosticsWriterTask = Task.Factory.StartNew(DiagnosticsWriter, TaskCreationOptions.LongRunning);
        }
        #endregion ctor

        #region DataAbstractAPI
        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();

            for (int i = 0; i < numberOfBalls; i++)
            {
                // tworzona jest nowa pozycja
                Vector startingPosition = new Vector(
                    random.Next((int)(screenWidth * 0.1), screenWidth - (int)(screenWidth * 0.1)),
                    random.Next((int)(screenHeight * 0.1), screenHeight - (int)(screenHeight * 0.1))
                );
                // ..i nowa prędkość (piksele na klatkę)
                Vector velocity = new Vector(
                    (random.NextDouble() - 0.5) * 6,
                    (random.NextDouble() - 0.5) * 6
                );

                // tworzenie obiektu
                Ball newBall = new Ball(startingPosition, velocity);
                upperLayerHandler(startingPosition, newBall);

                BallsList.Add(newBall);

                // uruchomienie wątku dla tej kulki
                var worker = new BallWorker(
                    newBall,
                    screenWidth,
                    screenHeight,
                    BallDiameter,
                    lockObject,
                    BallsList,
                    () => Disposed,
                    LogDiagnostics,
                    globalTimer
                );
                Thread thread = new Thread(worker.Run);
                BallThreads.Add(thread);
                thread.Start();
            }

            // dodanie informacji o rozpoczęciu działania programu
            LogDiagnostics($"Program uruchomiony z {numberOfBalls} kulami. Czas: {globalTimer.ElapsedMilliseconds}ms");
        }

        public override void SetScreenSize(double width, double height)
        {
            screenWidth = (int)width;
            screenHeight = (int)height;
        }
        #endregion DataAbstractAPI

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Disposed = true;

                    foreach (var thread in BallThreads)
                        thread.Join();

                    // zatrzymanie zapisu diagnostyki
                    diagnosticsEvent.Set();
                    diagnosticsWriterTask.Wait(1000);
                    FlushDiagnosticsQueue();
                    BallsList.Clear();
                }
            }
            else
            {
                throw new ObjectDisposedException(nameof(DataImplementation));
            }
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable

        #region private
        private const double BallDiameter = 40.0;

        private void LogDiagnostics(string message)
        {
            string timestampedMessage = $"[{globalTimer.ElapsedMilliseconds}ms] {message}";
            if (diagnosticsQueue.Count < maxQueueSize)
            {
                diagnosticsQueue.Enqueue(timestampedMessage);
                diagnosticsEvent.Set();
            }
            else
            {
                Debug.WriteLine($"Kolejka diagnostyczna przepełniona. Utracono wiadomość {timestampedMessage}");
            }
        }

        private void DiagnosticsWriter()
        {
            using (StreamWriter writer = new StreamWriter(diagnosticsFilePath, false, Encoding.ASCII))
            {
                writer.WriteLine($"=== Symulacja kulek - rozpoczęto {DateTime.Now} ===");
                writer.Flush();

                while (!Disposed || !diagnosticsQueue.IsEmpty)
                {
                    if (diagnosticsQueue.IsEmpty)
                    {
                        diagnosticsEvent.Reset();
                        diagnosticsEvent.WaitOne(500);
                        continue;
                    }

                    if (diagnosticsQueue.TryDequeue(out string message))
                    {
                        try
                        {
                            writer.WriteLine(message);
                            writer.Flush();
                            Thread.Sleep(1); // symulacja ograniczonej przepustowości zapisu
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Błąd zapisu danych diagnostycznych: {ex.Message}");
                        }
                    }
                }

                writer.WriteLine($"=== Symulacja kulek - zakończono {DateTime.Now} ===");
            }
        }

        private void FlushDiagnosticsQueue()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(diagnosticsFilePath, true, Encoding.ASCII))
                {
                    while (diagnosticsQueue.TryDequeue(out string message))
                    {
                        writer.WriteLine(message);
                    }
                    writer.WriteLine($"=== Zakończenie programu: {DateTime.Now} ===");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd przy kończeniu zapisu diagnostyki: {ex.Message}");
            }
        }

        // Klasa worker do obsługi kulki (ruch niezależny od upływu czasu)
        private class BallWorker
        {
            private readonly Ball ball;
            private readonly int screenWidth;
            private readonly int screenHeight;
            private readonly double ballDiameter;
            private readonly object lockObject;
            private readonly List<Ball> ballsList;
            private readonly Func<bool> isDisposed;
            private readonly Action<string> logDiagnostics;
            private readonly Stopwatch globalTimer;

            // dla stałego klatkowania
            private const int TARGET_FRAME_TIME = 16; // ~60 FPS

            private int updateCount = 0;

            public BallWorker(
                Ball ball,
                int screenWidth,
                int screenHeight,
                double ballDiameter,
                object lockObject,
                List<Ball> ballsList,
                Func<bool> isDisposed,
                Action<string> logDiagnostics,
                Stopwatch globalTimer)
            {
                this.ball = ball;
                this.screenWidth = screenWidth;
                this.screenHeight = screenHeight;
                this.ballDiameter = ballDiameter;
                this.lockObject = lockObject;
                this.ballsList = ballsList;
                this.isDisposed = isDisposed;
                this.logDiagnostics = logDiagnostics;
                this.globalTimer = globalTimer;

                this.logDiagnostics($"Nowa kula id={this.ball.Id}, pozycja: ({this.ball.getPosition.x:F2}, {this.ball.getPosition.y:F2}), " +
                                    $"prędkość: ({this.ball.Velocity.x:F2}, {this.ball.Velocity.y:F2})");
            }

            public void Run()
            {
                while (!isDisposed())
                {
                    // Aktualizacja pozycji i kolizji (stały krok)
                    lock (lockObject)
                    {
                        UpdatePosition();
                        HandleCollisions();

                        if (++updateCount % 30 == 0)
                        {
                            logDiagnostics($"Kula id={ball.Id}, pozycja: ({ball.getPosition.x:F2}, {ball.getPosition.y:F2}), " +
                                           $"prędkość: ({ball.Velocity.x:F2}, {ball.Velocity.y:F2})");
                        }
                    }

                    Thread.Sleep(TARGET_FRAME_TIME); // czekanie na następną klatkę (~16ms)
                }

                logDiagnostics($"Zakończono kulę id={ball.Id}");
            }

            private void UpdatePosition()
            {
                double posX = ball.getPosition.x;
                double posY = ball.getPosition.y;

                double velX = ball.Velocity.x;
                double velY = ball.Velocity.y;

                // STAŁY krok: przesunięcie o prędkość bez skalowania
                double newX = posX + velX;
                double newY = posY + velY;

                if (newX < 0 || newX > screenWidth - ballDiameter)
                {
                    velX = -velX;
                    double maxX = Math.Max(0, screenWidth - ballDiameter);
                    newX = Math.Clamp(newX, 0, maxX);
                    logDiagnostics($"Kula id={ball.Id} ___ odbicie od poziomej ściany");
                }

                if (newY < 0 || newY > screenHeight - ballDiameter)
                {
                    velY = -velY;
                    double maxY = Math.Max(0, screenHeight - ballDiameter);
                    newY = Math.Clamp(newY, 0, maxY);
                    logDiagnostics($"Kula id={ball.Id} ||| odbicie od pionowej ściany");
                }

                ball.Velocity = new Vector(velX, velY);
                Vector delta = new Vector(newX - posX, newY - posY);
                ball.Move(delta);
            }

            private void HandleCollisions()
            {
                foreach (var other in ballsList)
                {
                    if (other == ball)
                        continue;

                    double dx = other.getPosition.x - ball.getPosition.x;
                    double dy = other.getPosition.y - ball.getPosition.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance < ballDiameter)
                    {
                        double mass1 = Math.Pow(ballDiameter, 2);
                        double mass2 = Math.Pow(ballDiameter, 2);
                        double nx = dx / distance;
                        double ny = dy / distance;

                        double dvx = other.Velocity.x - ball.Velocity.x;
                        double dvy = other.Velocity.y - ball.Velocity.y;

                        double dotProduct = dvx * nx + dvy * ny;
                        if (dotProduct > 0) return;

                        double impulse = (2 * dotProduct) / (mass1 + mass2);

                        Vector vel1 = new Vector(
                            ball.Velocity.x + impulse * mass2 * nx,
                            ball.Velocity.y + impulse * mass2 * ny);

                        Vector vel2 = new Vector(
                            other.Velocity.x - impulse * mass1 * nx,
                            other.Velocity.y - impulse * mass1 * ny);

                        ball.Velocity = vel1;
                        other.Velocity = vel2;

                        double overlap = ballDiameter - distance;
                        double separationX = nx * overlap / 2;
                        double separationY = ny * overlap / 2;

                        ball.Move(new Vector(-separationX, -separationY));
                        other.Move(new Vector(separationX, separationY));

                        logDiagnostics($"<33 Kolizja kula id={ball.Id} z kula id={other.Id}; " +
                                       $"prędkości po kolizji: ({vel1.x:F2}, {vel1.y:F2}) i ({vel2.x:F2}, {vel2.y:F2})");
                    }
                }
            }
        }
        #endregion private

        #region TestingInfrastructure
        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
        #endregion TestingInfrastructure
    }
}
