using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Petrov_122.Pages
{
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment = new Payment();

        public AddPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();

            CBCategory.ItemsSource = Entities.GetContext().Category.ToList();
            CBCategory.DisplayMemberPath = "Name";
            CBUser.ItemsSource = Entities.GetContext().User.ToList();
            CBUser.DisplayMemberPath = "FIO";

            if (selectedPayment != null)
                _currentPayment = selectedPayment;

            DataContext = _currentPayment;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentPayment.Name))
                errors.AppendLine("Укажите название платежа!");

            // Проверка даты
            if (string.IsNullOrWhiteSpace(TBDate.Text))
                errors.AppendLine("Укажите дату!");
            else
            {
                // Пытаемся преобразовать текст в дату
                if (DateTime.TryParse(TBDate.Text, out DateTime parsedDate))
                {
                    _currentPayment.Date = parsedDate;
                }
                else
                {
                    errors.AppendLine("Укажите дату в правильном формате! (например: 15.12.2024)");
                }
            }

            if (string.IsNullOrWhiteSpace(_currentPayment.Num.ToString()) || _currentPayment.Num <= 0)
                errors.AppendLine("Укажите количество!");
            if (string.IsNullOrWhiteSpace(_currentPayment.Price.ToString()) || _currentPayment.Price <= 0)
                errors.AppendLine("Укажите цену!");
            if (string.IsNullOrWhiteSpace(_currentPayment.UserID.ToString()) || _currentPayment.UserID <= 0)
                errors.AppendLine("Укажите клиента!");
            if (string.IsNullOrWhiteSpace(_currentPayment.CategoryID.ToString()) || _currentPayment.CategoryID <= 0)
                errors.AppendLine("Укажите категорию!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_currentPayment.ID == 0)
                Entities.GetContext().Payment.Add(_currentPayment);

            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBPaymentName.Text = "";
            TBAmount.Text = "";
            TBCount.Text = "";
            TBDate.Text = "";
            CBCategory.SelectedIndex = -1;
            CBUser.SelectedIndex = -1;
        }
    }
}