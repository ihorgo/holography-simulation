using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interferometry
{
    /// <summary>
    /// Делегат для обновления данных
    /// </summary>
    public delegate void OnUpdateInterferometryData();

    /// <summary>
    /// Интерфейс предоставления данных для расчета интерферометрии
    /// </summary>
    public interface InterferometryDataProvider
    {
        /// <summary>
        /// Скорость света (в м/с)
        /// </summary>
        double LightSpeed {set; get;}

        /// <summary>
        /// Ширина пластинки (в мм)
        /// </summary>
        double PlateWidth { set; get; }

        /// <summary>
        /// Разрешение пластинки (в лин/мм)
        /// </summary>
        double PlateResolution { set; get; }

        /// <summary>
        /// Длина волны (в мкм)
        /// </summary>
        double Delta { set; get; }

        /// <summary>
        /// Амплитуда объектной волны (в мм)
        /// </summary>
        double Ar { set; get; }

        /// <summary>
        /// Начальная фаза объектной волны (в град)
        /// </summary>
        double PhiR { set; get; }

        /// <summary>
        /// Амплитуда опорной волны (в мм)
        /// </summary>
        double Ap { set; get; }

        /// <summary>
        /// Начальная фаза опорной волны (в град)
        /// </summary>
        double PhiP { set; get; }

        /// <summary>
        /// Угол между опорной и объектной волной (в град)
        /// </summary>
        double Alpha { set; get; }

        /// <summary>
        /// Расстояние наблюдения голограммы (мм)
        /// </summary>
        double HoloZ { set; get; }

        /// <summary>
        /// Расстояние записи голограммы (мм)
        /// </summary>
        double HoloZRecord { set; get; }

        /// <summary>
        /// Длина волны для голографии (мм)
        /// </summary>
        double HoloLyambda { set; get; }

        /// <summary>
        /// Размер пикселя в голограмме (мм)
        /// </summary>
        double HoloPixelSize { set; get; }

        /// <summary>
        /// Угол падения опорной волны при записи (град)
        /// </summary>
        double HoloAlphaRecord { set; get; }

        /// <summary>
        /// Угол падения опорной волны при восстановлении (град)
        /// </summary>
        double HoloAlphaReconstructed { set; get; }

        /// <summary>
        /// Подавлять постоянную составляющую?
        /// </summary>
        bool HoloSuppressDcTerm { set; get; }

        /// <summary>
        /// Собирать преобразование Фурье и Френеля в центре
        /// </summary>
        bool HoloCombineInTheCenter { set; get; }

        /// <summary>
        /// Событие обновления данных
        /// </summary>
        event OnUpdateInterferometryData UpdateInterferometryDataHandler;
    }
}
