using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Petrov_122.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0;
        }

        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return
                string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x =>
                x.ToString("X2")));
            }
        }

        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }

        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxFrst.Focus();
        }

        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxScnd.Focus();
        }

        private void lblFioHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxFIO.Focus();
        }

        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = Visibility.Visible;
            if (txtbxLog.Text.Length > 0)
            {
                lblLogHitn.Visibility = Visibility.Hidden;
            }
        }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = Visibility.Visible;
            if (txtbxFIO.Text.Length > 0)
            {
                lblFioHitn.Visibility = Visibility.Hidden;
            }
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = Visibility.Visible;
            if (passBxFrst.Password.Length > 0)
            {
                lblPassHitn.Visibility = Visibility.Hidden;
            }
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = Visibility.Visible;
            if (passBxScnd.Password.Length > 0)
            {
                lblPassSecHitn.Visibility = Visibility.Hidden;
            }
        }

        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            using (Entities db = new Entities())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == txtbxLog.Text);

                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }

                if (passBxFrst.Password.Length < 6)
                {
                    MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                    return;
                }

                bool hasNumber = false;
                bool hasValidCharacters = true;

                for (int i = 0; i < passBxFrst.Password.Length; i++)
                {
                    char c = passBxFrst.Password[i];

                    // Проверка на цифры
                    if (c >= '0' && c <= '9')
                        hasNumber = true;
                    // Проверка на допустимые символы: английские буквы, русские буквы и специальные символы
                    else if (!((c >= 'A' && c <= 'Z') ||
                              (c >= 'a' && c <= 'z') ||
                              (c >= 'А' && c <= 'Я') ||
                              (c >= 'а' && c <= 'я') ||
                              c == 'Ё' || c == 'ё' ||
                              IsSpecialCharacter(c)))
                    {
                        hasValidCharacters = false;
                        break;
                    }
                }

                if (!hasValidCharacters)
                {
                    MessageBox.Show("Пароль содержит недопустимые символы! Разрешены только буквы (русские и английские), цифры и основные специальные символы.");
                    return;
                }
                else if (!hasNumber)
                {
                    MessageBox.Show("Добавьте хотя бы одну цифру!");
                    return;
                }

                if (passBxFrst.Password != passBxScnd.Password)
                {
                    MessageBox.Show("Пароли не совпадают!");
                    return;
                }

                User userObject = new User
                {
                    FIO = txtbxFIO.Text,
                    Login = txtbxLog.Text,
                    Password = GetHash(passBxFrst.Password),
                    Role = comboBxRole.Text
                };

                db.User.Add(userObject);
                db.SaveChanges();

                MessageBox.Show("Пользователь успешно зарегистрирован!");

                txtbxLog.Clear();
                passBxFrst.Clear();
                passBxScnd.Clear();
                comboBxRole.SelectedIndex = 0;
                txtbxFIO.Clear();
            }
        }

        // Метод для проверки специальных символов
        private bool IsSpecialCharacter(char c)
        {
            // Разрешенные специальные символы
            string allowedSpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            return allowedSpecialCharacters.Contains(c);
        }
    }
}