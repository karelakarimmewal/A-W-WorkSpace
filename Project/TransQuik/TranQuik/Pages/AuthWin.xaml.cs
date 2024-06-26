using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TranQuik.Model;

namespace TranQuik.Pages
{
    public partial class AuthWin : Window
    {
        private Control currentFocusedControl;
        private UserSessions userSessions;
        
        public AuthWin(UserSessions usersessions)
        {
            InitializeComponent();
            userSessions = usersessions;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentFocusedControl = sender as Control;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentFocusedControl = sender as Control;
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string buttonText = button.Content.ToString();

                if (currentFocusedControl == userCodeInput)
                {
                    userCodeInput.Password += buttonText;
                }
                else if (currentFocusedControl == userPasswordInput)
                {
                    userPasswordInput.Password += buttonText;
                }
            }
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (currentFocusedControl == userCodeInput && userCodeInput.Password.Length > 0)
            {
                userCodeInput.Password = userCodeInput.Password.Substring(0, userCodeInput.Password.Length - 1);
            }
            else if (currentFocusedControl == userPasswordInput && userPasswordInput.Password.Length > 0)
            {
                userPasswordInput.Password = userPasswordInput.Password.Substring(0, userPasswordInput.Password.Length - 1);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (currentFocusedControl == userCodeInput)
            {
                userCodeInput.Password = string.Empty;
            }
            else if (currentFocusedControl == userPasswordInput)
            {
                userPasswordInput.Clear();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task SaveUserSessionAsync(int staffID, int staffRoleID, string staffFirstName, string staffLastName)
        {
            // Simulate an asynchronous save operation. Replace this with your actual save logic.
            await Task.Run(() =>
            {
                userSessions.Authentication_StaffID = staffID;
                userSessions.Authentication_StaffRoleID = staffRoleID;
                userSessions.Authentication_StaffFirstName = staffFirstName;
                userSessions.Authentication_StaffLastName = staffLastName;
            });
        }

        private async void authenticationButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            string userCode = userCodeInput.Password;
            string userPassword = userPasswordInput.Password;
            UserAuth userAuth = new UserAuth();
            var (isLogged, staffID, staffRoleID, staffFirstName, staffLastName) = await userAuth.AuthenticateUserAsync(userCode, userPassword);

            if (isLogged)
            {
                await SaveUserSessionAsync(staffID, staffRoleID, staffFirstName, staffLastName);

                this.Close();
            }
            else
            {
                Notification.NotificationLoginFailed();
            }
        }


    }
}
