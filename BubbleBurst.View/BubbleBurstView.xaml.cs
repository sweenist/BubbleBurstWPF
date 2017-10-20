using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BubbleBurst.ViewModel;

namespace BubbleBurst.View
{
    /// <summary>
    /// The top-level View of the game, which contains a bubble matrix,
    /// game-over dialog, and the context menu.
    /// </summary>
    public partial class BubbleBurstView : UserControl
    {
        private readonly BubbleBurstViewModel _bubbleBurst;

        /// <summary>Initializes a new instance of the <see cref="BubbleBurstView"/> class.</summary>
        public BubbleBurstView()
        {
            LoadBubbleViewResources();

            InitializeComponent();

            _bubbleBurst = DataContext as BubbleBurstViewModel;
            _bubbleMatrixView.MatrixDimensionsAvailable += HandleMatrixDimensionsAvailable;
        }

        private static void LoadBubbleViewResources()
        {
            // Insert the BubbleView resources at the App level to avoid resource duplication.
            // If we insert the resources into this control's Resources collection, every time
            // a BubbleView is removed from the UI some ugly debug warning messages are spewed out.
            string path = "pack://application:,,,/BubbleBurst.View;component/BubbleViewResources.xaml";
            var bubbleViewResources = new ResourceDictionary
            {
                Source = new Uri(path)
            };
            Application.Current.Resources.MergedDictionaries.Add(bubbleViewResources);
        }

        private void HandleMatrixDimensionsAvailable(object sender, EventArgs e)
        {
            // Hook the keyboard event on the Window because this
            // control does not receive keystrokes.
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown += HandleWindowPreviewKeyDown;
            }

            StartNewGame();
        }

        private void HandleWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool undo = Keyboard.Modifiers.Equals(ModifierKeys.Control)
                && e.Key.Equals(Key.Z);

            if (undo && _bubbleBurst.CanUndo)
            {
                _bubbleBurst.BubbleMatrix.Undo();
                e.Handled = true;
            }
        }

        private void StartNewGame()
        {
            var rows = _bubbleMatrixView.RowCount;
            var cols = _bubbleMatrixView.ColumnCount;
            _bubbleBurst.BubbleMatrix.SetDimensions(rows, cols);
            _bubbleBurst.BubbleMatrix.StartNewGame();
        }
    }
}