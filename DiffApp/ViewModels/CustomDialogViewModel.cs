using DiffApp.Models;
using DiffApp.Helpers;

namespace DiffApp.ViewModels
{
    public class CustomDialogViewModel : ViewModelBase
    {
        private DialogResult _result = DialogResult.None;

        public string Title { get; }
        public string Message { get; }
        public DialogImage Image { get; }
        public DialogButtons Buttons { get; }

        public bool IsOkVisible => Buttons == DialogButtons.Ok;
        public bool IsYesNoVisible => Buttons == DialogButtons.YesNo;
        public bool IsConfirmCancelVisible => Buttons == DialogButtons.ConfirmCancel;

        public string IconText { get; private set; } = string.Empty;
        public string IconColorKey { get; private set; } = "BrushPrimary";

        public ICommand ResultCommand { get; }

        public Action? CloseAction { get; set; }

        public DialogResult Result => _result;

        public CustomDialogViewModel(string message, string title, DialogButtons buttons, DialogImage image)
        {
            Message = message;
            Title = title;
            Buttons = buttons;
            Image = image;

            ResultCommand = new RelayCommand(OnResult);
            SetupIcon();
        }

        private void OnResult(object? parameter)
        {
            if (parameter is DialogResult result)
            {
                _result = result;
                CloseAction?.Invoke();
            }
        }

        private void SetupIcon()
        {
            switch (Image)
            {
                case DialogImage.Information:
                    IconText = "\ue88e";
                    IconColorKey = "BrushPrimary";
                    break;
                case DialogImage.Question:
                    IconText = "\ue8fd";
                    IconColorKey = "BrushAccent";
                    break;
                case DialogImage.Warning:
                    IconText = "\ue002";
                    IconColorKey = "WarningBrush";
                    break;
                case DialogImage.Error:
                    IconText = "\ue000";
                    IconColorKey = "DangerBrush";
                    break;
                default:
                    IconText = string.Empty;
                    break;
            }
        }
    }
}