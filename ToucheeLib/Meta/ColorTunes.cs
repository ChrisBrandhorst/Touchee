using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using System.Drawing;

namespace Touchee.Meta {

    public class ArtworkColors {

        public Color BackgroundColor { get; protected set; }
        public Color ForegroundColor { get; protected set; }
        public Color ForegroundColor2 { get; protected set; }

        Bitmap _bitmap;


        public static ArtworkColors Generate(Image image) {
            return new ArtworkColors(image);
        }


        ArtworkColors(Image image) {
            _bitmap = new Bitmap(image);

            this.GetColorMap(
                (int)Math.Ceiling(image.Width * .25),
                0,
                (int)Math.Ceiling(image.Width * .75),
                (int)Math.Ceiling(image.Height * .05),
                4
            );
            //this.GetColorMap(
            //    0,
            //    0,
            //    image.Width,
            //    image.Height,
            //    4
            //);
        }


        Color[] GetColorMap(int sx, int sy, int width, int height, int maxColors) {

            var s = DateTime.Now;


            var colors = new Color[width * height];
            for (int j = 0; j < height; j++) {
                for (int i = 0; i < width; i++) {
                    colors[j * width + i] = _bitmap.GetPixel(sx + i, sy + j);
                }
            }


            var boxes = MMCQ.Quantize(colors, maxColors);
            
            Console.WriteLine((DateTime.Now - s).TotalMilliseconds.ToString());

            foreach (var c in boxes.Select(b => b.GetAverage())) {
                Console.WriteLine(c.ToString());
            }

            return null;
        }




        class MMCQ {

            public const int SIGBITS = 5;
            public const int RSHIFT = 8 - SIGBITS;

            static int _maxIterations = 1000;
            static double _fractByPopulations = .75;

            public static IEnumerable<ColorBox> Quantize(Color[] pixels, int maxColors) {

                if (pixels.Length == 0)
                    throw new ArgumentException("Pixel array is empty");
                if (maxColors < 2 || maxColors > 256)
                    throw new ArgumentOutOfRangeException("Argument maxColors should be at least 2 and maximally 256");

                var histogram = GetHistogram(pixels);
                var colorBox = ColorBox.Create(pixels, histogram);

                var queue = new PriorityQueue( (a, b) => a.GetCount().CompareTo(b.GetCount()) );
                queue.Push(colorBox);
                Iter(queue, _fractByPopulations * maxColors, histogram);

                var queue2 = new PriorityQueue( (a, b) => (a.GetCount() * a.GetVolume()).CompareTo(b.GetCount() * b.GetVolume()) );
                while (queue.Count > 0) {
                    queue2.Push(queue.Pop());
                }
                Iter(queue2, maxColors - queue2.Count, histogram);

                var boxes = new List<ColorBox>();
                while (queue2.Count > 0) {
                    boxes.Add(queue2.Pop());
                }
                return boxes;
            }


            static int[] GetHistogram(Color[] pixels) {
                var size = 1 << (3 * SIGBITS);
                var histogram = new int[size];
                int index;

                foreach (var pixel in pixels) {
                    index = MMCQ.GetColorIndex(
                        pixel.R >> RSHIFT,
                        pixel.G >> RSHIFT,
                        pixel.B >> RSHIFT
                    );
                    histogram[index] = histogram[index] + 1;
                }

                return histogram;
            }


            internal static int GetColorIndex(int r, int g, int b) {
                return (r << (2 * SIGBITS)) + (g << SIGBITS) + b;
            }


            static void Iter(PriorityQueue queue, double target, int[] histogram) {

                int nIter = 0, nColors = 1;
                ColorBox box, box1, box2;
                ColorBox[] boxes;

                while (nIter < _maxIterations) {
                    box = queue.Pop();
                    if (box.GetCount() == 0) {
                        queue.Push(box);
                        nIter++;
                        continue;
                    }
                    boxes = MedianCutApply(histogram, box);
                    box1 = boxes[0];
                    box2 = boxes[1];
                    if (box1 == null) {
                        Console.WriteLine("box1 not defined; shouldn't happen");
                        return;
                    }
                    queue.Push(box1);
                    if (box2 != null) {
                        queue.Push(box2);
                        nColors++;
                    }
                    if (nColors >= target)
                        return;
                    if (nIter++ > _maxIterations) {
                        Console.WriteLine("Infinite loop; perhaps too few pixels");
                        return;
                    }
                }

            }


            static ColorBox[] MedianCutApply(int[] histogram, ColorBox box) {

                if (box.GetCount() == 0)
                    return null;
                if (box.GetCount() == 1)
                    return new ColorBox[2]{box.Copy(), null};

                int rw = box.R2 - box.R1 + 1,
                    gw = box.G2 - box.G1 + 1,
                    bw = box.B2 - box.B1 + 1,
                    maxw = Math.Max(rw, Math.Max(gw, bw)),
                    total = 0,
                    sum, index;

                var partialSum = new Dictionary<int,int>();
                var lookAheadSum = new Dictionary<int, int>();

                if (maxw == rw) {
                    for (int r = box.R1; r <= box.R2; r++) {
                        sum = 0;
                        for (int g = box.G1; g <= box.G2; g++) {
                            for (int b = box.B1; b <= box.B2; b++) {
                                index = MMCQ.GetColorIndex(r, g, b);
                                sum += histogram[index];
                            }
                        }
                        total += sum;
                        partialSum[r] = total;
                    }
                }

                else if (maxw == gw) {
                    for (int g = box.G1; g <= box.G2; g++) {
                        sum = 0;
                        for (int r = box.R1; r <= box.R2; r++) {
                            for (int b = box.B1; b <= box.B2; b++) {
                                index = MMCQ.GetColorIndex(r, g, b);
                                sum += histogram[index];
                            }
                        }
                        total += sum;
                        partialSum[g] = total;
                    }
                }

                else if (maxw == bw) {
                    for (int b = box.B1; b <= box.B2; b++) {
                        sum = 0;
                        for (int r = box.R1; r <= box.R2; r++) {
                            for (int g = box.G1; g <= box.G2; g++) {
                                index = MMCQ.GetColorIndex(r, g, b);
                                sum += histogram[index];
                            }
                        }
                        total += sum;
                        partialSum[b] = total;
                    }
                }

                var keys = partialSum.Keys.ToList();
                keys.Sort();
                for (int i = 0; i < keys.Count; i++) {
                    lookAheadSum[keys[i]] = total - partialSum[keys[i]];
                }

                if (maxw == rw) {
                    return DoCut(box, partialSum, lookAheadSum, total, 'r');
                }
                if (maxw == gw) {
                    return DoCut(box, partialSum, lookAheadSum, total, 'g');
                }
                if (maxw == bw) {
                    return DoCut(box, partialSum, lookAheadSum, total, 'b');
                }

                return null;
            }


            static ColorBox[] DoCut(ColorBox box, Dictionary<int, int> partialSum, Dictionary<int, int> lookAheadSum, int total, char color) {

                int dim1 = 0, dim2 = 0;
                switch (color) {
                    case 'r':
                        dim1 = box.R1;
                        dim2 = box.R2;
                        break;
                    case 'g':
                        dim1 = box.G1;
                        dim2 = box.G2;
                        break;
                    case 'b':
                        dim1 = box.B1;
                        dim2 = box.B2;
                        break;
                }

                for (int i = dim1; i <= dim2; i++) {
                    if (partialSum[i] > (total / 2)) {
                        ColorBox box1 = box.Copy(),
                                 box2 = box.Copy();
                        int left = i - dim1,
                            right = dim2 - i,
                            d2;
                        if (left <= right) {
                            d2 = Math.Min(dim2 - 1, ~~(i + right / 2));
                        }
                        else {
                            d2 = Math.Max(dim1, ~~(i - 1 - left / 2));
                        }
                        while (partialSum[d2] == 0)
                            d2++;
                        int count2 = lookAheadSum[d2];
                        while (count2 == 0 && partialSum[d2 - 1] > 0)
                            count2 = lookAheadSum[--d2];

                        switch (color) {
                            case 'r':
                                box1.R2 = d2;
                                box2.R1 = d2 + 1;
                                break;
                            case 'g':
                                box1.G2 = d2;
                                box2.G1 = d2 + 1;
                                break;
                            case 'b':
                                box1.B2 = d2;
                                box2.B1 = d2 + 1;
                                break;
                        }

                        Console.WriteLine("cbox counts: " + (box.GetCount()) + ", " + (box1.GetCount()) + ", " + (box2.GetCount()));

                        return new ColorBox[2] { box1, box2 };
                    }
                }
                return null;
            }



        }




        class ColorBox {

            int _volume, _count;
            bool _count_set, _volume_set, _average_set;
            int[] _histogram;
            Color _average;

            public int R1 { get; set; }
            public int R2 { get; set; }
            public int G1 { get; set; }
            public int G2 { get; set; }
            public int B1 { get; set; }
            public int B2 { get; set; }

            public ColorBox(int r1, int r2, int g1, int g2, int b1, int b2, int[] histogram) {
                R1 = r1;
                R2 = r2;
                G1 = g1;
                G2 = g2;
                B1 = b1;
                B2 = b2;
                _histogram = histogram;
            }

            public static ColorBox Create(Color[] pixels, int[] histogram) {
                int rmin = 1000000, rmax = 0,
                    gmin = 1000000, gmax = 0,
                    bmin = 1000000, bmax = 0,
                    r, b, g;

                foreach (var pixel in pixels) {
                    r = pixel.R >> MMCQ.RSHIFT;
                    g = pixel.G >> MMCQ.RSHIFT;
                    b = pixel.B >> MMCQ.RSHIFT;
                    if (r < rmin) {
                        rmin = r;
                    }
                    else if (r > rmax) {
                        rmax = r;
                    }
                    if (g < gmin) {
                        gmin = g;
                    }
                    else if (g > gmax) {
                        gmax = g;
                    }
                    if (b < bmin) {
                        bmin = b;
                    }
                    else if (b > bmax) {
                        bmax = b;
                    }
                }

                return new ColorBox(rmin, rmax, gmin, gmax, bmin, bmax, histogram);
            }

            public int GetVolume(bool forced = false) {
                if (!_volume_set || forced) {
                    _volume = (R2 - R1 + 1) * (G2 - G1 + 1) * (B2 - B1 + 1);
                    _volume_set = true;
                }
                return _volume;
            }

            public int GetCount(bool forced = false) {
                if (!_count_set || forced) {
                    int numpix = 0, index;

                    for (int r = R1; r <= R2; r++) {
                        for (int g = G1; g <= G2; g++) {
                            for (int b = B1; b <= B2; b++) {
                                index = MMCQ.GetColorIndex(r, g, b);
                                numpix += _histogram[index];
                            }
                        }
                    }
                    _count_set = true;
                    _count = numpix;
                }
                return _count;
            }

            public ColorBox Copy() {
                return new ColorBox(R1, R2, G1, G2, B1, B2, _histogram);
            }

            public Color GetAverage(bool forced = false) {
                if (!_average_set || forced) {
                    int mult = 1 << (8 - MMCQ.SIGBITS), total = 0, rsum = 0, gsum = 0, bsum = 0, index, hval;
                    for (int r = R1; r <= R2; r++) {
                        for (int g = G1; g <= G2; g++) {
                            for (int b = B1; b <= B2; b++) {
                                index = MMCQ.GetColorIndex(r, g, b);
                                hval = _histogram[index];
                                total += hval;
                                rsum += (int)(hval * (r + .5) * mult);
                                gsum += (int)(hval * (g + .5) * mult);
                                bsum += (int)(hval * (b + .5) * mult);
                            }
                        }
                    }
                    if (total > 0) {
                        _average = Color.FromArgb( ~~(rsum / total), ~~(gsum / total), ~~(bsum / total) );
                    }
                    else {
                        _average = Color.FromArgb(
                            ~~(mult * (R1 + R2 + 1) / 2),
                            ~~(mult * (G1 + G2 + 1) / 2),
                            ~~(mult * (B1 + B2 + 1) / 2)
                        );
                    }
                }
                _average_set = true;
                return _average;
            }

        }




        class PriorityQueue : List<ColorBox> {

            Comparison<ColorBox> _comparison;
            bool _sorted = false;

            public PriorityQueue(Comparison<ColorBox> comparison) {
                _comparison = comparison;
            }

            bool DoSort() {
                if (!_sorted)
                    this.Sort(_comparison);
                return _sorted = true;
            }

            public bool Push(ColorBox box) {
                this.Add(box);
                return _sorted = false;
            }

            public ColorBox Pop() {
                this.DoSort();
                var box = this[this.Count - 1];
                this.RemoveAt(this.Count - 1);
                return box;
            }

        }



    }

}
