namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Represents some work that BubbleMatrixView must 
    /// process for a given set of BubbleViewModels.
    /// </summary>
    public class BubblesTask
    {
        #region Constructor

        internal BubblesTask(BubblesTaskType taskType, bool isUndo, Func<IEnumerable<BubbleViewModel>> getBubbles, Action complete)
        {
            TaskType = taskType;
            IsUndo = isUndo;
            _getBubbles = getBubbles;
            Complete = complete;
        }

        #endregion // Constructor

        #region Properties

        public IEnumerable<BubbleViewModel> Bubbles
        {
            get
            {
                if (_bubbles == null)
                {
                    // The list of bubbles associated with this task is
                    // retrieved once, on demand, because retrieving the 
                    // list can have side effects.
                    _bubbles = _getBubbles().ToArray();
                }
                return _bubbles;
            }
        }

        public Action Complete { get; private set; }

        public bool IsUndo { get; private set; }

        public BubblesTaskType TaskType { get; private set; }

        #endregion // Properties

        #region Fields

        BubbleViewModel[] _bubbles;
        readonly Func<IEnumerable<BubbleViewModel>> _getBubbles;

        #endregion // Fields
    }
}