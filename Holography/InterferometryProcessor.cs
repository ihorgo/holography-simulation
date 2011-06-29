using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interferometry
{
    /// <summary>
    /// Класс рассчета интерферометрии
    /// </summary>
    public class InterferometryProcessor
    {
        private InterferometryDataProvider m_interferometryDataProvider = null;
        private double[] m_lines = null;
        private double[] m_grid = null;

        /// <summary>
        /// Волновое число (* 10^6 рад/м)
        /// </summary>
        public double WaveNumber
        {
            get
            {
                return 2.0 * Math.PI / m_interferometryDataProvider.Delta;
            }
        }

        /// <summary>
        /// Возвращает массив интенсивносстей линий (мм^2)
        /// </summary>
        public double[] LineIntensities
        {
            get { return m_lines; }
        }

        /// <summary>
        /// Возвращает сетку шагов (мм)
        /// </summary>
        public double[] StepGrid
        {
            get { return m_grid; }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public InterferometryProcessor(InterferometryDataProvider provider)
        {
            m_interferometryDataProvider = provider;
            m_interferometryDataProvider.UpdateInterferometryDataHandler += new OnUpdateInterferometryData(m_interferometryDataProvider_UpdateInterferometryDataHandler);
            Compute();
        }

        /// <summary>
        /// Деструктор
        /// </summary>
        ~InterferometryProcessor()
        {
            m_interferometryDataProvider.UpdateInterferometryDataHandler -= new OnUpdateInterferometryData(m_interferometryDataProvider_UpdateInterferometryDataHandler);
        }

        /// <summary>
        /// Делегат на завершение расчетов
        /// </summary>
        public delegate void OnFinishComputing();
        public event OnFinishComputing OnFinishComputingHandler;

        /// <summary>
        /// Расcчет интерференции двух плоских волн
        /// </summary>
        public void Recompute()
        {
            Compute();
        }

        private void Compute()
        {
            int linesCount = (int)(m_interferometryDataProvider.PlateWidth * m_interferometryDataProvider.PlateResolution);
            if (m_lines == null || m_lines.Length != linesCount)
            {
                m_grid = new double[linesCount];
                m_lines = new double[linesCount];
                GC.Collect(); // подчистим память
            }

            // Ширина одной линии пластинки (в мм)
            double lineWidth = 1.0 / m_interferometryDataProvider.PlateResolution;

            double ap2 = m_interferometryDataProvider.Ap * m_interferometryDataProvider.Ap;
            double ar2 = m_interferometryDataProvider.Ar * m_interferometryDataProvider.Ar;
            double kk = 2.0 * WaveNumber * 1e3;
            double diffPhases0 = (m_interferometryDataProvider.PhiP - m_interferometryDataProvider.PhiR) * Math.PI / 180.0;
            double sin_halfAlpha = Math.Sin(0.5 * m_interferometryDataProvider.Alpha * Math.PI / 180.0);

            int halfCount = linesCount / 2;
            for (int i = 0; i < linesCount; i++)
            {
                // текущий шаг (в мм)
                double x = lineWidth * (i - halfCount);
                m_grid[i] = x;
                // разность фаз в текущей точке
                double phasesDiff = kk * sin_halfAlpha * x + diffPhases0;
                // интенсивновность в текущей точке
                m_lines[i] = ap2 + ar2 + 2.0 * m_interferometryDataProvider.Ap * m_interferometryDataProvider.Ar * Math.Cos(phasesDiff);
            }

            if (OnFinishComputingHandler != null)
                OnFinishComputingHandler();
        }

        #region Event handlers

        void m_interferometryDataProvider_UpdateInterferometryDataHandler()
        {
            Compute();
        }

        #endregion
    }
}
