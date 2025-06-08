namespace MapGenerator
{
    using System.Diagnostics;
    using System.Drawing;

    internal class Program
    {
        static void Main(string[] args)
        {
            int width = 400;
            int height = 400;

            // Количество итераций сглаживания
            int iterations = 100;
            //Шумность первичного паттерна (0 - 1)
            double noise = 0.3;

            double[,] map = new double[width, height];
            bool[,] rmap = new bool[width, height];
            Random rnd = new Random();

            //презагружаем градиент
            Bitmap gradient = new Bitmap("gradient-1.png");
            Color[] gradientColors = new Color[gradient.Width];
            int gradientColorsCount = gradient.Width;
            for (int x = 0; x < gradient.Width; x++)
            {
                gradientColors[x] = gradient.GetPixel(x, 0);
            }

            //функция для нормализации карты высот
            void Normilize()
            {
                //Нормализуем карту высот
                double maxHeight = -1;
                foreach (double cellHeight in map)
                {
                    if (cellHeight > maxHeight)
                    {
                        maxHeight = cellHeight;
                    }
                }
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        map[x, y] /= maxHeight;
                    }
                }
            }

            //функция для отрисовки карты высот в файл
            void Render(string filename)
            {
                Bitmap bmp = new Bitmap(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double normalizedHeight = map[x, y];
                        int colorIndex = (int)(normalizedHeight * (gradientColorsCount - 1));
                        Color c = gradientColors[colorIndex];

                        bmp.SetPixel(x, y, c);
                    }
                }

                bmp.Save(filename);
            }

            //Случайное заполнение карты заполнености
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    rmap[x, y] = rnd.NextDouble() >= noise;
                }
            }

            //Случайное заполнение карты высот
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = rnd.NextDouble();
                }
            }

            //Сглаживание карты высот
            for (int i = 0; i < iterations; i++)
            {
                double[,] bmap = new double[width, height];

                //перераспределение высот
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        double neighborSum = 0;
                        bool isCurentCellFilled = rmap[x, y];
                        List<(int x, int y)> Neighbords = new List<(int x, int y)>();

                        //Считаем среднюю высоту на поле 3 на 3
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                int ox = (x + dx + width) % width;
                                int oy = (y + dy + height) % height;

                                bool isOtherCellFilled = rmap[ox, oy];
                                if (isCurentCellFilled || isOtherCellFilled || (x == ox && y == oy))
                                {
                                    Neighbords.Add((ox, oy));
                                    neighborSum += map[ox, oy];
                                }
                            }
                        }

                        double addHeight = neighborSum / Neighbords.Count;

                        //Изменяем новую карту
                        foreach (var (ox, oy) in Neighbords)
                        {
                            bmap[ox, oy] += addHeight;
                        }
                    }
                }

                map = bmap;
                Normilize(); // Нормализуем карту после каждой итерации сглаживания
            }

            string filename = "image.png";
            Render(filename);
            Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
        }
    }
}
