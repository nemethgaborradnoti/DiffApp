using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiffApp.Models
{
    public class ChangeBlock : INotifyPropertyChanged
    {
        public Guid Id { get; } = Guid.NewGuid();
        public BlockType Kind { get; set; }
        public List<ChangeLine> OldLines { get; } = new();
        public List<ChangeLine> NewLines { get; } = new();

        public bool IsMergeable => Kind != BlockType.Unchanged;

        public int StartIndexOld { get; set; }
        public int StartIndexNew { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isHovered;
        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered != value)
                {
                    _isHovered = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}