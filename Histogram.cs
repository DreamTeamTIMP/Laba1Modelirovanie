using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Raylib_CSharp.Windowing;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Transformations;

namespace Laba1.Histogram;

/// <summary>
/// Класс, управляющий окном с гистограммой в отдельном потоке.
/// </summary>
public class HistogramWindow : IDisposable
{
    private readonly int _width;
    private readonly int _height;
    private readonly string _title;
    
    private Thread _windowThread = null!;
    private CancellationTokenSource _cts = null!;
    private ConcurrentQueue<(double[] data, int barsCount)> _dataQueue; // для обновления данных из основного потока
    private double[] histogramData;
    private double histogramMin;
    private double histogramMax;

    /// <summary>Создаёт окно гистограммы (но не запускает его).</summary>
    public HistogramWindow(int width, int height, string title)
    {
        _width = width;
        _height = height;
        _title = title;
        _dataQueue = new ConcurrentQueue<(double[], int)>();
        histogramData = [0.2f, 0.5f, 0.8f, 0.3f, 0.9f, 0.4f, 0.6f]; // данные по умолчанию
    }

    /// <summary>Запускает окно в отдельном потоке.</summary>
    public void Start()
    {
        if (_windowThread != null && _windowThread.IsAlive)
            throw new InvalidOperationException("Окно уже запущено.");

        _cts = new CancellationTokenSource();
        _windowThread = new Thread(RunRaylibLoop);
        _windowThread.Start();
    }

    /// <summary>Обновляет данные гистограммы (потокобезопасно).</summary>
    /// <param name="newData">Новый массив значений (0..1).</param>
    public void UpdateData(double[] newData, int barsCount)
    {
        // Кладём копию массива, чтобы избежать изменений извне
        var newDataCopy = (double[])newData.Clone();
        _dataQueue.Enqueue((newDataCopy, barsCount));
    }

    /// <summary>Закрывает окно и останавливает поток.</summary>
    public void Close()
    {
        _cts?.Cancel();
        // Не ждём принудительно завершения потока, чтобы избежать deadlock'а,
        // но можно добавить ожидание с таймаутом, если нужно.
    }

    /// <summary>Освобождение ресурсов.</summary>
    public void Dispose()
    {
        Close();
    }

    // Метод, выполняющийся в отдельном потоке
    private void RunRaylibLoop()
    {
        // Отключаем лишние логи
        Raylib_CSharp.Logging.Logger.SetTraceLogLevel(Raylib_CSharp.Logging.TraceLogLevel.Warning);
        // Инициализация окна
        Window.Init(_width, _height, _title);
        // Устанавливаем целевую частоту кадров

        // Основной цикл Raylib
        while (!Window.ShouldClose() && !_cts.Token.IsCancellationRequested)
        {
            // Проверяем, есть ли новые данные для отображения
            if (_dataQueue.TryDequeue(out var update))
            {
                UpdateHistogramData(update.data, update.barsCount);
            }

            // Отрисовка
            Graphics.BeginDrawing();
            Graphics.ClearBackground(Color.RayWhite);



            DrawHistogram(histogramData, histogramMin, histogramMax);

            Graphics.EndDrawing();
        }

        // Закрываем окно
        Window.Close();
    }

    private void UpdateHistogramData(double[] inputData, int barsCount)
    {
        double[] data = new double[barsCount];
        
        double max = inputData[0];
        double min = inputData[0];
        for (int i = 0; i < inputData.Length; ++i)
        {
            if (inputData[i] > max) max = inputData[i];
            if (inputData[i] < min) min = inputData[i];
        }

        double step = (max - min) / barsCount;
        for (int i = 0; i < inputData.Length; ++i)
        {
            int index = (int) ((inputData[i] - min) / step);
            if (index >= barsCount) 
            {
                Console.WriteLine(index);
                index = barsCount - 1;
            }
            data[index] += 1;
        }

        histogramData = data;
        histogramMax = max;
        histogramMin = min;
    } 

    private void DrawHistogram(double[] data, double minX, double maxX)
    {
        int barsCount = data.Length;
        int margin = 50;
        float graphWidth = _width - 2 * margin;
        float graphHeight = _height - 2 * margin;
        
        float barXMargin = 200f / data.Length;

        // Ширина столбца с плавающей точкой
        float barWidth = graphWidth / barsCount - barXMargin;
        if (barWidth < 1) barWidth = 1;

        // Находим максимальную частоту
        double maxFreq = double.MinValue;
        foreach (var val in data)
            if (val > maxFreq) maxFreq = val;
        if (maxFreq <= 0) maxFreq = 1;

        float stepX = (float)((maxX - minX) / barsCount);

        // Рисуем столбцы
        for (int i = 0; i < barsCount; i++)
        {
            float x = margin + i * (barWidth + barXMargin);
            float barHeight = (float)(data[i] * graphHeight / maxFreq);
            float y = _height - margin - barHeight;

            Graphics.DrawRectangleRec(new Rectangle(x, y, barWidth, barHeight), Color.Blue);
        }

        // Подписи по оси X (снизу)
        int maxLabelsX = 10;
        int labelStepX = Math.Max(1, barsCount / maxLabelsX);
        for (int i = 0; i < barsCount; i += labelStepX)
        {
            double xValue = minX + i * stepX;
            string xLabel = xValue.ToString("0.00");
            float xPos = margin + i * (barWidth + barXMargin);
            Graphics.DrawText(xLabel, (int)xPos, _height - margin + 5, 10, Color.DarkGray);
        }
        // Подпись правого края
        string rightLabel = maxX.ToString("0.00");
        float lastX = margin + barsCount * (barWidth + barXMargin) - 5f;
        Graphics.DrawText(rightLabel, (int)lastX, _height - margin + 5, 10, Color.DarkGray);

        // ПОДПИСИ ПО ОСИ Y (СЛЕВА) — значения частот от 0 до maxFreq
        int yLabelsCount = 5; // можно изменить при необходимости
        for (int j = 0; j <= yLabelsCount; j++)
        {
            double freqValue = maxFreq * j / yLabelsCount;
            // Позиция Y, соответствующая этому значению частоты
            float yPos = _height - margin - (float)(freqValue * graphHeight / maxFreq);
            
            // Форматируем подпись (целые числа, если freqValue целое, иначе с одним знаком)
            string yLabel = freqValue.ToString("0.##");
            
            // Рисуем текст слева от графика (с небольшим отступом)
            Graphics.DrawText(yLabel, (int)(margin - 40), (int)(yPos - 5), 10, Color.DarkGray);
            
            // Опционально: можно добавить тонкую линию сетки
            // Graphics.DrawLine(margin - 5, yPos, margin, yPos, Color.LightGray);
        }
    }
}