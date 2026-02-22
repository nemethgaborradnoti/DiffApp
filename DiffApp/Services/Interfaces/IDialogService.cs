namespace DiffApp.Services.Interfaces
{
    public interface IDialogService
    {
        bool Confirm(string message, string title);
        void Alert(string message, string title);
    }
}