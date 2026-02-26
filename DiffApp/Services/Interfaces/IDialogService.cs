namespace DiffApp.Services.Interfaces
{
    public interface IDialogService
    {
        DialogResult ShowDialog(string message, string title, DialogButtons buttons, DialogImage image);
    }
}