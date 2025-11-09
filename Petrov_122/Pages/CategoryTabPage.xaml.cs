using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Petrov_122.Pages
{
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            DataGridCategory.ItemsSource =
            Entities.GetContext().Category.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;

        }
        private void Page_IsVisibleChanged(object sender,
        DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(x
                => x.Reload());
                DataGridCategory.ItemsSource =
                Entities.GetContext().Category.ToList();
            }
        }
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var categoriesForRemoving = DataGridCategory.SelectedItems.Cast<Category>().ToList();

            if (MessageBox.Show($"Вы точно хотите удалить {categoriesForRemoving.Count} категорий?",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var context = Entities.GetContext();

                    foreach (var category in categoriesForRemoving)
                    {
                        var payments = context.Payment.Where(p => p.CategoryID == category.ID).ToList();
                        context.Payment.RemoveRange(payments);
                    }

                    context.Category.RemoveRange(categoriesForRemoving);
                    context.SaveChanges();

                    MessageBox.Show("Данные успешно удалены!");
                    DataGridCategory.ItemsSource = Entities.GetContext().Category.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Pages.AddCategoryPage((sender as
            Button).DataContext as Category));
        }
    }
}


