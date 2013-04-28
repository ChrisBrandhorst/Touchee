using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Playback {
    
    /// <summary>
    /// Represents a playback queue
    /// </summary>
    public class Queue {


        #region Privates

        /// <summary>
        /// Contains the items in the original order
        /// </summary>
        List<QueueItem> _itemsOriginal;

        /// <summary>
        /// Contains the items in the active order
        /// </summary>
        List<QueueItem> _items;

        /// <summary>
        /// Shuffle value
        /// </summary>
        bool _shuffle = false;

        /// <summary>
        /// Position of the queue
        /// </summary>
        int _index = -1;

        /// <summary>
        /// The start- and end index of the priority items in this queue
        /// </summary>
        int _priorityStart = -1;

        /// <summary>
        /// The length of the priority section of this queue
        /// </summary>
        int _priorityCount = 0;

        #endregion




        #region Properties


        /// <summary>
        /// All items in the queue in the order in which they will be played
        /// </summary>
        public IEnumerable<QueueItem> Items {
            get {
                return _items;
            }
        }


        /// <summary>
        /// The upcoming items in the queue in the order in which they will be played
        /// </summary>
        public IEnumerable<QueueItem> Upcoming {
            get {
                var start = _index + 1;
                return start > _items.Count ? new List<QueueItem>() : _items.GetRange(start, _items.Count - start);
            }
        }


        /// <summary>
        /// Returns the current item in the queue
        /// </summary>
        public QueueItem Current {
            get {
                return Index >= 0 && Index < _items.Count ? _items[Index] : null;
            }
        }


        /// <summary>
        /// The current and upcoming items in the queue in the order in which they will be played
        /// </summary>
        public IEnumerable<QueueItem> CurrentAndUpcoming {
            get {
                var start = _index;
                return start > _items.Count ? new List<QueueItem>() : _items.GetRange(start, _items.Count - start);
            }
        }


        /// <summary>
        /// Returns the previous item in the queue
        /// </summary>
        public QueueItem Prev {
            get {
                var i = _index - 1;
                return i >= 0 && i < _items.Count ? _items[i] : null;
            }
        }


        /// <summary>
        /// Returns the next item in the queue
        /// </summary>
        public QueueItem Next {
            get {
                var i = _index + 1;
                return i >= 0 && i < _items.Count ? _items[i] : null;
            }
        }


        /// <summary>
        /// Returns the item at the given index
        /// </summary>
        /// <param name="index">The index of the item</param>
        /// <exception cref="ArgumentOutOfRangeException">If the given value is out of range</exception>
        public QueueItem ItemAt(int index) {
            return this[index];
        }


        /// <summary>
        /// Returns the item at the given index
        /// </summary>
        /// <param name="index">The index of the item</param>
        /// <exception cref="ArgumentOutOfRangeException">If the given value is out of range</exception>
        public QueueItem this[int index] {
            get {
                return _items[index];
            }
        }


        /// <summary>
        /// The index of the current item in the queue
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the given value is less than 0 or larger than the length of the queue</exception>
        public int Index {
            get { return _index; }
            set {
                
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Given value is smaller than 0");
                else if (value > _items.Count)
                    throw new ArgumentOutOfRangeException("Given value is larger than the length of the queue");
                else {

                    // If a value equal to the number of items is given, go back to before the start
                    _index = value == _items.Count ? -1 : value;

                    // Clear priority queue if moved to outside the priority part
                    if (_priorityStart > -1 && (_index == -1 || _index > _priorityEnd))
                        ClearPriority();

                    // Callbacks
                    if (IndexChanged != null)
                        IndexChanged.Invoke(this);
                    if (Finished != null && value == _items.Count)
                        Finished.Invoke(this);
                    
                }
            }
        }


        /// <summary>
        /// The last index of the priority queue
        /// </summary>
        int _priorityEnd {
            get {
                return _priorityStart == -1 ? -1 : _priorityStart + _priorityCount - 1;
            }
        }


        /// <summary>
        /// The number of items in the upcoming queue which are priority items
        /// </summary>
        public int UpcomingPriorityCount {
            get {
                if (_priorityStart == -1)
                    return 0;
                else
                    return _priorityEnd - _index;
            }
        }


        /// <summary>
        /// Whether the queue has started
        /// </summary>
        public bool IsBeforeFirstItem { get { return _index == -1; } }


        /// <summary>
        /// Whether the current item is the first item of the queue
        /// </summary>
        public bool IsAtFirstItem { get { return _index == 0; } }


        /// <summary>
        /// Whether the current item is the last item of the queue
        /// </summary>
        public bool IsAtLastItem { get { return _index == Items.Count() - 1; } }


        /// <summary>
        /// Gets or sets the shuffling of the queue
        /// </summary>
        public bool Shuffle {
            get { return _shuffle; }
            set {
                if (_shuffle == value) return;

                // We are going to shuffle
                if (_shuffle = value) {
                    
                }
                // Back to original
                else {
                    
                }
            }
        }


        /// <summary>
        /// The repeat mode of the queue
        /// </summary>
        public RepeatMode Repeat { get; set; }


        #endregion



        
        #region Constructors

        /// <summary>
        /// Constructs a new, empty queue.
        /// </summary>
        public Queue() {
            Repeat = RepeatMode.Off;
            _items = new List<QueueItem>();
            _itemsOriginal = new List<QueueItem>();
        }


        /// <summary>
        /// Constructs a new queue with one QueueItem in it.
        /// </summary>
        /// <param name="item">The item to add to the queue</param>
        public Queue(QueueItem item) : this() {
            _itemsOriginal.Add(item);
            _items.Add(item);
        }


        /// <summary>
        /// Constructs a new queue with a number of QueueItems in it.
        /// </summary>
        /// <param name="items">The initial set of items in the queue</param>
        public Queue(IEnumerable<QueueItem> items) : this() {
            _itemsOriginal.AddRange(items);
            _items.AddRange(items);
        }


        /// <summary>
        /// Constructs a new queue with one item in it.
        /// </summary>
        /// <param name="item">The item to add to the queue</param>
        /// <param name="container">The container from which the item is added</param>
        public Queue(IItem item, Container container) : this(new QueueItem(container, item)) { }


        /// <summary>
        /// Constructs a new queue with a number of items in it.
        /// </summary>
        /// <param name="item">The item to add to the queue</param>
        /// <param name="container">The container from which the item is added</param>
        public Queue(IEnumerable<IItem> items, Container container) : this(items.Select(i => new QueueItem(container, i))) { }


        #endregion




        #region Enqueueing



        /// <summary>
        /// Completele clears the queue
        /// </summary>
        public void Clear() {
            Index = -1;
            _items.Clear();
            _priorityStart = -1;
            _priorityCount = 0;
            OnItemsUpdated();
        }


        /// <summary>
        /// Clears the upcoming items from the queue
        /// </summary>
        public void ClearUpcoming() {
            int start = _priorityStart == -1 ? _index + 1 : _priorityEnd + 1;
            if (start < _items.Count) {
                _items.RemoveRange(start, _items.Count - start);
                OnItemsUpdated();
            }
        }


        /// <summary>
        /// Clears the priority queue
        /// </summary>
        public void ClearPriority() {
            if (ClearPriorityInternal())
                OnItemsUpdated();
        }


        /// <summary>
        /// Clears the priority queue
        /// </summary>
        /// <returns>True if the upcoming items is modified</returns>
        bool ClearPriorityInternal() {
            bool ret = false;

            if (_priorityStart != -1) {
                int first, count;

                // If the current position is after the priority part, silently remove the priority part
                if (_index > _priorityEnd) {
                    first = _priorityStart;
                    count = _priorityCount;
                    _index -= count;
                }
                // Else, remove upcoming priority items
                else {
                    first = _index + 1;
                    count = UpcomingPriorityCount;
                    ret = true;
                }
                _items.RemoveRange(first, count);
                _priorityStart = -1;
                _priorityCount = 0;
            }

            return ret;
        }



        /// <summary>
        /// Appends the given item to the end of the queue
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Push(QueueItem item) {
            _items.Add(item);
            OnItemsUpdated();
        }


        /// <summary>
        /// Appends the given items to the end of the queue
        /// </summary>
        /// <param name="items">The items to add</param>
        public void Push(IEnumerable<QueueItem> items) {
            _items.AddRange(items);
            OnItemsUpdated();
        }


        /// <summary>
        /// Prepends the given item to the start of the priority queue
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Prioritize(QueueItem item) {
            Prioritize(new List<QueueItem>() { item });
        }


        /// <summary>
        /// Prepends the given items to the start of the priority queue
        /// </summary>
        /// <param name="items">The items to add</param>
        public void Prioritize(IEnumerable<QueueItem> items) {
            EnlargePriority(items.Count());
            _items.InsertRange(_priorityStart, items);
            OnItemsUpdated();
        }


        /// <summary>
        /// Appends the given item to the end of the priority queue
        /// </summary>
        /// <param name="item">The item to add</param>
        public void PushToPriority(QueueItem item) {
            PushToPriority(new List<QueueItem>() { item });
        }


        /// <summary>
        /// Appends the given items to the end of the priority queue
        /// </summary>
        /// <param name="items">The items to add</param>
        public void PushToPriority(IEnumerable<QueueItem> items) {
            EnlargePriority(items.Count());
            _items.InsertRange(_priorityEnd + 1, items);
            OnItemsUpdated();
        }


        /// <summary>
        /// Enlarges the priority queue with the given number
        /// </summary>
        /// <param name="count">The number of items to enlarge the priority queue with</param>
        void EnlargePriority(int count) {
            if (_priorityStart == -1) {
                _priorityStart = _index + 1;
                _priorityCount = 0;
            }
            _priorityCount += count;
        }


        #endregion




        #region Queue control


        /// <summary>
        /// Go to the previous item
        /// </summary>
        /// <returns>The new current item, or null if none</returns>
        public QueueItem GoPrev() {
            --Index;
            return Current;
        }


        /// <summary>
        /// Move to the next item
        /// </summary>
        /// <returns>The new current item, or null if none</returns>
        public QueueItem GoNext() {
            return this.GoNext(false);
        }


        /// <summary>
        /// Move to the next item
        /// </summary>
        /// <param name="ignoreRepeat">Whether the repeat property should be ignored when determining the nex item</param>
        /// <returns>The new current item, or null if none</returns>
        public QueueItem GoNext(bool ignoreRepeat) {

            // Calculate next index
            int nextIndex = Index + 1;

            // Handle repeat
            switch (Repeat) {

                // Go to beginning when at the end of the list, regardless of ignoreRepeat
                case RepeatMode.All:
                    if (nextIndex == _items.Count)
                        nextIndex = 0;
                    break;

                // Repeat the current item, unless forced into next
                case RepeatMode.One:
                    if (!ignoreRepeat)
                        nextIndex = Index;
                    break;

            }

            // Set index and return current item
            Index = nextIndex;
            return Current;
        }


        /// <summary>
        /// Toggles the shuffle property
        /// </summary>
        /// <returns>The new value for the shuffle property</returns>
        public bool ToggleShuffle() {
            return Shuffle = !Shuffle;
        }


        /// <summary>
        /// Toggles the repeat property.
        /// Cycles in the order Alle, One, Off
        /// </summary>
        /// <returns>The new value for the repeat property</returns>
        public RepeatMode ToggleRepeat() {
            switch (Repeat) {
                case RepeatMode.Off:
                    Repeat = RepeatMode.All;
                    break;
                case RepeatMode.All:
                    Repeat = RepeatMode.One;
                    break;
                case RepeatMode.One:
                    Repeat = RepeatMode.Off;
                    break;
            }
            return Repeat;
        }


        #endregion




        #region Events

        public event QueueItemsUpdatedEventHandler ItemsUpdated;
        public event QueueIndexChangedEventHandler IndexChanged;
        public event QueueFinishedEventHandler Finished;

        /// <summary>
        /// Called when items are updated: firest the event
        /// </summary>
        void OnItemsUpdated() {
            if (ItemsUpdated != null)
                ItemsUpdated.Invoke(this);
        }

        #endregion




        #region Helpers


        #endregion



        public delegate void QueueItemsUpdatedEventHandler(Queue queue);
        public delegate void QueueIndexChangedEventHandler(Queue queue);
        public delegate void QueueFinishedEventHandler(Queue queue);

    }

}
