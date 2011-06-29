using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Interferometry
{
    public class FourierProcessor
    {
        private InterferometryDataProvider m_interferometryDataProvider = null;
        private BitmapImage m_lastImageMagnitude = null;
        private BitmapImage m_lastImagePhase = null;

        private BitmapSource m_fresnelPhaseImage;
        private BitmapSource m_fresnelAmplitudeImage;
        private BitmapSource m_fourierAmplitudeImage;
        private BitmapSource m_hologramImage;
        private BitmapSource m_restoredImage;
        private BitmapSource m_restoredImagePhase;

        public FourierProcessor(InterferometryDataProvider provider)
        {
            m_interferometryDataProvider = provider;
            m_interferometryDataProvider.UpdateInterferometryDataHandler += new OnUpdateInterferometryData(m_interferometryDataProvider_UpdateInterferometryDataHandler);
        }

        ~FourierProcessor()
        {
            m_interferometryDataProvider.UpdateInterferometryDataHandler -= new OnUpdateInterferometryData(m_interferometryDataProvider_UpdateInterferometryDataHandler);
        }


        public BitmapSource FresnelPhaseImage
        {
            get { return m_fresnelPhaseImage; }
        }

        public BitmapSource FresnelAmplitudeImage
        {
            get { return m_fresnelAmplitudeImage; }
        }

        public BitmapSource FourierAmplitudeImage
        {
            get { return m_fourierAmplitudeImage; }
        }

        public BitmapSource HologramImage
        {
            get { return m_hologramImage; }
        }

        public BitmapSource RestoredImage
        {
            get { return m_restoredImage; }
        }

        public BitmapSource RestoredImagePhase
        {
            get { return m_restoredImagePhase; }
        }

        public string FresnelAmplitudeComment = "";
        public string FresnelPhaseComment = "";
        public string FourierAmplitudeComment = "";
        public string HoloComment = "";

        /// <summary>
        /// Делегат на завершение расчетов
        /// </summary>
        public delegate void OnFinishComputing();
        public event OnFinishComputing OnFinishComputingHandler;

        public void Process(BitmapImage imgMagnitude, BitmapImage imgPhase)
        {
            m_lastImageMagnitude = imgMagnitude;
            if (m_lastImageMagnitude == null) return;

            m_lastImagePhase = imgPhase;
            if (m_lastImagePhase != null)
            {
                // Проверка совпадения по размерам амплитуды и фазы
                if ((imgMagnitude.PixelWidth != imgPhase.PixelWidth) ||
                    (imgMagnitude.PixelHeight != imgPhase.PixelHeight))
                {
                    throw new Exception("Размеры битмэпов амплитуды и фазы не совпадают");
                }
            }

            // Квадратность
            int width = imgMagnitude.PixelWidth;
            int height = imgMagnitude.PixelHeight;
            if (width != height)
            {
                throw new Exception("Ширина и высота битмэпов совпадают");
            }

            // Степень 2
            bool check = false;
            for (int k = 2; k <= 8192; k *= 2)
                if (k == width) { check = true; break; }
            if (!check) throw new Exception("Ширина и высота битмэпов не является степенью 2");

            // Получаем массив пикселей из битмэпа амплитуды
            int bytePerPixelMagn = imgMagnitude.Format.BitsPerPixel / 8;
            int nStride = imgMagnitude.PixelWidth * bytePerPixelMagn;
            byte[] pixelByteArrayMagn = new byte[imgMagnitude.PixelHeight * nStride];
            imgMagnitude.CopyPixels(pixelByteArrayMagn, nStride, 0);

            // Получаем массив пикселей из битмэпа фазы, если она задана
            byte[] pixelByteArrayPhase = null;
            int bytePerPixelPhase = 0;
            if (imgPhase != null)
            {
                bytePerPixelPhase = imgPhase.Format.BitsPerPixel / 8;
                nStride = imgPhase.PixelWidth * bytePerPixelPhase;
                pixelByteArrayPhase = new byte[imgPhase.PixelHeight * nStride];
                imgPhase.CopyPixels(pixelByteArrayPhase, nStride, 0);
            }

            ComplexNumber[,] values_fresnel = new ComplexNumber[width, height];
            ComplexNumber[,] values_fourier = new ComplexNumber[width, height];
            double[,] values_hologram = new double[width, height];
            double[,] values_restored = new double[width, height];

            // Параметры симуляции
            double z1 = m_interferometryDataProvider.HoloZRecord;
            double z2 = m_interferometryDataProvider.HoloZ;
            double alpha1 = m_interferometryDataProvider.HoloAlphaRecord * System.Math.PI / 180.0;
            double alpha2 = m_interferometryDataProvider.HoloAlphaReconstructed * System.Math.PI / 180.0;
            double lyambda = m_interferometryDataProvider.HoloLyambda;
            double dx = m_interferometryDataProvider.HoloPixelSize;
            bool suppressDcTerm = m_interferometryDataProvider.HoloSuppressDcTerm;
            bool combineInTheCenter = m_interferometryDataProvider.HoloCombineInTheCenter;

            // Постоянная часть уравнения плоской волны для записи голограммы
            double planeWaveKRecord = 2.0 * System.Math.PI * System.Math.Sin(alpha1) / lyambda;
            // Постоянная часть уравнения плоской волны для восстановления
            double planeWaveK = 2.0 * System.Math.PI * System.Math.Sin(alpha2) / lyambda;

            double frp = System.Math.PI * dx * dx / (lyambda * z1);
            double frp2 = System.Math.PI / lyambda * z1;
            double frp_reconstructed = System.Math.PI * dx * dx / (lyambda * z2);
            double frp2_reconstructed = System.Math.PI / lyambda * z2;

            // Подинтегральная часть преобразования
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = bytePerPixelMagn * (i * width + j);
                    double c = (double)pixelByteArrayMagn[index];
                    double p = 0;
                    if (pixelByteArrayPhase != null)
                    {
                        index = bytePerPixelPhase * (i * width + j);
                        p = System.Math.PI * (double)pixelByteArrayPhase[index] / 255.0; // [0-Pi]
                    }

                    values_fresnel[j, i] = (new ComplexNumberExp(c, p + frp * (i * i + j * j))).ToComplexNumber;
                    values_fourier[j, i] = (new ComplexNumberExp(c, p)).ToComplexNumber;
                }
            }

            ComplexNumber[,] fft_fresnel = Fft2D.Perform(values_fresnel, width, height, true);
            ComplexNumber[,] fft_fourier = Fft2D.Perform(values_fourier, width, height, true);

            // Прединтегральная часть преобразования
            double magn_max = double.MinValue;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    fft_fresnel[j, i] *= new ComplexNumberExp(frp * (i * i + j * j) + frp2);

                    double new_magn = fft_fresnel[j, i].Magnitude;
                    if (new_magn > magn_max) magn_max = new_magn;
                }
            }

            // form hologram
            double hologram_max = double.MinValue;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double aa = -planeWaveKRecord * (i + j);
                    ComplexNumber cv = fft_fresnel[j, i] + new ComplexNumberExp(magn_max, aa);
                    double magn = cv.Magnitude;
                    values_hologram[j, i] = magn * magn;
                    if (values_hologram[j, i] > hologram_max) hologram_max = values_hologram[j, i];
                }
            }
            double holo_avg = 0.0;
            if (suppressDcTerm)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        values_hologram[j, i] = values_hologram[j, i] / hologram_max;
                        holo_avg += values_hologram[j, i];
                    }
                }
                holo_avg /= (height * width);
            }

            // reconstruction
            ComplexNumber[,] values_hologram2 = new ComplexNumber[width, height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double aa2 = -planeWaveKRecord * (i + j);
                    ComplexNumberExp cv = new ComplexNumberExp(magn_max, aa2);
                    
                    double magn = (values_hologram[j, i] - holo_avg);
                    values_hologram2[j, i] = (new ComplexNumberExp(magn, frp_reconstructed * (i * i + j * j)) * cv).ToComplexNumber;
                }
            }
            ComplexNumber[,] ifft_holo = Fft2D.Perform(values_hologram2, width, height, true);

            ComplexNumber[,] reconstructed = new ComplexNumber[width, height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    ComplexNumberExp c = new ComplexNumberExp(frp_reconstructed * (i * i + j * j) + frp2_reconstructed);
                    reconstructed[j, i] = ifft_holo[j, i] * c;
                }
            }

            #region Заполнение битмапов для вывода графики

            // Вычисление максимумов и минимумов выводимых значений для перевода в простраство [0..1]
            double fresnel_phase_max = double.MinValue;
            double fresnel_phase_min = double.MaxValue;
            double fresnel_magnitude_max = double.MinValue;
            double fresnel_magnitude_min = double.MaxValue;
            double fourier_magnitude_max = double.MinValue;
            double fourier_magnitude_min = double.MaxValue;
            double holo_max = double.MinValue;
            double holo_min = double.MaxValue;
            double restored_max = double.MinValue;
            double restored_min = double.MaxValue;
            double restored_max_phase = double.MinValue;
            double restored_min_phase = double.MaxValue;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double frn_magn = fft_fresnel[j, i].Magnitude;
                    double frn_ph = fft_fresnel[j, i].Phase;
                    if (frn_ph < fresnel_phase_min) fresnel_phase_min = frn_ph;
                    if (frn_ph > fresnel_phase_max) fresnel_phase_max = frn_ph;
                    if (frn_magn < fresnel_magnitude_min) fresnel_magnitude_min = frn_magn;
                    if (frn_magn > fresnel_magnitude_max) fresnel_magnitude_max = frn_magn;

                    double four_magn = fft_fourier[j, i].Magnitude;
                    if (four_magn < fourier_magnitude_min) fourier_magnitude_min = four_magn;
                    if (four_magn > fourier_magnitude_max) fourier_magnitude_max = four_magn;

                    double holo = values_hologram[j, i];
                    if (holo < holo_min) holo_min = holo;
                    if (holo > holo_max) holo_max = holo;

                    double restored = reconstructed[j, i].Magnitude;
                    if (restored < restored_min) restored_min = restored;
                    if (restored > restored_max) restored_max = restored;

                    double restored_phase = reconstructed[j, i].Phase;
                    if (restored_phase < restored_min_phase) restored_min_phase = restored_phase;
                    if (restored_phase > restored_max_phase) restored_max_phase = restored_phase;
                }
            }
            FresnelAmplitudeComment = "Мин(мм) = " + fresnel_magnitude_min.ToString("0.0000") + "\nМакс(мм) = " + fresnel_magnitude_max.ToString("0.0000");
            FresnelPhaseComment = "Мин(рад) = " + fresnel_phase_min.ToString("0.0000") + "\nМакс(рад) = " + fresnel_phase_max.ToString("0.0000");
            FourierAmplitudeComment = "Мин(мм) = " + fourier_magnitude_min.ToString("0.0000") + "\nМакс(мм) = " + fourier_magnitude_max.ToString("0.0000");
            //HoloComment = "Мин(мм^2) = " + holo_min.ToString("0.0000") + "\nМакс(мм^2) = " + holo_max.ToString("0.0000");

            // Формируем карту фазы преобразования Френеля
            byte[] imageData = new byte[3 * height * width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Для удобства собираем в центре
                    int ii = i;
                    int jj = j;
                    if (combineInTheCenter)
                    {
                        if (j < width / 2) jj = width / 2 + j;
                        else jj = j - width / 2;
                        if (i < height / 2) ii = height / 2 + i;
                        else ii = i - height / 2;
                    }

                    double interp_intens = (fft_fresnel[jj, ii].Phase - fresnel_phase_min) / (fresnel_phase_max - fresnel_phase_min);
                    if (interp_intens < 0.0) interp_intens = 0.0;
                    if (interp_intens > 1.0) interp_intens = 1.0;
                    byte col = (byte)(interp_intens * 255.0);
                    int index = i * width + j;

                    imageData[index * 3] = col;
                    imageData[index * 3 + 1] = col;
                    imageData[index * 3 + 2] = col;
                }
            }

            // Формируем карту амплитуды преобразования Френеля
            byte[] imageData2 = new byte[3 * height * width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Для удобства собираем в центре
                    int ii = i;
                    int jj = j;
                    if (combineInTheCenter)
                    {
                        if (j < width / 2) jj = width / 2 + j;
                        else jj = j - width / 2;
                        if (i < height / 2) ii = height / 2 + i;
                        else ii = i - height / 2;
                    }

                    double interp_intens = (fft_fresnel[jj, ii].Magnitude - fresnel_magnitude_min) / (fresnel_magnitude_max - fresnel_magnitude_min);
                    if (interp_intens < 0.0) interp_intens = 0.0;
                    if (interp_intens > 1.0) interp_intens = 1.0;
                    byte col = (byte)(interp_intens * 255.0);
                    int index = i * width + j;

                    imageData2[index * 3] = col;
                    imageData2[index * 3 + 1] = col;
                    imageData2[index * 3 + 2] = col;
                }
            }

            // Формируем карту амплитуды преобразования Фурье
            byte[] imageData3 = new byte[3 * height * width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Для удобства собираем в центре
                    int ii = i;
                    int jj = j;
                    if (combineInTheCenter)
                    {
                        if (j < width / 2) jj = width / 2 + j;
                        else jj = j - width / 2;
                        if (i < height / 2) ii = height / 2 + i;
                        else ii = i - height / 2;
                    }

                    double interp_intens = (fft_fourier[jj, ii].Magnitude - fourier_magnitude_min) / (fourier_magnitude_max - fourier_magnitude_min);
                    if (interp_intens < 0.0) interp_intens = 0.0;
                    if (interp_intens > 1.0) interp_intens = 1.0;
                    byte col = (byte)(interp_intens * 255.0);
                    int index = i * width + j;

                    imageData3[index * 3] = col;
                    imageData3[index * 3 + 1] = col;
                    imageData3[index * 3 + 2] = col;
                }
            }

            // Формируем голограмму
            byte[] imageData4 = new byte[3 * height * width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double interp_intens = (values_hologram[j, i] - holo_min) / (holo_max - holo_min);
                    if (interp_intens < 0.0) interp_intens = 0.0;
                    if (interp_intens > 1.0) interp_intens = 1.0;
                    byte col = (byte)(interp_intens * 255.0);
                    int index = i * width + j;

                    imageData4[index * 3] = col;
                    imageData4[index * 3 + 1] = col;
                    imageData4[index * 3 + 2] = col;
                }
            }

            // Формируем восстановленное изображение
            byte[] imageData5 = new byte[3 * height * width];
            byte[] imageData6 = new byte[3 * height * width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int index = i * width + j;

                    double magn = reconstructed[j, i].Magnitude;
                    double interp_intens = (magn - restored_min) / (restored_max - restored_min);
                    if (interp_intens < 0.0) interp_intens = 0.0;
                    if (interp_intens > 1.0) interp_intens = 1.0;
                    byte col = (byte)(interp_intens * 255.0);
                    imageData5[index * 3] = col;
                    imageData5[index * 3 + 1] = col;
                    imageData5[index * 3 + 2] = col;

                    double ph = reconstructed[j, i].Phase;
                    interp_intens = (ph - restored_min_phase) / (restored_max_phase - restored_min_phase);
                    if (interp_intens < 0.0) interp_intens = 0.0;
                    if (interp_intens > 1.0) interp_intens = 1.0;
                    col = (byte)(interp_intens * 255.0);
                    imageData6[index * 3] = col;
                    imageData6[index * 3 + 1] = col;
                    imageData6[index * 3 + 2] = col;
                }
            }

            // Создание битмапов
            m_fresnelPhaseImage = BitmapSource.Create(width, height, 300, 300,
                                      PixelFormats.Rgb24, null, imageData, width * 3);
            m_fresnelAmplitudeImage = BitmapSource.Create(width, height, 300, 300,
                                      PixelFormats.Rgb24, null, imageData2, width * 3);
            m_fourierAmplitudeImage = BitmapSource.Create(width, height, 300, 300,
                                      PixelFormats.Rgb24, null, imageData3, width * 3);
            m_hologramImage = BitmapSource.Create(width, height, 300, 300,
                                      PixelFormats.Rgb24, null, imageData4, width * 3);
            m_restoredImage = BitmapSource.Create(width, height, 300, 300,
                                      PixelFormats.Rgb24, null, imageData5, width * 3);
            m_restoredImagePhase = BitmapSource.Create(width, height, 300, 300,
                                      PixelFormats.Rgb24, null, imageData6, width * 3);

            #endregion

            GC.Collect();

            if (OnFinishComputingHandler != null)
                OnFinishComputingHandler();
        }

        #region Event handlers

        void m_interferometryDataProvider_UpdateInterferometryDataHandler()
        {
            try
            {
                Process(m_lastImageMagnitude, m_lastImagePhase);
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show(exc.Message);
            }
        }

        #endregion
    }
}
