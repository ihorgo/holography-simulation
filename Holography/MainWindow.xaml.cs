using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace Interferometry
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, InterferometryDataProvider
    {
        private double m_lightSpeed = 299792458; // м/с
        private double m_delta = 0.64384691; // мкм
        private double m_plateWidth = 5.0; // мм
        private double m_plateResolustion = 100.0; // лин/мм
        private double m_Ap = 5.0; // мм
        private double m_PhiP = 0.0; // град
        private double m_Ar = 5.0; // мм
        private double m_PhiR = 0.0; // град
        private double m_alpha = 30.0; // град

        private double m_holoZ = 100.0; // мм
        private double m_holoZRecord = 100.0; // мм
        private double m_holoLyambda = 5e-4; // мм
        private double m_holoPixelSize = 0.01; // мм
        private double m_holoAlphaRecord = 0; // град
        private double m_holoAlphaReconstructed = 0; // град
        private bool m_holoSuppressDcTerm = true;
        private bool m_holoCombineInTheCenter = true;

        private InterferometryProcessor m_interferometryProcessor = null;
        private FourierProcessor m_fourierProcessor = null;

        CompositeDataSource m_compositeDataSource = null;

        #region Свойства для привязки данных (реализация InterferometryDataProvider)

        public double LightSpeed
        {
            set { m_lightSpeed = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_lightSpeed; }
        }

        public double PlateWidth
        {
            set { m_plateWidth = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_plateWidth; }
        }

        public double PlateResolution
        {
            set { m_plateResolustion = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_plateResolustion; }
        }

        public double Delta
        {
            set { m_delta = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_delta; }
        }

        public double Ar
        {
            set { m_Ar = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_Ar; }
        }

        public double PhiR
        {
            set { m_PhiR = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_PhiR; }
        }

        public double Ap
        {
            set { m_Ap = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_Ap; }
        }

        public double PhiP
        {
            set { m_PhiP = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_PhiP; }
        }

        public double Alpha
        {
            set { m_alpha = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_alpha; }
        }

        public double HoloZ
        {
            set { m_holoZ = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_holoZ; }
        }

        public double HoloZRecord
        {
            set { m_holoZRecord = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_holoZRecord; }
        }

        public double HoloLyambda
        {
            set { m_holoLyambda = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_holoLyambda; }
        }

        public double HoloPixelSize
        {
            set 
            { 
                m_holoPixelSize = value;
                if (imageObject.Source != null)
                {
                    double sz = m_holoPixelSize * ((BitmapSource)imageObject.Source).PixelWidth;
                    labelHoloSize.Text = "Размер объекта = " + sz.ToString("0.000") + " мм";
                }
                else
                {
                    labelHoloSize.Text = "Размер объекта = 0 мм";
                }
                if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); 
            }
            get { return m_holoPixelSize; }
        }

        public double HoloAlphaRecord
        {
            set { m_holoAlphaRecord = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_holoAlphaRecord; }
        }

        public double HoloAlphaReconstructed
        {
            set { m_holoAlphaReconstructed = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_holoAlphaReconstructed; }
        }

        public bool HoloSuppressDcTerm
        {
            set { m_holoSuppressDcTerm = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_holoSuppressDcTerm; }
        }

        public bool HoloCombineInTheCenter
        {
            set { m_holoCombineInTheCenter = value; if (UpdateInterferometryDataHandler != null) UpdateInterferometryDataHandler(); }
            get { return m_holoCombineInTheCenter; }
        }

        public event OnUpdateInterferometryData UpdateInterferometryDataHandler;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            m_interferometryProcessor = new InterferometryProcessor(this);
            m_interferometryProcessor.OnFinishComputingHandler += new InterferometryProcessor.OnFinishComputing(m_interferometryProcessor_OnFinishComputingHandler);

            m_fourierProcessor = new FourierProcessor(this);
            m_fourierProcessor.OnFinishComputingHandler += new FourierProcessor.OnFinishComputing(m_fourierProcessor_OnFinishComputingHandler);

            m_interferometryProcessor.Recompute();
        }

        #region Event handlers

        private CompositeDataSource CreateNewGraphSource(double[] xvalues, double[] yvalues)
        {
            var xDataSource = new EnumerableDataSource<double>(xvalues);
            xDataSource.SetXMapping(lx => lx);
            var yDataSource = new EnumerableDataSource<double>(yvalues);
            yDataSource.SetYMapping(y => y);
            return new CompositeDataSource(xDataSource, yDataSource);
        }

        void m_interferometryProcessor_OnFinishComputingHandler()
        {
            // строим график
            double[] steps = m_interferometryProcessor.StepGrid;
            double[] intensities = m_interferometryProcessor.LineIntensities;
            if (steps.Length != 0)
            {
                if (m_compositeDataSource == null)
                {
                    m_compositeDataSource = CreateNewGraphSource(steps, intensities);
                    intensityPlotter.AddLineGraph(m_compositeDataSource, new Pen(Brushes.Blue, 1), new PenDescription("Интенсивность"));
                    intensityPlotter.FitToView();
                }
                else
                {
                    EnumerableDataSource<double> xds = (EnumerableDataSource<double>)m_compositeDataSource.DataParts.ElementAt(0);
                    EnumerableDataSource<double> yds = (EnumerableDataSource<double>)m_compositeDataSource.DataParts.ElementAt(1);
                    double[] xpoints = (double[])xds.Data;
                    double[] ypoints = (double[])yds.Data;
                    intensityPlotter.RemoveUserElements();
                    if (xpoints.Length != steps.Length || ypoints.Length != intensities.Length)
                    {   
                        m_compositeDataSource = CreateNewGraphSource(steps, intensities);
                    }
                    else
                    {
                        for(int i = 0; i < steps.Length; i++)
                        {
                            xpoints[i] = steps[i];
                            ypoints[i] = intensities[i];
                        }
                    }
                    intensityPlotter.AddLineGraph(m_compositeDataSource, new Pen(Brushes.Blue, 1), new PenDescription("Интенсивность"));
                    GC.Collect();
                }

                // пересоздаем битмап
                byte[] imageData = new byte[3 * intensities.Length * intensities.Length];
                double maxIntens = (m_Ap + m_Ar) * (m_Ap + m_Ar);
                for (int i = 0; i < intensities.Length; i++)
                {
                    for (int j = 0; j < intensities.Length; j++)
                    {
                        double intens = intensities[j];
                        double interp_intens = intens / maxIntens;
                        if (interp_intens < 0.0) interp_intens = 0.0;
                        if (interp_intens > 1.0) interp_intens = 1.0;
                        byte col = (byte)(interp_intens * 255.0);
                        int index = i * intensities.Length + j;
                        
                        imageData[index * 3] = col;
                        imageData[index * 3 + 1] = col;
                        imageData[index * 3 + 2] = col;
                    }
                }

                BitmapSource bs = BitmapSource.Create(intensities.Length, intensities.Length, 300, 300, 
                                                      PixelFormats.Rgb24, null, imageData, 3 * intensities.Length);

                imageInterferension.Source = bs;
            }
            else
            {
                intensityPlotter.RemoveUserElements();
                imageInterferension.Source = null;
            }
        }

        void m_fourierProcessor_OnFinishComputingHandler()
        {
            // Обновляем картинки
            imageFresnelPhase.Source = m_fourierProcessor.FresnelPhaseImage;
            imageFresnelAmplitude.Source = m_fourierProcessor.FresnelAmplitudeImage;
            imageFourierAmplitude.Source = m_fourierProcessor.FourierAmplitudeImage;
            imageHologram.Source = m_fourierProcessor.HologramImage;
            imageRestoring.Source = m_fourierProcessor.RestoredImage;
            imageRestoringPhase.Source = m_fourierProcessor.RestoredImagePhase;

            // Обновляем подписи
            textblockFresnelAmplitude.Text = m_fourierProcessor.FresnelAmplitudeComment;
            textblockFresnelPhase.Text = m_fourierProcessor.FresnelPhaseComment;
            textblockFourierAmplitude.Text = m_fourierProcessor.FourierAmplitudeComment;
            textblockHolo.Text = m_fourierProcessor.HoloComment;
        }

        private void alphaTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) this.alphaSlider.Focus();
        }

        private void textBoxHolo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) this.buttonPerform.Focus();
        }

        private void checkBoxHolo_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void buttonOpenImage_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Bitmap files (*.bmp)|*.bmp";//|All files (*.*)|*.*";
            dialog.Title = "Select a bitmap file";
            System.Windows.Forms.DialogResult res = dialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                BitmapImage img = new BitmapImage(new Uri(dialog.FileName));
                if (img.Palette != null)
                {
                    System.Windows.MessageBox.Show("Выбранная битовая карта имеет палитру. Пересохраните изображение без использования палитры");
                    return;
                }

                imageObject.Source = img;
                if (imageObject.Source != null)
                {
                    double sz = m_holoPixelSize * ((BitmapSource)imageObject.Source).PixelWidth;
                    labelHoloSize.Text = "Размер объекта = " + sz.ToString("0.000") + " мм";
                }
            }
        }

        private void buttonOpenImagePhase_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Bitmap files (*.bmp)|*.bmp";//|All files (*.*)|*.*";
            dialog.Title = "Select a bitmap file";
            System.Windows.Forms.DialogResult res = dialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                BitmapImage img = new BitmapImage(new Uri(dialog.FileName));
                if (img.Palette != null)
                {
                    System.Windows.MessageBox.Show("Выбранная битовая карта имеет палитру. Пересохраните изображение без использования палитры");
                    return;
                }

                imagePhase.Source = img;
            }
        }

        private void buttonPerform_Click(object sender, RoutedEventArgs e)
        {
            if (imageObject.Source == null)
            {
                System.Windows.MessageBox.Show("Исходное изображение не выбрано");
                return;
            }

            try
            {
                m_fourierProcessor.Process((BitmapImage)imageObject.Source, (BitmapImage)imagePhase.Source);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void imageObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview preview = new ImagePreview(imageObject.Source);
            preview.Owner = this;
            preview.ShowDialog();
        }

        private void imagePhase_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview preview = new ImagePreview(imagePhase.Source);
            preview.Owner = this;
            preview.ShowDialog();
        }

        private void imageFresnelAmplitude_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview preview = new ImagePreview(imageFresnelAmplitude.Source);
            preview.Owner = this;
            preview.ShowDialog();
        }

        private void imageFourierAmplitude_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview preview = new ImagePreview(imageFourierAmplitude.Source);
            preview.Owner = this;
            preview.ShowDialog();
        }

        private void imageFresnelPhase_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview preview = new ImagePreview(imageFresnelPhase.Source);
            preview.Owner = this;
            preview.ShowDialog();
        }

        private void imageHologram_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview preview = new ImagePreview(imageHologram.Source);
            preview.Owner = this;
            preview.ShowDialog();
        }

        private void imageRestoring_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview preview = new ImagePreview(imageRestoring.Source);
            preview.Owner = this;
            preview.ShowDialog();
        }

        private void imageRestoringPhase_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImagePreview preview = new ImagePreview(imageRestoringPhase.Source);
            preview.Owner = this;
            preview.ShowDialog();
        }

        #endregion
    }
}
