using System.Windows;
using System.Windows.Input;
using MvvmFoundation.Wpf;

namespace BubbleBurst.ViewModel
{
    public class GameOverViewModel
    {
        readonly BubbleMatrixViewModel _bubbleMatrix;

        internal GameOverViewModel(BubbleMatrixViewModel bubbleMatrix)
        {
            if (bubbleMatrix == null)
                throw new ArgumentNullException("bubbleMatrix");

            _bubbleMatrix = bubbleMatrix;

            if (_bubbleMatrix.Bubbles.Count == 0)
            {
                Title = "CONGRATULATIONS!";
            }
            else
            {
                var pluralEnding = bubbleMatrix.Bubbles.Count == 1 ? string.Empty : "S";
                Title = $"{bubbleMatrix.Bubbles.Count} BUBBLE{pluralEnding} LEFT";
            }
        }

        public event EventHandler RequestClose;

        public ICommand QuitCommand => new RelayCommand(Application.Current.Shutdown);

        public string Subtitle => $"Most bubbles popped at once: {_bubbleMatrix.MostBubblesPoppedAtOnce}";

        public string Title { get; }

        public void Close()
        {
            RaiseRequestClose();
        }

        public void StartNewGame()
        {
            _bubbleMatrix.StartNewGame();
        }

        private void RaiseRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}