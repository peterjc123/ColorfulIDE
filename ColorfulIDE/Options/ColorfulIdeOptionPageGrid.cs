using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using HiTeam.ColorfulIDE.Settings;
using Microsoft.VisualStudio.Shell;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.IO;
using HiTeam.ColorfulIDE.Properties;

namespace HiTeam.ColorfulIDE.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [CLSCompliant(false), ComVisible(true)]
    public class ColorfulIdeOptionPageGrid : DialogPage
    {
        public ColorfulIdeOptionPageGrid()
        {
            IsDirectory = true;
            BackgroundImageFileAbsolutePath = Environment.CurrentDirectory + @"\vsixicon.png";
            BackgroundImageAbsolutePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            Opacity = 0.35;
            PositionHorizon = PositionH.Right;
            PositionVertical = PositionV.Bottom;
            Interval = 60000;
            OpacityInterval = 200;
            ChangeBackgroundColor = false;
        }

        [Category("Image")]
        [LocalizedDisplayName("S208")]
        [LocalizedDescription("S210")]
        public bool IsDirectory { get; set; }

        [Category("Image")]
        [LocalizedDisplayName("S212")]
        [LocalizedDescription("S214")]
        [EditorAttribute(typeof(BrowseSingleFile), typeof(UITypeEditor))]
        public string BackgroundImageFileAbsolutePath { get; set; }

        [Category("Image")]
        [LocalizedDisplayName("S216")]
        [LocalizedDescription("S218")]
        [EditorAttribute(typeof(BrowseFile), typeof(UITypeEditor))]
        public string BackgroundImageAbsolutePath { get; set; }

        [Category("Image")]
        [LocalizedDisplayName("S220")]
        [LocalizedDescription("S222")]
        public double Opacity { get; set; }

        [Category("Layout")]
        [LocalizedDisplayName("S224")]
        [LocalizedDescription("S226")]
        [PropertyPageTypeConverter(typeof(PositionHTypeConverter))]
        [TypeConverter(typeof(PositionHTypeConverter))]
        public PositionH PositionHorizon { get; set; }

        [Category("Layout")]
        [LocalizedDisplayName("S228")]
        [LocalizedDescription("S230")]
        [PropertyPageTypeConverter(typeof(PositionVTypeConverter))]
        [TypeConverter(typeof(PositionVTypeConverter))]
        public PositionV PositionVertical { get; set; }

        [Category("Layout")]
        [LocalizedDisplayName("S232")]
        [LocalizedDescription("S234")]
        public double Interval { get; set; }

        [Category("Layout")]
        [LocalizedDisplayName("S236")]
        [LocalizedDescription("S238")]
        public double OpacityInterval { get; set; }

        [Category("Layout")]
        [LocalizedDisplayName("S240")]
        [LocalizedDescription("S242")]
        public bool ChangeBackgroundColor { get; set; }

        [Category("Layout")]
        [LocalizedDisplayName("S244")]
        [LocalizedDescription("S246")]
        public bool AutoResize { get; set; }

        [Category("Layout")]
        [LocalizedDisplayName("S248")]
        [LocalizedDescription("S250")]
        public bool RandomSequence { get; set; }
    }

    public class PositionHTypeConverter : EnumConverter
    {
        public PositionHTypeConverter()
            : base(typeof(PositionH))
        {

        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;

            switch (str)
            {
                case "Right":
                    return PositionH.Right;
                case "Left":
                    return PositionH.Left;
                default:
                    return base.ConvertFrom(context, culture, value);
            }

        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string)) return base.ConvertTo(context, culture, value, destinationType);
            string result = null;
            if (value != null && (int)value == 0)
            {
                result = "Left";
            }
            else if (value != null && (int)value == 1)
            {
                result = "Right";
            }

            return result ?? base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class PositionVTypeConverter : EnumConverter
    {
        public PositionVTypeConverter()
            : base(typeof(PositionV))
        {

        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var str = value as string;

            switch (str)
            {
                case null:
                    return base.ConvertFrom(context, culture, value);
                case "Top":
                    return PositionV.Top;
                case "Bottom":
                    return PositionV.Bottom;
                default:
                    return base.ConvertFrom(context, culture, value);
            }

        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string)) return base.ConvertTo(context, culture, value, destinationType);
            if (value != null)
            {
                string result;
                switch ((int)value)
                {
                    case 0:
                        result = "Top";
                        break;
                    case 1:
                        result = "Bottom";
                        break;
                    default:
                        result = null;
                        break;
                }

                return result ?? base.ConvertTo(context, culture, value, destinationType);
            }
            else
            {
                return null;
            }
        }
    }

    internal class BrowseFile : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                var edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null) return value;
            }
            var open = new FolderBrowserDialog { SelectedPath = Path.GetDirectoryName((string)value) };

            //open.FileName = Path.GetFileName((string)value);
            return open.ShowDialog() == DialogResult.OK ? open.SelectedPath : value;
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }

    internal class BrowseSingleFile : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider == null) return value;
            var edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc == null) return value;
            var open = new OpenFileDialog
            {
                Filter = Resources.BrowseSingleFile_EditValue_Image_Files___BMP___JPG___GIF___PNG____BMP___JPG___GIF___PNG,
                FileName = Path.GetDirectoryName((string)value)
            };

            //open.FileName = Path.GetFileName((string)value);
            return open.ShowDialog() == DialogResult.OK ? open.FileName : value;
        }
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }

}
