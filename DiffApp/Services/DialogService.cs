using DiffApp.Views;

namespace DiffApp.Services
{
    public class DialogService : IDialogService
    {
        public DialogResult ShowDialog(string message, string title, DialogButtons buttons, DialogImage image)
        {
            var viewModel = new CustomDialogViewModel(message, title, buttons, image);

            var window = new CustomDialogWindow
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };

            viewModel.CloseAction = () => window.Close();

            window.ShowDialog();

            return viewModel.Result;
        }
    }
}