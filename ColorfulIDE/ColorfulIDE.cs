using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Text.Editor;
using HiTeam.ColorfulIDE.Settings;
using System.Timers;
using System.Collections.Generic;


namespace HiTeam.ColorfulIDE
{
    /// <summary>
    /// Adornment class that draws a square box in the top right hand corner of the viewport
    /// </summary>
    /// 

    public class ColorfulIde : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_mDisposed) return;
            if (disposing)
            {
                _timer.Dispose();
                _opacityTimer.Dispose();
                // Release managed resources
            }

            // Release unmanaged resources

            _mDisposed = true;
        }

        ~ColorfulIde()
        {
            Dispose(false);
        }

        private bool _mDisposed;

        private readonly Setting _config;
        private Image _image;
        private BitmapImage _bitmap;
        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _adornmentLayer;
        private readonly Timer _timer;
        private readonly Timer _opacityTimer;
        private int _opacityIndex;
        private int _flag;
        private double _ratio;

        private readonly IServiceProvider _serviceProvider;

        private int _current;
        private string[] _imagePaths;
        private int[] _sequence;

        private void RandomSequence()
        {
            var list = new List<int>();
            var random = new Random();
            for (int i = 0; i < _imagePaths.Length; i++)
            {
                int id;
                do
                {
                    id = random.Next(_imagePaths.Length);
                } while (list.Contains(id));
                list.Add(id);
            }
            _sequence = list.ToArray();
        }

        /// <summary>
        /// Creates a square image and attaches an event handler to the layout changed event that
        /// adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        /// <param name="sp"></param>
        public ColorfulIde(IWpfTextView view, IServiceProvider sp)
        {
            try
            {
                _serviceProvider = sp;
                _view = view;
                _config = GetConfigFromVisualStudioSettings();

                if (_config.IsDirectory)
                {
                    var imagelist = new List<string>();
                    AddImages("*.png", imagelist);
                    AddImages("*.jpg", imagelist);
                    AddImages("*.gif", imagelist);
                    AddImages("*.bmp", imagelist);
                    _imagePaths = imagelist.ToArray();
                }
                else
                {
                    var path = _config.BackgroundImageFileAbsolutePath;
                    if (File.Exists(path))
                        _imagePaths = new[] { path };
                }

                if (_imagePaths.Length == 0)
                    return;

                RandomSequence();

                _current = 0;

                _adornmentLayer = view.GetAdornmentLayer("Colorful-IDE");

                ChangeImage();

                _timer = new Timer(_config.Interval);
                _timer.Elapsed += delegate
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(RefreshImage));
                };
                //_Timer.Start();
                _timer.Enabled = _config.IsDirectory;

                _opacityIndex = 10;
                _flag = 0;
                _opacityTimer = new Timer(_config.OpacityInterval);
                _opacityTimer.Elapsed += delegate
                {
                    try
                    {
                        if (_opacityIndex < 0 || _opacityIndex > 10)
                        {
                            return;
                        }
                        if (_flag == 0)
                            _opacityIndex -= 2;
                        else
                            _opacityIndex += 2;

                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _adornmentLayer.Opacity = _opacityIndex / 10.0;
                        }));

                        if (Math.Abs(_opacityIndex) < 1e-6)
                        {
                            _opacityTimer.Stop();
                            _flag = 1;
                            Application.Current.Dispatcher.BeginInvoke(new Action(() => { ChangeImage(); OnSizeChange(); }));
                            _opacityIndex = 0;
                            _opacityTimer.Start();
                        }
                        else if (_opacityIndex == 10)
                        {
                            _opacityTimer.Stop();
                            _flag = 0;
                            Application.Current.Dispatcher.BeginInvoke(new Action(OnSizeChange));
                            _opacityIndex = 10;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                };

                _view.ViewportHeightChanged += delegate { OnSizeChange(); };
                _view.ViewportWidthChanged += delegate { OnSizeChange(); };
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void AddImages(string format, List<string> imagelist)
        {
            _imagePaths = Directory.GetFiles(_config.BackgroundImageAbsolutePath, format, SearchOption.AllDirectories);
            imagelist.AddRange(_imagePaths);
        }


        private void RefreshImage()
        {
            _opacityTimer.Start();
        }

        private void ChangeImage()
        {
            _image = new Image { Opacity = _config.Opacity };

            _bitmap = new BitmapImage();
            _bitmap.BeginInit();

            _bitmap.UriSource = new Uri(_imagePaths[_sequence[_current]], UriKind.Absolute);
            _bitmap.EndInit();
            _image.Source = _bitmap;

            var cut = new Int32Rect(0, 0, 1, 1);

            //计算Stride

            var stride = _bitmap.Format.BitsPerPixel * cut.Width / 8;

            //声明字节数组

            var data = new byte[cut.Height * stride];
            _bitmap.CopyPixels(cut, data, stride, 0);

            if (_config.ChangeBackgroundColor)
            {
                if (data.Length == 3)
                {
                    _view.Background = new SolidColorBrush(Color.FromRgb(data[2], data[1], data[0]));
                }
                else if (data[3] != 0)
                    _view.Background = new SolidColorBrush(Color.FromArgb(data[3], data[2], data[1], data[0]));
            }


            _image.Width = _bitmap.PixelWidth;
            _image.Height = _bitmap.PixelHeight;

            _image.IsHitTestVisible = false;

            _current = (_current + 1) % _imagePaths.Length;
        }

        private Setting GetConfigFromVisualStudioSettings()
        {
            try
            {
                var config = new Setting();

                var dte2 = (DTE2)_serviceProvider.GetService(typeof(DTE));

                var props = dte2.Properties["Colorful-IDE", "General"];

                config.IsDirectory = props.Item("IsDirectory").Value;
                config.BackgroundImageFileAbsolutePath = props.Item("BackgroundImageFileAbsolutePath").Value;
                config.BackgroundImageAbsolutePath = props.Item("BackgroundImageAbsolutePath").Value;
                config.Opacity = props.Item("Opacity").Value;
                config.PositionHorizon = (PositionH)props.Item("PositionHorizon").Value;
                config.PositionVertical = (PositionV)props.Item("PositionVertical").Value;
                config.Interval = props.Item("Interval").Value;
                config.OpacityInterval = props.Item("OpacityInterval").Value;
                config.ChangeBackgroundColor = props.Item("ChangeBackgroundColor").Value;
                config.AutoResize = props.Item("AutoResize").Value;
                config.RandomSequence = props.Item("RandomSequence").Value;
                return config;
            }
            catch (Exception)
            {
                return Setting.Deserialize();
            }
        }

        public void OnSizeChange()
        {
            try
            {
                _adornmentLayer.RemoveAllAdornments();

                ResizeImage();

                switch (_config.PositionHorizon)
                {
                    case PositionH.Left:
                        Canvas.SetLeft(_image, _view.ViewportLeft);
                        break;
                    case PositionH.Right:
                        Canvas.SetLeft(_image, _view.ViewportRight - _bitmap.PixelWidth * _ratio);
                        break;
                    case PositionH.Center:
                        Canvas.SetLeft(_image, _view.ViewportRight - _view.ViewportWidth + (_view.ViewportWidth / 2 - _bitmap.PixelWidth * _ratio / 2));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                switch (_config.PositionVertical)
                {
                    case PositionV.Top:
                        Canvas.SetTop(_image, _view.ViewportTop);
                        break;
                    case PositionV.Bottom:
                        Canvas.SetTop(_image, _view.ViewportBottom - _bitmap.PixelHeight * _ratio);
                        break;
                    case PositionV.Center:
                        Canvas.SetTop(_image, _view.ViewportBottom - _view.ViewportHeight + (_view.ViewportHeight / 2 - _bitmap.PixelHeight * _ratio / 2.0));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _image, null);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ResizeImage()
        {
            _ratio = 1.0;

            if (!_config.AutoResize)
            {
                return;
            }

            if (_bitmap.PixelWidth > _view.ViewportWidth)
            {
                _ratio = _view.ViewportWidth / _bitmap.PixelWidth;
            }

            if (_bitmap.PixelHeight > _view.ViewportHeight)
            {
                _ratio = _view.ViewportHeight / _bitmap.PixelHeight < _ratio ? _view.ViewportHeight / _bitmap.PixelHeight : _ratio;
            }

            _image.Height = _bitmap.PixelHeight * _ratio;
            _image.Width = _bitmap.PixelWidth * _ratio;
        }
    }
}
