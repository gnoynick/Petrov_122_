using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Petrov_122.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }

            string hashedPass = GetHash(CurrentPasswordBox.Password);
            using (var db = new Entities())
            {
                var user = db.User
                    .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);

                if (user == null)
                {
                    MessageBox.Show("Текущий пароль/Логин неверный!");
                    return;
                }

                if (NewPasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                    return;
                }

                bool en = true;
                bool number = false;

                for (int i = 0; i < NewPasswordBox.Password.Length; i++)
                {
                    char c = NewPasswordBox.Password[i];

                    if (c >= '0' && c <= '9')
                        number = true;
                    else if (!((c >= 'A' && c <= 'Z') ||
                              (c >= 'a' && c <= 'z') ||
                              (c >= 'А' && c <= 'Я') ||
                              (c >= 'а' && c <= 'я') ||
                              c == 'Ё' || c == 'ё' ||
                              IsSpecialCharacter(c)))
                    {
                        en = false;
                        break;
                    }
                }

                if (!en)
                {
                    MessageBox.Show("Пароль содержит недопустимые символы! Разрешены только буквы (русские и английские), цифры и основные специальные символы.");
                    return;
                }
                else if (!number)
                {
                    MessageBox.Show("Добавьте хотя бы одну цифру!");
                    return;
                }

                if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Новые пароли не совпадают!");
                    return;
                }

                if (en && number)
                {
                    user.Password = GetHash(NewPasswordBox.Password);
                    db.SaveChanges();
                    MessageBox.Show("Пароль успешно изменен!");
                    NavigationService?.Navigate(new AuthPage());
                }
            }
        }

        private bool IsSpecialCharacter(char c)
        {
            string allowedSpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            return allowedSpecialCharacters.Contains(c);
        }
    }
}