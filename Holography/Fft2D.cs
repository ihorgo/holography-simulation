using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interferometry
{
    /// <summary>
    /// Комплексное число
    /// </summary>
    public struct ComplexNumber
    {
        /// <summary>
        /// Действительная часть
        /// </summary>
        public double RealPart;

        /// <summary>
        /// Мнимая часть
        /// </summary>
        public double ImaginaryPart;

        /// <summary>
        /// Конструктор
        /// </summary>
        public ComplexNumber(double x, double y)
        {
            RealPart = x;
            ImaginaryPart = y;
        }

        /// <summary>
        /// Модуль
        /// </summary>
        public double Magnitude
        {
            get { return Math.Sqrt(RealPart * RealPart + ImaginaryPart * ImaginaryPart); }
        }

        /// <summary>
        /// Фаза
        /// </summary>
        public double Phase
        {
            get 
            {
                if (RealPart == 0)
                {
                    if (ImaginaryPart == 0) return 0;
                    return ImaginaryPart > 0 ? System.Math.PI / 2 : -System.Math.PI / 2;
                }
                return Math.Atan(ImaginaryPart / RealPart); 
            }
        }

        public ComplexNumberExp ToComplexNumberExp
        {
            get { return new ComplexNumberExp(Magnitude, Phase); }
        }

        public static ComplexNumber operator +(ComplexNumber n1, ComplexNumber n2)
        {
            return new ComplexNumber(n1.RealPart + n2.RealPart, n1.ImaginaryPart + n2.ImaginaryPart);
        }

        public static ComplexNumber operator -(ComplexNumber n1, ComplexNumber n2)
        {
            return new ComplexNumber(n1.RealPart - n2.RealPart, n1.ImaginaryPart - n2.ImaginaryPart);
        }

        public static ComplexNumber operator *(ComplexNumber n1, ComplexNumber n2)
        {
            double rp = n1.RealPart * n2.RealPart - n1.ImaginaryPart * n2.ImaginaryPart;
            double ip = n1.ImaginaryPart * n2.RealPart + n1.RealPart * n2.ImaginaryPart;
            return new ComplexNumber(rp, ip);
        }

        public static ComplexNumber operator /(ComplexNumber n1, ComplexNumber n2)
        {
            double d = n2.RealPart * n2.RealPart + n2.ImaginaryPart * n2.ImaginaryPart;
            double rp = (n1.RealPart * n2.RealPart + n1.ImaginaryPart * n2.ImaginaryPart) / d;
            double ip = (n1.ImaginaryPart * n2.RealPart - n1.RealPart * n2.ImaginaryPart) / d;
            return new ComplexNumber(rp, ip);
        }

        public static ComplexNumber operator +(ComplexNumber n1, ComplexNumberExp n2)
        {
            return new ComplexNumber(n1.RealPart + n2.RealPart, n1.ImaginaryPart + n2.ImaginaryPart);
        }

        public static ComplexNumber operator -(ComplexNumber n1, ComplexNumberExp n2)
        {
            return new ComplexNumber(n1.RealPart - n2.RealPart, n1.ImaginaryPart - n2.ImaginaryPart);
        }

        public static ComplexNumber operator *(ComplexNumber n1, ComplexNumberExp n2)
        {
            double rp = n1.RealPart * n2.RealPart - n1.ImaginaryPart * n2.ImaginaryPart;
            double ip = n1.ImaginaryPart * n2.RealPart + n1.RealPart * n2.ImaginaryPart;
            return new ComplexNumber(rp, ip);
        }

        public static ComplexNumber operator /(ComplexNumber n1, ComplexNumberExp n2)
        {
            double d = n2.RealPart * n2.RealPart + n2.ImaginaryPart * n2.ImaginaryPart;
            double rp = (n1.RealPart * n2.RealPart + n1.ImaginaryPart * n2.ImaginaryPart) / d;
            double ip = (n1.ImaginaryPart * n2.RealPart - n1.RealPart * n2.ImaginaryPart) / d;
            return new ComplexNumber(rp, ip);
        }

        public static ComplexNumber operator /(ComplexNumber n1, double n2)
        {
            return new ComplexNumber(n1.RealPart / n2, n1.ImaginaryPart / n2);
        }
    }

    /// <summary>
    /// Комплексное число в экспоненциальной форме
    /// </summary>
    public struct ComplexNumberExp
    {
        public double Magnitude;
        public double Phase;

        public ComplexNumberExp(double magnitude, double phase)
        {
            Magnitude = magnitude;
            Phase = phase;
        }

        public ComplexNumberExp(double phase)
        {
            Magnitude = 1.0;
            Phase = phase;
        }

        public ComplexNumberExp(ComplexNumber num)
        {
            Magnitude = num.Magnitude;
            Phase = num.Phase;
        }

        public ComplexNumber ToComplexNumber
        {
            get { return new ComplexNumber(RealPart, ImaginaryPart); }
        }

        public double RealPart
        {
            get { return Magnitude * System.Math.Cos(Phase); }
        }

        public double ImaginaryPart
        {
            get { return Magnitude * System.Math.Sin(Phase); }
        }

        public static ComplexNumberExp operator +(ComplexNumberExp n1, ComplexNumberExp n2)
        {
            return new ComplexNumberExp(n1.ToComplexNumber + n2.ToComplexNumber);
        }

        public static ComplexNumberExp operator -(ComplexNumberExp n1, ComplexNumberExp n2)
        {
            return new ComplexNumberExp(n1.ToComplexNumber - n2.ToComplexNumber);
        }

        public static ComplexNumberExp operator *(ComplexNumberExp n1, ComplexNumberExp n2)
        {
            return new ComplexNumberExp(n1.Magnitude * n2.Magnitude, n1.Phase + n2.Phase);
        }

        public static ComplexNumberExp operator /(ComplexNumberExp n1, ComplexNumberExp n2)
        {
            return new ComplexNumberExp(n1.Magnitude / n2.Magnitude, n1.Phase - n2.Phase);
        }

        public static ComplexNumberExp operator +(ComplexNumberExp n1, ComplexNumber n2)
        {
            return new ComplexNumberExp(n1.ToComplexNumber + n2);
        }

        public static ComplexNumberExp operator -(ComplexNumberExp n1, ComplexNumber n2)
        {
            return new ComplexNumberExp(n1.ToComplexNumber - n2);
        }

        public static ComplexNumberExp operator *(ComplexNumberExp n1, ComplexNumber n2)
        {
            return new ComplexNumberExp(n1.Magnitude * n2.Magnitude, n1.Phase + n2.Phase);
        }

        public static ComplexNumberExp operator /(ComplexNumberExp n1, ComplexNumber n2)
        {
            return new ComplexNumberExp(n1.Magnitude / n2.Magnitude, n1.Phase - n2.Phase);
        }

        public static ComplexNumberExp operator /(ComplexNumberExp n1, double n2)
        {
            return new ComplexNumberExp(n1.Magnitude / n2, n1.Phase);
        }
    }

    /// <summary>
    /// 2-мерное Быстрое Преобразование Фурье
    /// </summary>
    static public class Fft2D
    {
        /// <summary>
        /// Реализует преобразование (http://www.codeproject.com/KB/GDI/FFT.aspx)
        /// </summary>
        static public ComplexNumber[,] Perform(ComplexNumber[,] c, int nx, int ny, bool isForward)
        {
            int i, j;
            int m;//Power of 2 for current number of points
            double[] real;
            double[] imag;
            ComplexNumber[,] output = new ComplexNumber[nx, ny];
            //output = c; // Copying Array
            // Transform the Rows 
            real = new double[nx];
            imag = new double[nx];

            for (j = 0; j < ny; j++)
            {
                for (i = 0; i < nx; i++)
                {
                    real[i] = c[i, j].RealPart;
                    imag[i] = c[i, j].ImaginaryPart;
                }
                // Calling 1D FFT Function for Rows
                m = (int)Math.Log((double)nx, 2);//Finding power of 2 for current number of points e.g. for nx=512 m=9
                FFT1D(isForward, m, ref real, ref imag);

                for (i = 0; i < nx; i++)
                {
                    output[i, j].RealPart = real[i];
                    output[i, j].ImaginaryPart = imag[i];
                }
            }
            // Transform the columns  
            real = new double[ny];
            imag = new double[ny];

            for (i = 0; i < nx; i++)
            {
                for (j = 0; j < ny; j++)
                {
                    real[j] = output[i, j].RealPart;
                    imag[j] = output[i, j].ImaginaryPart;
                }
                // Calling 1D FFT Function for Columns
                m = (int)Math.Log((double)ny, 2);//Finding power of 2 for current number of points e.g. for nx=512 m=9
                FFT1D(isForward, m, ref real, ref imag);
                for (j = 0; j < ny; j++)
                {
                    output[i, j].RealPart = real[j];
                    output[i, j].ImaginaryPart = imag[j];
                }
            }

            // clear memory
            GC.Collect();

            return output;
        }

        /// <summary>
        ///    http://www.codeproject.com/KB/GDI/FFT.aspx
        /// 
        ///    This computes an in-place complex-to-complex FFT
        ///    x and y are the real and imaginary arrays of 2^m points.
        ///    Formula: forward
        ///             N-1
        ///              ---
        ///            1 \         - j k 2 pi n / N
        ///    X(K) = --- > x(n) e                  = Forward transform
        ///            N /                            n=0..N-1
        ///              ---
        ///             n=0
        ///    Formula: reverse
        ///             N-1
        ///             ---
        ///             \          j k 2 pi n / N
        ///    X(n) =    > x(k) e                  = Inverse transform
        ///             /                             k=0..N-1
        ///             ---
        ///             k=0
        /// </summary>
        static private void FFT1D(bool isForward, int m, ref double[] x, ref double[] y)
        {
            long nn, i, i1, j, k, i2, l, l1, l2;
            double c1, c2, tx, ty, t1, t2, u1, u2, z;
            
            /* Calculate the number of points */
            nn = 1;
            for (i = 0; i < m; i++) nn *= 2;

            /* Do the bit reversal */
            i2 = nn >> 1;
            j = 0;
            for (i = 0; i < nn - 1; i++)
            {
                if (i < j)
                {
                    tx = x[i];
                    ty = y[i];
                    x[i] = x[j];
                    y[i] = y[j];
                    x[j] = tx;
                    y[j] = ty;
                }
                k = i2;
                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            /* Compute the FFT */
            c1 = -1.0;
            c2 = 0.0;
            l2 = 1;
            for (l = 0; l < m; l++)
            {
                l1 = l2;
                l2 <<= 1;
                u1 = 1.0;
                u2 = 0.0;
                for (j = 0; j < l1; j++)
                {
                    for (i = j; i < nn; i += l2)
                    {
                        i1 = i + l1;
                        t1 = u1 * x[i1] - u2 * y[i1];
                        t2 = u1 * y[i1] + u2 * x[i1];
                        x[i1] = x[i] - t1;
                        y[i1] = y[i] - t2;
                        x[i] += t1;
                        y[i] += t2;
                    }
                    z = u1 * c1 - u2 * c2;
                    u2 = u1 * c2 + u2 * c1;
                    u1 = z;
                }
                c2 = Math.Sqrt((1.0 - c1) / 2.0);
                if (!isForward) c2 = -c2;
                c1 = Math.Sqrt((1.0 + c1) / 2.0);
            }

            /* Scaling for forward transform */
            if (!isForward)
            //double sqrt_n = System.Math.Sqrt((double)nn);
            {
                for (i = 0; i < nn; i++)
                {
                    x[i] /= (double)nn;// sqrt_n;
                    y[i] /= (double)nn;// sqrt_n;
                }
            }
        }
    }
}
