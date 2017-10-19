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
        #region Constructor

        public BubbleBurstView()
        {
            LoadBubbleViewResources();

            InitializeComponent();

            _bubbleBurst = base.DataContext as BubbleBurstViewModel;
            _bubbleMatrixView.MatrixDimensionsAvailable += this.HandleMatrixDimensionsAvailable;
        }

        static void LoadBubbleViewResources()
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

        #endregion // Constructor

        #region Methods

        void HandleMatrixDimensionsAvailable(object sender, EventArgs e)
        {
            // Hook the keyboard event on the Window because this
            // control does not receive keystrokes.
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown += this.HandleWindowPreviewKeyDown;
            }

            this.StartNewGame();
        }

        void HandleWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool undo = 
                Keyboard.Modifiers == ModifierKeys.Control && 
                e.Key == Key.Z;

            if (undo && _bubbleBurst.CanUndo)
            {
                _bubbleBurst.BubbleMatrix.Undo();
                e.Handled = true;
            }
        }

        void StartNewGame()
        {
            int rows = _bubbleMatrixView.RowCount;
            int cols = _bubbleMatrixView.ColumnCount;
            _bubbleBurst.BubbleMatrix.SetDimensions(rows, cols);
            _bubbleBurst.BubbleMatrix.StartNewGame();
        }

        #endregion // Methods

        #region Fields

        readonly BubbleBurstViewModel _bubbleBurst;

        #endregion // Fields
    }
}