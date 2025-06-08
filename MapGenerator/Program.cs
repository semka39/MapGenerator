namespace MapGenerator
{
    using System.Drawing;

    internal class Program
    {
        static void Main(string[] args)
        {
            int bright_step = 3;
            int width  = 400;
            int height = 400;

            // Количество итераций сглаживания
            int iterations = 100;
            //Шумность карты (0 - 1)
            double noise   = 0.4;

            double[,] map = new double[width, height];
            bool[,] rmap = new bool[width, height];
            Random rnd = new Random();

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

            void Render(string filename)
            {
                Bitmap bmp = new Bitmap(width, height);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double normalizedHeight = map[x, y];
                        byte brightness = (byte)(normalizedHeight * 255);
                        Color c = Color.FromArgb(brightness, brightness, brightness);

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
                //Render();
            }

            Render("image.png");
        }
    }
}
