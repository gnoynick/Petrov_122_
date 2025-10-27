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
            if (string.IsNullOrWhiteSpace(TBDateTime.Value?.ToString()))
                errors.AppendLine("Укажите дату и время!");
            if (string.IsNullOrWhiteSpace(_currentPayment.Num.ToString()))
                errors.AppendLine("Укажите количество!");
            if (string.IsNullOrWhiteSpace(_currentPayment.Price.ToString()))
                errors.AppendLine("Укажите цену!");
            if (string.IsNullOrWhiteSpace(_currentPayment.UserID.ToString()))
                errors.AppendLine("Укажите клиента!");
            if (string.IsNullOrWhiteSpace(_currentPayment.CategoryID.ToString()))
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
            TBDateTime.Value = null;
            CBCategory.SelectedIndex = -1;
            CBUser.SelectedIndex = -1;
        }
    }
}