using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace Petrov_122.Pages
{
    public partial class DiagrammPage : Page
    {
        private readonly Entities _context = new Entities();

        public DiagrammPage()
        {
            InitializeComponent();

            // Инициализация диаграммы
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            var currentSeries = new Series("Платежи")
            {
                IsValueShownAsLabel = true
            };
            ChartPayments.Series.Add(currentSeries);

            // Загрузка данных в ComboBox
            LoadComboBoxData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                CmbUser.ItemsSource = _context.User.ToList();
                CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));

                // Установка значений по умолчанию
                if (CmbUser.Items.Count > 0)
                    CmbUser.SelectedIndex = 0;
                if (CmbDiagram.Items.Count > 0)
                    CmbDiagram.SelectedItem = SeriesChartType.Column;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                if (currentSeries == null) return;

                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var categoriesList = _context.Category.ToList();
                foreach (var category in categoriesList)
                {
                    var sum = _context.Payment.ToList()
                        .Where(u => u.User == currentUser && u.Category == category)
                        .Sum(u => u.Price * u.Num);

                    currentSeries.Points.AddXY(category.Name, sum);
                }
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();

                var application = new Excel.Application();
                application.SheetsInNewWorkbook = allUsers.Count();
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                decimal grandTotal = 0;

                for (int i = 0; i < allUsers.Count(); i++)
                {
                    int startRowIndex = 1;
                    Excel.Worksheet worksheet = application.Worksheets.Item[i + 1];
                    worksheet.Name = allUsers[i].FIO;

                    worksheet.Cells[1, startRowIndex] = "Дата платежа";
                    worksheet.Cells[2, startRowIndex] = "Название";
                    worksheet.Cells[3, startRowIndex] = "Стоимость";
                    worksheet.Cells[4, startRowIndex] = "Количество";
                    worksheet.Cells[5, startRowIndex] = "Сумма";

                    Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[5, 1]];
                    columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columnHeaderRange.Font.Bold = true;
                    startRowIndex++;

                    var userCategories = allUsers[i].Payment.OrderBy(u => u.Date).GroupBy(u => u.Category).OrderBy(u => u.Key.Name);

                    foreach (var groupCategory in userCategories)
                    {
                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[1, startRowIndex], worksheet.Cells[5, startRowIndex]];
                        headerRange.Merge();
                        headerRange.Value = groupCategory.Key.Name;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;

                        foreach (var payment in groupCategory)
                        {
                            worksheet.Cells[1, startRowIndex] = payment.Date?.ToString("dd.MM.yyyy") ?? "не указана";
                            worksheet.Cells[2, startRowIndex] = payment.Name;
                            worksheet.Cells[3, startRowIndex] = (double)payment.Price;
                            (worksheet.Cells[3, startRowIndex] as Excel.Range).NumberFormat = "0.00";
                            worksheet.Cells[4, startRowIndex] = payment.Num;
                            worksheet.Cells[5, startRowIndex].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                            (worksheet.Cells[5, startRowIndex] as Excel.Range).NumberFormat = "0.00";
                            startRowIndex++;
                        }

                        Excel.Range sumRange = worksheet.Range[worksheet.Cells[1, startRowIndex], worksheet.Cells[4, startRowIndex]];
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО:";
                        sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                        worksheet.Cells[5, startRowIndex].Formula = $"=SUM(E{startRowIndex - groupCategory.Count()}:E{startRowIndex - 1})";
                        sumRange.Font.Bold = true;
                        (worksheet.Cells[5, startRowIndex] as Excel.Range).Font.Bold = true;

                        grandTotal += groupCategory.Sum(p => p.Price * p.Num) ?? 0;
                        startRowIndex++;
                    }

                    Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[5, startRowIndex - 1]];
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle =
                    Excel.XlLineStyle.xlContinuous;

                    worksheet.Columns.AutoFit();
                }

                // Добавляем лист с общим итогом
                Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                summarySheet.Name = "Общий итог";

                summarySheet.Cells[1, 1] = "Общий итог:";
                summarySheet.Cells[1, 2] = (double)grandTotal;

                Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
                summaryRange.Font.Bold = true;

                summarySheet.Columns.AutoFit();

                application.Visible = true;

                // Сохраняем файл
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                workbook.SaveAs($@"{desktopPath}\Payments.xlsx");

                MessageBox.Show("Данные экспортированы в Excel на рабочий стол", "Успешно",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Excel: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList();
                var allCategories = _context.Category.ToList();

                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (var user in allUsers)
                {
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userParagraph.set_Style("Заголовок 1");
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add();

                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count() + 1, 2);

                    paymentsTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    Word.Range cellRange;
                    cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.Bold = 1;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];
                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        cellRange = paymentsTable.Cell(i + 2, 2).Range;
                        var sum = user.Payment.ToList()
                            .Where(u => u.Category == currentCategory)
                            .Sum(u => u.Num * u.Price);
                        cellRange.Text = string.Format("{0:N2} руб.", sum);
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }
                    document.Paragraphs.Add();

                    // Максимальный платеж
                    Payment maxPayment = user.Payment.OrderByDescending(u => u.Price * u.Num).FirstOrDefault();
                    if (maxPayment != null)
                    {
                        Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                        Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                        maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} за {maxPayment.Price * maxPayment.Num:N2} руб. от {maxPayment.Date?.ToString("dd.MM.yyyy") ?? "не указана"}";
                        maxPaymentParagraph.set_Style("Заголовок 2");
                        maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxPaymentRange.InsertParagraphAfter();
                    }
                    document.Paragraphs.Add();

                    // Минимальный платеж
                    Payment minPayment = user.Payment.Where(p => (p.Price * p.Num) > 0).OrderBy(u => u.Price * u.Num).FirstOrDefault();
                    if (minPayment != null)
                    {
                        Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                        Word.Range minPaymentRange = minPaymentParagraph.Range;
                        minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} за {minPayment.Price * minPayment.Num:N2} руб. от {minPayment.Date?.ToString("dd.MM.yyyy") ?? "не указана"}";
                        minPaymentParagraph.set_Style("Заголовок 2");
                        minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minPaymentRange.InsertParagraphAfter();
                    }

                    if (user != allUsers.LastOrDefault())
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }

                // Нижний колонтитул с номерами страниц
                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                // Верхний колонтитул с датой
                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 10;
                    headerRange.Text = DateTime.Now.ToString("dd/MM/yyyy");
                }

                application.Visible = true;

                // Сохраняем файлы
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                document.SaveAs2($@"{desktopPath}\Payments.docx");
                document.SaveAs2($@"{desktopPath}\Payments.pdf", Word.WdExportFormat.wdExportFormatPDF);

                MessageBox.Show("Данные экспортированы в Word и PDF на рабочий стол", "Успешно",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Word: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}