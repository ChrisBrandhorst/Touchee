using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Touchee.Media;
using Touchee.Components.Playback;

namespace Touchee.Playback {
    
    /// <remarks>
    /// Represents a playing queue
    /// </remarks>
    public class Queue : Collectable<Queue> {


        #region Privates

        /// <summary>
        /// Contains the (possible shuffled) non-priority items
        /// </summary>
        List<IItem> _items;
        /// <summary>
        /// Contains the unshuffled non-priority items
        /// </summary>
        List<IItem> _itemsUnshuffled;

        /// <summary>
        /// Shuffle value
        /// </summary>
        bool _shuffle = false;

        /// <summary>
        /// Index value
        /// </summary>
        int _index = -1;

        #endregion


        #region Properties

        /// <summary>
        /// The items in the queue in the order in which they will be played
        /// </summary>
        public IEnumerable<IItem> Items { get { return _items; } }


        /// <summary>
        /// Gets or sets the shuffling of the queue
        /// </summary>
        public bool Shuffle {
            get { return _shuffle; }
            set {
                if (_shuffle == value) return;
                if (_shuffle = value) {
                    var current = Current;
                    _items.Clear();
                    _items.AddRange(_itemsUnshuffled);
                    _items.RemoveAt(Index);
                    _items.Shuffle();
                    _items.Insert(0, current);
                    Index = 0;
                }
                else {
                    ResetItems();
                }
            }
        }


        /// <summary>
        /// The repeat mode of the queue
        /// </summary>
        public RepeatMode Repeat { get; set; }


        /// <summary>
        /// The current item in the queue
        /// </summary>
        public IItem Current {
            get { return Index >= 0 && Index < _items.Count ? _items[Index] : null; }
            set { Index = _items.IndexOf(value); }
        }


        /// <summary>
        /// Returns the previous item in the queue
        /// </summary>
        public IItem Prev { get {
            var i = _index - 1;
            return i >= 0 && i < _items.Count ? _items[i] : null;
        } }


        /// <summary>
        /// Returns the next item in the queue
        /// </summary>
        public IItem Next { get {
            var i = _index + 1;
            return i >= 0 && i < _items.Count ? _items[i] : null;
        } }


        /// <summary>
        /// The index of the current item in the queue
        /// </summary>
        public int Index {
            get { return _index; }
            set {
                //if (value == _index)
                //    return;
                //else 
                if (value >= 0 && value < _items.Count) {
                    var previous = Current;
                    _index = value;
                    if (IndexChanged != null)
                        IndexChanged.Invoke(this, previous, Current);
                }
                else {
                    _index = -1;
                    if (Finished != null)
                        Finished.Invoke(this);
                }

            }
        }


        /// <summary>
        /// Whether the queue has started
        /// </summary>
        public bool IsBeforeFirstItem { get { return Current != null; } }


        /// <summary>
        /// Whether the current item is the first item of the queue
        /// </summary>
        public bool IsAtFirstItem { get { return _index == 0; } }


        /// <summary>
        /// Whether the current item is the last item of the queue
        /// </summary>
        public bool IsAtLastItem { get { return _index == _items.Count - 1; } }


        /// <summary>
        /// The content type of the items in the queue
        /// </summary>
        public string ContentType { get; protected set; }


        /// <summary>
        /// Can be used for keeping track of the current player
        /// </summary>
        public IPlayer CurrentPlayer { get; set; }


        #endregion


        #region Events

        public event QueueItemsUpdatedEventHandler ItemsUpdated;
        public event QueueIndexChangedEventHandler IndexChanged;
        public event QueueFinishedEventHandler Finished;

        #endregion


        #region Constructors

        /// <summary>
        /// Constructs a new, empty queue.
        /// </summary>
        /// <param name="contentType">The content type of the items in the queue</param>
        public Queue(string contentType) {
            ContentType = contentType;
            Repeat = RepeatMode.Off;
            _items = new List<IItem>();
            _itemsUnshuffled = new List<IItem>();
        }


        /// <summary>
        /// Constructs a new queue with one item in it.
        /// </summary>
        /// <param name="item">The item to add to the queue</param>
        /// <param name="contentType">The content type of the items in the queue</param>
        public Queue(IItem item, string contentType) : this(contentType) {
            _itemsUnshuffled.Add(item);
            ResetItems();
        }


        /// <summary>
        /// Constructs a new queue with a number of items in it.
        /// </summary>
        /// <param name="items">The initial set of items in the queue</param>
        /// <param name="contentType">The content type of the items in the queue</param>
        public Queue(IEnumerable<IItem> items, string contentType) : this(contentType) {
            _itemsUnshuffled.AddRange(items);
            ResetItems();
        }


        #endregion


        #region Enqueueing


        /// <summary>
        /// Enqueue an item at the end of the queue
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Enqueue(IItem item) {
            _items.Add(item);
            _itemsUnshuffled.Add(item);
            OnItemsUpdated();
        }


        /// <summary>
        /// Enqueue an item at a specific index
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="index">The index to add the item at</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than Count.</exception>
        public void Enqueue(IItem item, int index) {
            _items.Insert(index, item);
            _itemsUnshuffled.Insert(index, item);
            OnItemsUpdated();
        }


        /// <summary>
        /// Enqueue a number of items at the end of the queue
        /// </summary>
        /// <param name="items">The items to add</param>
        public void Enqueue(IEnumerable<IItem> items) {
            _items.AddRange(items);
            _itemsUnshuffled.AddRange(items);
            OnItemsUpdated();
        }


        /// <summary>
        /// Enqueue a number of items at a specific index
        /// </summary>
        /// <param name="item">The item sto add</param>
        /// <param name="index">The index to add the items at</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0 -or- index is greater than Count.</exception>
        public void Enqueue(IEnumerable<IItem> items, int index) {
            _items.InsertRange(index, items);
            _itemsUnshuffled.InsertRange(index, items);
            OnItemsUpdated();
        }


        /// <summary>
        /// Moves the specified item in the queue to the specified index
        /// </summary>
        /// <param name="item">The item to move</param>
        /// <param name="index">The index to place the item at</param>
        /// <exception cref="ArgumentOutOfRangeException">item does not exist in the List</exception>
        /// <exception cref="ArgumentOutOfRangeException">to is not a valid index in the List</exception>
        public void MoveQueued(IItem item, int index) {
            this.MoveQueued(_items.IndexOf(item), index);
        }


        /// <summary>
        /// Moves the item at the specied index in the queue to the target index.
        /// Only items that are after the current index can be moved to after the current index.
        /// </summary>
        /// <param name="from">The index of the item to move</param>
        /// <param name="to">The index to place the item at</param>
        /// <exception cref="ArgumentOutOfRangeException">from is not a valid index in the List</exception>
        /// <exception cref="ArgumentOutOfRangeException">to is not a valid index in the List</exception>
        public void MoveQueued(int from, int to) {
            if (from <= Index || to <= Index) return;

            _items.Insert(to, _items[from]);
            _items.RemoveAt(from);

            if (!Shuffle) {
                _itemsUnshuffled.Insert(to, _itemsUnshuffled[from]);
                _itemsUnshuffled.RemoveAt(from);
            }

            OnItemsUpdated();
        }


        /// <summary>
        /// Called when items are updated: firest the event
        /// </summary>
        void OnItemsUpdated() {
            if (ItemsUpdated != null)
                ItemsUpdated.Invoke(this);
        }

        
        #endregion


        #region Queue control


        /// <summary>
        /// Go to the previous item
        /// </summary>
        /// <returns>The new current item, or null if none</returns>
        public IItem GoPrev() {
            --Index;
            return Current;
        }


        /// <summary>
        /// Move to the next item
        /// </summary>
        /// <returns>The new current item, or null if none</returns>
        public IItem GoNext() {
            return this.GoNext(false);
        }


        /// <summary>
        /// Move to the next item
        /// </summary>
        /// <param name="ignoreRepeat">Whether the repeat property should be ignored when determining the nex item</param>
        /// <returns>The new current item, or null if none</returns>
        public IItem GoNext(bool ignoreRepeat) {

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

            // If we are at the end
            if (nextIndex == _items.Count)
                nextIndex = -1;

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


        #region Helpers

        void ResetItems() {
            Index = _itemsUnshuffled.IndexOf(Current);
            _items = new List<IItem>(_itemsUnshuffled);
        }

        #endregion


    }



    public delegate void QueueItemsUpdatedEventHandler(Queue queue);
    public delegate void QueueIndexChangedEventHandler(Queue queue, IItem previous, IItem current);
    public delegate void QueueFinishedEventHandler(Queue queue);

}