using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using VinylStudio.model;

namespace VinylStudio.ui
{
    /// <summary>
    /// Interaktionslogik für ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        private CollectionView _dataModel;

        private bool _exportGenre = false;
        private bool _exportType = false;
        private bool _exportLength = false;
        private bool _exportRating = false;
        private bool _exportReleased = false;
        private bool _exportPurchased = false;
        private bool _exportPrice = false;
        private bool _exportLocation = false;
        private bool _exportTracklist = false;
        private bool _groupTracklist = false;
        private string _colorBackground = "#ffffff";
        private string _colorForeground = "#000000";
        private string _colorHeaderBackground = "#ff972f";
        private string _colorHeaderForeground = "#ffffff";

        /**
         * Constructor of this class
         */
        public ExportDialog(CollectionView model)
        {
            InitializeComponent();

            _dataModel = model;
        }

        /**
         * Is called if the user clicked Cancel
         */
        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close();
        }

        /**
         * Lets the user select a color and returns it as HTML color code
         */
        private string SelectColor(string initialHTMLColor)
        {
            ColorSelectionDialog dialog = new(initialHTMLColor)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            string color = dialog.OpenDialog();

            return color;
        }

        /**
         * Is called if the user wants to change the background color
         */
        private void OnSelectBackground(object sender, RoutedEventArgs e)
        {
            ExtractColors();
            string htmlColor = SelectColor(_colorBackground);

            System.Windows.Media.Color color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(htmlColor);
            SolidColorBrush brush = new SolidColorBrush(color);
            labelBackground.Background = brush;
        }

        /**
         * Is called if the user wants to change the foreground color
         */
        private void OnSelectForeground(object sender, RoutedEventArgs e)
        {
            ExtractColors();
            string htmlColor = SelectColor(_colorBackground);

            System.Windows.Media.Color color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(htmlColor);
            SolidColorBrush brush = new SolidColorBrush(color);
            labelForeground.Background = brush;
        }

        /**
         * Is called if the user wants to change the header background color
         */
        private void OnSelectHeaderBackground(object sender, RoutedEventArgs e)
        {
            ExtractColors();
            string htmlColor = SelectColor(_colorBackground);

            System.Windows.Media.Color color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(htmlColor);
            SolidColorBrush brush = new SolidColorBrush(color);
            labelHeaderBackground.Background = brush;
        }

        /**
         * Is called if the user wants to change the header foreground color
         */
        private void OnSelectHeaderForeground(object sender, RoutedEventArgs e)
        {
            ExtractColors();
            string htmlColor = SelectColor(_colorBackground);

            System.Windows.Media.Color color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(htmlColor);
            SolidColorBrush brush = new SolidColorBrush(color);
            labelHeaderForeground.Background = brush;
        }

        /**
         * Is called if the user clicked Export
         */
        private void OnExportClicked(object sender, EventArgs e)
        {
            ExtractOptions();

            // TODO: ask for filename
            Export("ExcelExport.xlsx");

            Close();
        }

        /** 
         * Gets the colors from the UI controls ans stores it as HTML color code in the members
         * of this class
         */
        private void ExtractColors()
        {
            SolidColorBrush? brush = labelBackground.Background as SolidColorBrush;
            if (brush != null)
            {
                _colorBackground = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
            }

            brush = labelForeground.Background as SolidColorBrush;
            if (brush != null)
            {
                _colorForeground = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
            }

            brush = labelHeaderBackground.Background as SolidColorBrush;
            if (brush != null)
            {
                _colorHeaderBackground = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
            }

            brush = labelHeaderForeground.Background as SolidColorBrush;
            if (brush != null)
            {
                _colorHeaderForeground = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
            }
        }

        /**
         * Extracts all options from the UI controls into the members of this class
         */
        private void ExtractOptions()
        {
            _exportGenre = checkGenre.IsChecked == null ? false : (bool)checkGenre.IsChecked;
            _exportType = checkType.IsChecked == null ? false : (bool)checkType.IsChecked;
            _exportLength = checkLength.IsChecked == null ? false : (bool)checkLength.IsChecked;
            _exportRating = checkRating.IsChecked == null ? false : (bool)checkRating.IsChecked;
            _exportReleased = checkReleased.IsChecked == null ? false : (bool)checkReleased.IsChecked;
            _exportPurchased = checkPurchased.IsChecked == null ? false : (bool)checkPurchased.IsChecked;
            _exportPrice = checkPrice.IsChecked == null ? false : (bool)checkPrice.IsChecked;
            _exportLocation = checkLocation.IsChecked == null ? false : (bool)checkLocation.IsChecked;
            _exportTracklist = checkTracklist.IsChecked == null ? false : (bool)checkTracklist.IsChecked; 
            _groupTracklist = checkGrouping.IsChecked == null ? false : (bool)checkGrouping.IsChecked;

            ExtractColors();
        }

        /**
         * Exports all data to the excel file
         */
        private void Export(string filename)
        {
            int row = 2;
            
            using (XLWorkbook workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet = workbook.Worksheets.Add("VinylStudio");
                
                // write header
                ExportHeader(worksheet);

                // Export albums
                foreach (AlbumModel album in _dataModel)
                {
                    row = ExportAlbum(worksheet, row, album);
                }

                worksheet.Columns().AdjustToContents();
                worksheet.SheetView.FreezeRows(1);

                // save the file to disc
                workbook.SaveAs(filename);
            }
        }

        /**
         * Exports the header of the excel table
         */
        private void ExportHeader(IXLWorksheet worksheet)
        {
            int col = 1;

            worksheet.Cell(1, col++).Value = "Interpret";
            worksheet.Cell(1, col++).Value = "Album";

            if (_exportGenre)
            {
                worksheet.Cell(1, col++).Value = "Genre";
            }

            if (_exportType)
            {
                worksheet.Cell(1, col++).Value = "Type";
            }

            if (_exportLength)
            {
                worksheet.Cell(1, col++).Value = "Length";
            }

            if (_exportRating)
            {
                worksheet.Cell(1, col++).Value = "Rating";
            }

            if (_exportReleased)
            {
                worksheet.Cell(1, col++).Value = "Release Year";
            }

            if (_exportPurchased)
            {
                worksheet.Cell(1, col++).Value = "Purchased";
            }

            if (_exportPrice)
            {
                worksheet.Cell(1, col++).Value = "Price";
            }

            if (_exportLocation)
            {
                worksheet.Cell(1, col++).Value = "Location";
            }

            // Format header
            char endColumn = (char)('A' + col - 2);
            var range = worksheet.Cells($"A1:{endColumn}1");
            var style = range.Style;
            style.Font.Bold = true;
            style.Fill.BackgroundColor = XLColor.FromHtml(_colorHeaderBackground);
            style.Font.FontColor = XLColor.FromHtml(_colorHeaderForeground);            
        }

        /**
         * Exports an album to the worksheet and returns the current row position after the export
         */
        private int ExportAlbum(IXLWorksheet worksheet, int row, AlbumModel album)
        {
            int col = 1;

            worksheet.Cell(row, col++).Value = album.Interpret?.Name;
            worksheet.Cell(row, col++).Value = album.Name;

            if (_exportGenre)
            {
                worksheet.Cell(row, col++).Value = (album.Genre == null ? "" : album.Genre.Name);
            }

            if (_exportType)
            {
                worksheet.Cell(row, col++).Value = AlbumTypeTranslator.GetEnumDescription(album.AlbumType);
            }

            if (_exportLength)
            {
                worksheet.Cell(row, col++).Value = album.CummulatedLengthString;
            }

            if (_exportRating)
            {
                worksheet.Cell(row, col++).Value = AlbumRatingTranslator.GetEnumDescription(album.AlbumRating);
            }

            if (_exportReleased)
            {
                worksheet.Cell(row, col++).Value = album.ReleaseYear;
            }

            if (_exportPurchased)
            {
                worksheet.Cell(row, col++).Value = album.PurchaseYear;
            }

            if (_exportPrice)
            {
                worksheet.Cell(row, col++).Value = album.PriceString;
            }

            if (_exportLocation)
            {
                worksheet.Cell(row, col++).Value = album.Location;
            }

            char endColumn = (char)('A' + col - 2);
            var range = worksheet.Cells($"A{row}:{endColumn}{row}");
            var style = range.Style;
            style.Fill.BackgroundColor = XLColor.FromHtml(_colorBackground);
            style.Font.FontColor = XLColor.FromHtml(_colorForeground);
            style.Font.Bold = true;

            if (_exportTracklist)
            {
                row++;
                row = ExportTracklist(worksheet, row, album, endColumn);
            }

            return ++row;
        }

        /**
         * Exports the tracklist of an album
         */
        private int ExportTracklist(IXLWorksheet worksheet, int row, AlbumModel album, char endColumn)
        {
            int startRow = row;

            // header
            worksheet.Cell(row, 2).Value = "Song";
            worksheet.Cell(row, 3).Value = "Side";
            worksheet.Cell(row, 4).Value = "Track";
            worksheet.Cell(row, 5).Value = "Length";

            // format header
            var range = worksheet.Cells($"A{row}:{endColumn}{row}");
            var style = range.Style;
            style.Font.Italic = true;
            style.Fill.BackgroundColor = XLColor.FromHtml(_colorHeaderBackground);
            style.Font.FontColor = XLColor.FromHtml(_colorHeaderForeground);
            
            foreach (SongModel song in album.Songs)
            {
                row++;
                worksheet.Cell(row, 2).Value = song.Name;
                worksheet.Cell(row, 3).Value = song.Side.ToString();
                worksheet.Cell(row, 4).Value = song.Index;
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, 5).Value = song.LengthInMinutes;

                range = worksheet.Cells($"A{row}:{endColumn}{row}");
                style = range.Style;
                style.Font.Bold = false;
                style.Font.Italic = true;
                style.Fill.BackgroundColor = XLColor.FromHtml(_colorBackground);
                style.Font.FontColor = XLColor.FromHtml(_colorForeground);
            }

            if (_groupTracklist)
            {
                worksheet.Rows(startRow, row).Group();
            }
            
            return row;
        }
    }    
}
