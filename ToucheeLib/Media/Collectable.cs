﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Touchee {

    public enum ItemChangeTypes {
        Saved,
        Created,
        Updated,
        Deleted
    }

    /// <remarks>
    /// 
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public abstract class Collectable<T> : Base, IDisposable {


        #region ActiveRecord statics


        /// <summary>
        /// The storage 'table' for the base type
        /// </summary>
        static Dictionary<int, object> _table = new Dictionary<int, object>();


        /// <summary>
        /// Returns the element with the given ID of the base type
        /// </summary>
        /// <param name="id">The ID to look for</param>
        /// <returns>The corresponding element</returns>
        public static T Find(int id) {
            return (T)_table[id];
        }


        /// <summary>
        /// Returns the element with the given ID of the given type, which is a derived from the base
        /// </summary>
        /// <typeparam name="R">The type of the items to search</typeparam>
        /// <param name="id">The ID to look for</param>
        /// <returns>The corresponding element</returns>
        public static R Find<R>(int id) where R : T {
            var item = _table[id];
            R ret = default(R);
            if (item is R)
                ret = (R)item;
            else
                new System.Collections.Generic.KeyNotFoundException();
            return ret;
        }



        /// <summary>
        /// Gets all elements of the base type
        /// </summary>
        /// <returns>All elements of the base type</returns>
        public static IEnumerable<T> All() {
            return _table.Values.Cast<T>();
        }


        /// <summary>
        /// Returns all elements of the given type
        /// </summary>
        /// <typeparam name="R">Then type of elements to look for</typeparam>
        /// <returns>All elements of the given type</returns>
        public static IEnumerable<R> All<R>() where R : T {
            return _table.Values.Where(v => v is R).Cast<R>();
        }


        /// <summary>
        /// Returns all stored instances of the base type that match the given predicate
        /// </summary>
        /// <param name="pred">The predicate to apply to each instance</param>
        /// <returns>The stored instances that match he predicate</returns>
        public static IEnumerable<T> Where(Func<T, bool> pred) {
            return All().Where(pred);
        }


        /// <summary>
        /// Returns all stored instances of the given type that match the given predicate
        /// </summary>
        /// <typeparam name="R">The type to match</typeparam>
        /// <param name="pred">The predicate to apply to each instance</param>
        /// <returns>The stored instances that match he predicate</returns>
        public static IEnumerable<R> Where<R>(Func<R, bool> pred) where R : T {
            return All<R>().Where(pred);
        }


        /// <summary>
        /// Returns the first stored instances of the base type that match the given predicate
        /// </summary>
        /// <param name="pred">The predicate to match</param>
        /// <returns>The first stored instancs that match he predicate, or the default value if none</returns>
        public static T FirstOrDefault(Func<T, bool> pred) {
            return All().FirstOrDefault(pred);
        }


        /// <summary>
        /// Returns the first stored instances of the given type that match the given predicate
        /// </summary>
        /// <typeparam name="R">THe type to match</typeparam>
        /// <param name="pred">The predicate to match</param>
        /// <returns>The first stored instance that match he predicate, or the default value if none</returns>
        public static R FirstOrDefault<R>(Func<R, bool> pred) where R : T {
            return All<R>().FirstOrDefault(pred);
        }


        /// <summary>
        /// Checks whether any stored instance of the base type matches the predicate
        /// </summary>
        /// <param name="pred">The predicate to match</param>
        /// <returns>true if at least one stored instance matches the predicate, otherwise false</returns>
        public static bool Any(Func<T, bool> pred) {
            return All().Any(pred);
        }


        /// <summary>
        /// Checks whether any stored instance of the base type matches the predicate
        /// </summary>
        /// <typeparam name="R">The type to match</typeparam>
        /// <param name="pred">The predicate to match</param>
        /// <returns>true if at least one stored instance matches the predicate, otherwise false</returns>
        public static bool Any<R>(Func<R, bool> pred) where R : T {
            return All<R>().Any(pred);
        }


        /// <summary>
        /// Checks whether a stored instance of the base type with the given ID exists
        /// </summary>
        /// <param name="id">The ID to check</param>
        /// <returns>true if the instance exists, otherwise false</returns>
        public static bool Exists(int id) {
            return _table.ContainsKey(id);
        }


        /// <summary>
        /// Executes the given action on all stored instances of the base type
        /// </summary>
        /// <param name="action">The action to execute</param>
        public static void ForEach(Action<T> action) {
            All().ToList().ForEach(action);
        }


        /// <summary>
        /// Executes the given action on all stored instances of the given type
        /// </summary>
        /// <typeparam name="R">The type to match</typeparam>
        /// <param name="action">The action to execute</param>
        public static void ForEach<R>(Action<R> action) where R : T {
            All<R>().ToList().ForEach(action);
        }


        /// <summary>
        /// Clear the collection
        /// </summary>
        public static void Clear() {
            lock(_table) {
                foreach (var item in All().Cast<Collectable<T>>().ToList())
                    item.Dispose();

                _table.Clear();
                _sourceTable.Clear();
                _nextId = 1;
            }
        }


        #endregion



        #region Source ID

        /// <summary>
        /// The storage for the source collection mappings
        /// </summary>
        protected static Dictionary<object, object> _sourceTable = new Dictionary<object, object>();


        /// <summary>
        /// The source ID
        /// </summary>
        public virtual object SourceId {
            get {
                return Id;
            }
            protected set { }
        }


        /// <summary>
        /// Gets a stored instance of the base type by the source ID
        /// </summary>
        /// <param name="id">The ID to search for</param>
        /// <returns>The instance with the corresponding source ID</returns>
        public static T FindBySourceID(object id) {
            return (T)_sourceTable[id];
        }


        /// <summary>
        /// Gets a stored instance of the given type by the source ID
        /// </summary>
        /// <param name="id">The ID to search for</param>
        /// <returns>The instance with the corresponding source ID</returns>
        public static R FindBySourceID<R>(object id) where R : T {
            var item = _sourceTable[id];
            R ret = default(R);
            if (item is R)
                ret = (R)item;
            else
                new System.Collections.Generic.KeyNotFoundException();
            return ret;
        }


        /// <summary>
        /// Checks whether a stored instance of the base type with the given source ID exists
        /// </summary>
        /// <param name="id">The source ID to check</param>
        /// <returns>true if the instance exists, otherwise false</returns>
        public static bool ExistsBySourceID(object id) {
            return _sourceTable.ContainsKey(id);
        }


        /// <summary>
        /// Checks whether a stored instance of the given type with the given source ID exists
        /// </summary>
        /// <typeparam name="R">The type to match</typeparam>
        /// <param name="id">The ID to check</param>
        /// <returns>true if the instance exists, otherwise false</returns>
        public static bool ExistsBySourceID<R>(object id) where R : T {
            return ExistsBySourceID(id) && FindBySourceID(id) is R;
        }

        #endregion



        #region Events

        public delegate void ItemEventHandler(object sender, ItemEventArgs e);
        public class ItemEventArgs : EventArgs {
            public ItemChangeTypes ChangeType { get; protected set; }
            public T Item { get; protected set; }
            public ItemEventArgs(ItemChangeTypes changeType, object item) {
                this.ChangeType = changeType;
                this.Item = (T)item;
            }
        }
        public static event ItemEventHandler BeforeSave;
        public static event ItemEventHandler BeforeCreate;
        public static event ItemEventHandler BeforeUpdate;
        public static event ItemEventHandler BeforeDispose;
        public static event ItemEventHandler AfterSave;
        public static event ItemEventHandler AfterCreate;
        public static event ItemEventHandler AfterUpdate;
        public static event ItemEventHandler AfterDispose;

        #endregion



        #region Create, Update, Destroy

        /// <summary>
        /// The ID of this object
        /// </summary>
        [DataMember]
        public int Id { get; private set; }
        static int _nextId = 1;

        /// <summary>
        /// Whether this object has not been stored yet
        /// </summary>
        public bool IsNew { get { return this.Id == 0; } }

        /// <summary>
        /// Stores the instance in the collection of the derived type, giving it an ID
        /// </summary>
        public virtual void Save() {
            bool isNew = this.IsNew;

            // Do save and create or update before callbacks
            if (Collectable<T>.BeforeSave != null)
                Collectable<T>.BeforeSave.Invoke(this, new ItemEventArgs(ItemChangeTypes.Saved, this));
            if (isNew) {
                if (Collectable<T>.BeforeCreate != null && Id > 0)
                    Collectable<T>.BeforeCreate.Invoke(this, new ItemEventArgs(ItemChangeTypes.Created, this));
            }
            else {
                if (Collectable<T>.BeforeUpdate != null)
                    Collectable<T>.BeforeUpdate.Invoke(this, new ItemEventArgs(ItemChangeTypes.Updated, this));
            }

            // Set ID and store in tables
            if (isNew) {
                Id = _nextId++;
                _table[Id] = this;
                _sourceTable[SourceId] = this;
            }
            
            // Do create or update and save after callbacks
            if (isNew) {
                if (Collectable<T>.AfterCreate != null && Id > 0)
                    Collectable<T>.AfterCreate.Invoke(this, new ItemEventArgs(ItemChangeTypes.Created, this));
            }
            else {
                if (Collectable<T>.AfterUpdate != null && Id > 0)
                    Collectable<T>.AfterUpdate.Invoke(this, new ItemEventArgs(ItemChangeTypes.Updated, this));
            }
            if (Collectable<T>.AfterSave != null)
                Collectable<T>.AfterSave.Invoke(this, new ItemEventArgs(ItemChangeTypes.Saved, this));

        }


        /// <summary>
        /// Whether this instance has been disposed
        /// </summary>
        public bool IsDisposed { get; private set; }


        /// <summary>
        /// Disposes of the instance, informing any listeners if appropriate
        /// </summary>
        public virtual void Dispose() {
            if (!this.IsDisposed) {
                if (Collectable<T>.BeforeDispose != null && Id > 0)
                    Collectable<T>.BeforeDispose.Invoke(this, new ItemEventArgs(ItemChangeTypes.Deleted, this));
                if (_table.ContainsKey(this.Id))
                    _table.Remove(this.Id);
                if (_sourceTable.ContainsKey(this.SourceId))
                    _sourceTable.Remove(this.SourceId);
                this.IsDisposed = true;
                this.OnDispose();
                if (Collectable<T>.AfterDispose != null && Id > 0)
                    Collectable<T>.AfterDispose.Invoke(this, new ItemEventArgs(ItemChangeTypes.Deleted, this));
            }
        }


        /// <summary>
        /// Called when the object is being disposed
        /// </summary>
        public virtual void OnDispose() {
        }

        #endregion


    }

}
