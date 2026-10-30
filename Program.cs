using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Media;

namespace WebcamPhoto
{
    class Program
    {
        static void Main(string[] args)
        {
            // Создаем путь к подкаталогу IMG_HD_44577 в папке Изображения
            string picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string saveFolder = Path.Combine(picturesFolder, "IMG_HD_44577");

            // Создаем подкаталог, если его нет
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            Console.WriteLine("Фото с камеры (тестировалось Logi C270 HD WebCam)");
            Console.WriteLine($"Папка сохранения: {saveFolder}");
            Console.WriteLine("Будет сделано 3 снимка с интервалом 315 мс");
            Console.WriteLine("Инициализация камеры...\n");

            using (var capture = new VideoCapture(0))
            {
                if (!capture.IsOpened())
                {
                    Console.WriteLine("ERROR: Камера не найдена");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("Камера готова. Идёт съёмка...\n");

                // Прогрев камеры: захватываем "пустой" кадр для активации автофокуса
                using (var warmupFrame = new Mat())
                {
                    capture.Read(warmupFrame);
                }
                // Ждём завершения автофокусировки и автоэкспозиции
                Thread.Sleep(900);

                // Делаем 3 снимка
                for (int i = 1; i <= 3; i++)
                {
                    Console.WriteLine($"Снимок {i} из 3...");

                    using (var frame = new Mat())
                    {
                        // Захватываем кадр
                        capture.Read(frame);

                        if (frame.Empty())
                        {
                            Console.WriteLine($"  ОШИБКА: Не удалось захватить кадр {i}");
                            continue;
                        }

                        using (var bitmap = BitmapConverter.ToBitmap(frame))
                        {
                            // Создаем имя файла с номером снимка
                            string baseFileName = $"photo_{DateTime.Now:yyyyMMdd_HHmmss}_{i}";
                            string filePath = Path.Combine(saveFolder, baseFileName);

                            // Обрезаем фото под соотношение 3:4
                            using (var croppedBitmap = CropToRatio34(bitmap))
                            {
                                string croppedPath = $"{filePath}_cropped.jpg";
                                croppedBitmap.Save(croppedPath, ImageFormat.Jpeg);
                                Console.WriteLine($"  Обрезанное (3:4): {croppedPath}");
                            }
                        }
                    }

                    // Задержка между снимками (кроме последнего)
                    if (i < 3)
                    {
                        Thread.Sleep(315);
                    }
                }
            }
            // Звуковой сигнал
            Console.Beep(1000, 200); // частота 1000Гц, длительность 200мс
                                     // сообщение выхода
            Console.WriteLine("\nГотово! Сделано 3 снимка.");
            Console.WriteLine($"Все фото сохранены в: {saveFolder}");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        /// <summary>
        /// Обрезает изображение до соотношения сторон 3:4 (ширина:высота)
        /// Обрезка происходит по центру
        /// </summary>
        static System.Drawing.Bitmap CropToRatio34(System.Drawing.Bitmap sourceImage)
        {
            int originalWidth = sourceImage.Width;
            int originalHeight = sourceImage.Height;

            // Целевое соотношение: ширина 3, высота 4
            double targetRatio = 3.0 / 4.0; // 0.75

            double currentRatio = (double)originalWidth / originalHeight;

            int newWidth, newHeight, cropX, cropY;

            if (currentRatio > targetRatio)
            {
                // Изображение шире, чем нужно - обрезаем по ширине
                newHeight = originalHeight;
                newWidth = (int)(newHeight * targetRatio);
                cropX = (originalWidth - newWidth) / 2;
                cropY = 0;
            }
            else
            {
                // Изображение выше, чем нужно - обрезаем по высоте
                newWidth = originalWidth;
                newHeight = (int)(newWidth / targetRatio);
                cropX = 0;
                cropY = (originalHeight - newHeight) / 2;
            }

            // Создаем новый bitmap нужного размера
            var croppedBitmap = new System.Drawing.Bitmap(newWidth, newHeight);
            using (var graphics = System.Drawing.Graphics.FromImage(croppedBitmap))
            {
                graphics.DrawImage(sourceImage,
                    new System.Drawing.Rectangle(0, 0, newWidth, newHeight),
                    new System.Drawing.Rectangle(cropX, cropY, newWidth, newHeight),
                    GraphicsUnit.Pixel);
            }

            return croppedBitmap;
        }
    }
}
