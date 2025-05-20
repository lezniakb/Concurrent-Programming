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
        // watki z kulkami
        private readonly List<Thread> BallThreads = new();
        private List<Ball> BallsList = new();
        private readonly object lockObject = new object();
        // pomiary czasu
        private readonly Stopwatch globalTimer = new Stopwatch();
        private readonly Dictionary<int, long> ballLastUpdateTime = new Dictionary<int, long>();
        // diagnostyka
        private readonly ConcurrentQueue<string> diagnosticsQueue = new ConcurrentQueue<string>();
        private readonly Task diagnosticsWriterTask;
        private readonly string diagnosticsFilePath = "../../../../DaneDiagnostyczne.txt";
        private readonly int maxQueueSize = 1000;
        private readonly ManualResetEvent diagnosticsEvent = new ManualResetEvent(false);

        #endregion Fields
        #region ctor

        public DataImplementation()
        {
            // konstruktor uruchamia pomiar czasu oraz tworzy zadanie do diagnostyki
            globalTimer.Start();
            diagnosticsWriterTask = Task.Factory.StartNew(DiagnosticsWriter, TaskCreationOptions.LongRunning);
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            // dispozing obiektu
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            Random random = new Random();
            // dla kazdej kulki
            for (int i = 0; i < numberOfBalls; i++)
            {
                // tworzona jest nowa pozycja
                Vector startingPosition = new Vector(
                    random.Next((int)(screenWidth * 0.1), screenWidth - (int)(screenWidth * 0.1)),
                    random.Next((int)(screenHeight * 0.1), screenHeight - (int)(screenHeight * 0.1))
                );
                // ..i nowa predkosc
                Vector velocity = new Vector(
                    (random.NextDouble() - 0.5) * 6,
                    (random.NextDouble() - 0.5) * 6
                );

                // tworzenie obiektu
                Ball newBall = new Ball(startingPosition, velocity);
                upperLayerHandler(startingPosition, newBall);

                // dodajemy do listy
                BallsList.Add(newBall);

                // zapisz poczatkowy czas kulki 
                ballLastUpdateTime[newBall.Id] = globalTimer.ElapsedMilliseconds;
                
                // uruchomienie watku
                var worker = new BallWorker(newBall, screenWidth, screenHeight, BallDiameter, lockObject, BallsList,
                    () => Disposed, LogDiagnostics, ballLastUpdateTime, globalTimer);
                Thread thread = new Thread(worker.Run);
                BallThreads.Add(thread);
                thread.Start();
            }

            // dodanie informacji o rozpoczeciu dzialania programu
            LogDiagnostics($"Program uruchomiony z {numberOfBalls} kulami. Czas: {globalTimer.ElapsedMilliseconds}ms");
        }

        // ustawienie rozmiaru ekranu
        public override void SetScreenSize(double width, double height)
        {
            screenWidth = (int)width;
            screenHeight = (int)height;
        }

        #endregion DataAbstractAPI

        #region IDisposable

        // przechowywanie stanu obiektu
        protected virtual void Dispose(bool disposing)
        {
            // sprawdzanie czy obiekt nie zostal juz zwolniony
            if (!Disposed)
            {
                // zwolnienie zasobow
                if (disposing)
                {
                    Disposed = true;

                    foreach (var thread in BallThreads)
                        thread.Join();

                    // zatrzymanie zapisu diagnostyki
                    diagnosticsEvent.Set();
                    // czekaj na zakonczenie watku
                    diagnosticsWriterTask.Wait(1000);
                    // zapisz pozostale dane w kolejce
                    FlushDiagnosticsQueue();
                    // wyczysc liste kulek
                    BallsList.Clear();
                }
            }
            else { throw new ObjectDisposedException(nameof(DataImplementation)); }
        }


        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        // srednica kulki widziana ze strony logicznej != wyswietlana)
        private const double BallDiameter = 40.0;

        // metoda do logowania danych diagnostycznych
        private void LogDiagnostics(string message)
        {
            // znacznik czasu
            string timestampedMessage = $"[{globalTimer.ElapsedMilliseconds}ms] {message}";

            // umieszczanie wiadomosci w kolejce jesli jest miejsce
            if (diagnosticsQueue.Count < maxQueueSize)
            {
                diagnosticsQueue.Enqueue(timestampedMessage);
                diagnosticsEvent.Set(); // Powiadom wątek zapisujący
            }
            else
            {
                // jesli kolejka jest pelna to odrzucamy wiadomosc
                Debug.WriteLine($"Kolejka diagnostyczna przepelniona. Utracono wiadomość {timestampedMessage}");
            }
        }

        // zapis danych diagnostycznych do pliku
        private void DiagnosticsWriter()
        {
            using (StreamWriter writer = new StreamWriter(diagnosticsFilePath, false, Encoding.ASCII))
            {
                writer.WriteLine($"=== Symulacja kulek - rozpoczeto {DateTime.Now} ===");
                writer.Flush();

                // zapis diagnostyki w petli
                while (!Disposed || !diagnosticsQueue.IsEmpty)
                {
                    if (diagnosticsQueue.IsEmpty)
                    {
                        diagnosticsEvent.Reset();
                        // czekaj na kolejne dane lub zakoncz
                        diagnosticsEvent.WaitOne(500);
                        continue;
                    }

                    // wez dane z kolejki
                    if (diagnosticsQueue.TryDequeue(out string message))
                    {
                        try
                        {
                            writer.WriteLine(message);
                            writer.Flush();

                            // symulacja ograniczonej przepustowosci zapisu
                            Thread.Sleep(1);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"blad zapisu danych diagnostycznych: {ex.Message}");
                        }
                    }
                }

                writer.WriteLine($"=== Symulacja kulek - zakonczono {DateTime.Now} ===");
            }
        }

        // wymus zapis wszystkich danych z kolejki
        private void FlushDiagnosticsQueue()
        {
            try
            {
                // zapisz pozostale dane z kolejki
                using (StreamWriter writer = new StreamWriter(diagnosticsFilePath, true, Encoding.ASCII))
                {
                    while (diagnosticsQueue.TryDequeue(out string message))
                    {
                        writer.WriteLine(message);
                    }
                    writer.WriteLine($"=== Zakonczenie programu: {DateTime.Now} ===");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Blad przy konczeniu zapisu diagnostyki: {ex.Message}");
            }
        }

        // klasa worker do obslugi kulki
        private class BallWorker
        {
            // pola prywatne do obslugi kulki
            private readonly Ball ball;
            private readonly int screenWidth;
            private readonly int screenHeight;
            private readonly double ballDiameter;
            private readonly object lockObject;
            private readonly List<Ball> ballsList;
            private readonly Func<bool> isDisposed;
            // diagnostyka i timer
            private readonly Action<string> logDiagnostics;
            private readonly Dictionary<int, long> ballLastUpdateTime;
            private readonly Stopwatch globalTimer;

            // zmienne do programowania czasu rzeczywistego
            private readonly Stopwatch frameTimer = new Stopwatch();
            // 60fps
            private const int TARGET_FRAME_TIME = 16;

            // konstruktor
            public BallWorker(Ball ball, int screenWidth, int screenHeight, double ballDiameter,
                object lockObject, List<Ball> ballsList, Func<bool> isDisposed,
                Action<string> logDiagnostics, Dictionary<int, long> ballLastUpdateTime,
                Stopwatch globalTimer)
            {
                // przypisanie wartosci do pol
                this.ball = ball;
                this.screenWidth = screenWidth;
                this.screenHeight = screenHeight;
                this.ballDiameter = ballDiameter;
                this.lockObject = lockObject;
                this.ballsList = ballsList;
                this.isDisposed = isDisposed;
                this.logDiagnostics = logDiagnostics;
                this.ballLastUpdateTime = ballLastUpdateTime;
                this.globalTimer = globalTimer;

                this.logDiagnostics($"Nowa kula id={this.ball.Id}, pozycja: ({this.ball.getPosition.x:F2}, {this.ball.getPosition.y:F2}), " +
                             $"predkosc: ({this.ball.Velocity.x:F2}, {this.ball.Velocity.y:F2})");
            }

            // metoda uruchamiająca watek
            public void Run()
            {
                // uruchomienie timera
                frameTimer.Start();
                int updateCount = 0;

                // petla aktualizujaca pozycje kulki
                while (!isDisposed())
                {
                    frameTimer.Restart();

                    // obliczenie czasu aktualizacji
                    long currentTime = globalTimer.ElapsedMilliseconds;
                    long lastUpdateTime;

                    // aktualizacja czasu
                    lock (ballLastUpdateTime)
                    {
                        lastUpdateTime = ballLastUpdateTime[ball.Id];
                        ballLastUpdateTime[ball.Id] = currentTime;
                    }

                    // zamiana na sekundy
                    double deltaTime = (currentTime - lastUpdateTime) / 1000.0;

                    // ograniczenie deltaTime aby sie kulki nie teleportowaly (max 50ms)
                    deltaTime = Math.Min(deltaTime, 0.05);

                    // aktualizacja pozycji i kolizji w zamku
                    lock (lockObject)
                    {
                        UpdatePosition(deltaTime);
                        HandleCollisions();

                        // logowanie diagnostyki co 30 aktualizacji
                        if (++updateCount % 30 == 0)
                        {
                            logDiagnostics($"Kula id={ball.Id}, pozycja: ({ball.getPosition.x:F2}, {ball.getPosition.y:F2}), " +
                                        $"predkosc: ({ball.Velocity.x:F2}, {ball.Velocity.y:F2}), deltaTime: {deltaTime:F5}s");
                        }
                    }

                    // obliczenie czasu ramki
                    long frameTime = frameTimer.ElapsedMilliseconds;

                    // obliczenie czasu snu
                    int sleepTime = (int)Math.Max(1, TARGET_FRAME_TIME - frameTime);

                    // logowanie diagnostyki gdy przkroczono czasu ramki
                    if (frameTime > TARGET_FRAME_TIME && updateCount % 10 == 0)
                    {
                        logDiagnostics($"Kula id={ball.Id} !!! przekroczenie czasu ramki: {frameTime}ms (cel: {TARGET_FRAME_TIME}ms)");
                    }

                    // czekanie na nastepna klatke
                    Thread.Sleep(sleepTime);
                }

                logDiagnostics($"Zakonczono kule id={ball.Id}");
            }

            // aktualizacja pozycji kulki
            private void UpdatePosition(double deltaTime)
            {
                double posX = ball.getPosition.x;
                double posY = ball.getPosition.y;

                //programowanie czasu rzeczywistego, wykorzystanie deltaTime do obliczenia nowej pozycji
                double velX = ball.Velocity.x;
                double velY = ball.Velocity.y;

                // skalowanie do standardowego czasu ramki
                double newX = posX + velX * deltaTime * 60;
                double newY = posY + velY * deltaTime * 60;

                // ograniczenie pozycji do ekranu
                if (newX < 0 || newX > screenWidth - ballDiameter)
                {
                    velX = -velX;
                    // Fix: Ensure the maximum value is greater than the minimum value
                    double maxX = Math.Max(0, screenWidth - ballDiameter);
                    newX = Math.Clamp(newX, 0, maxX);
                    logDiagnostics($"Kula, id={ball.Id} ___ odbicie od poziomej sciany");
                }

                if (newY < 0 || newY > screenHeight - ballDiameter)
                {
                    velY = -velY;
                    // Fix: Ensure the maximum value is greater than the minimum value
                    double maxY = Math.Max(0, screenHeight - ballDiameter);
                    newY = Math.Clamp(newY, 0, maxY);
                    logDiagnostics($"Kula, id={ball.Id} ||| odbicie od pionowej sciany");
                }

                // aktualizacja pozycji i predkosci
                ball.Velocity = new Vector(velX, velY);
                Vector delta = new Vector(newX - posX, newY - posY);
                // uzyj funkcji move do przesuniecia
                ball.Move(delta);
            }

            // obsluga kolizji z innymi kulkami
            private void HandleCollisions()
            {
                // dla kazdej kulki w liscie
                foreach (var other in ballsList)
                {
                    // sprawdz czy to nie ta sama kulka
                    if (other == ball)
                        continue;
                    // sprawdz czy kulki sie stykaja
                    double dx = other.getPosition.x - ball.getPosition.x;
                    double dy = other.getPosition.y - ball.getPosition.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    //  obliczanie kolizji
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

                        // obliczanie nowych predkosci
                        Vector vel1 = new Vector(
                            ball.Velocity.x + impulse * mass2 * nx,
                            ball.Velocity.y + impulse * mass2 * ny);

                        Vector vel2 = new Vector(
                            other.Velocity.x - impulse * mass1 * nx,
                            other.Velocity.y - impulse * mass1 * ny);

                        // aktualizacja predkosci
                        ball.Velocity = vel1;
                        other.Velocity = vel2;

                        double overlap = ballDiameter - distance;
                        double separationX = nx * overlap / 2;
                        double separationY = ny * overlap / 2;

                        // przesunięcie kul
                        ball.Move(new Vector(-separationX, -separationY));
                        other.Move(new Vector(separationX, separationY));

                        // diagnostyka
                        logDiagnostics($"<33 Kolizja kula id={ball.Id} z kula id={other.Id}; " +
                                      $"predkosci po kolizji: ({vel1.x:F2}, {vel1.y:F2}) i ({vel2.x:F2}, {vel2.y:F2})");
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