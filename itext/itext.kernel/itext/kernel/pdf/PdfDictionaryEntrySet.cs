using System;
using System.Collections;
using System.Collections.Generic;

namespace iText.Kernel.Pdf {
    internal class PdfDictionaryEntrySet : ICollection<KeyValuePair<PdfName, PdfObject>>
    {
        private readonly ICollection<KeyValuePair<PdfName, PdfObject>> collection;

        internal PdfDictionaryEntrySet(ICollection<KeyValuePair<PdfName, PdfObject>> collection) {
            this.collection = collection;
        }

        public IEnumerator<KeyValuePair<PdfName, PdfObject>> GetEnumerator()
        {
            return new DirectEnumerator(collection.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<PdfName, PdfObject> item)
        {
            collection.Add(item);
        }

        public void Clear()
        {
            collection.Clear();
        }

        public bool Contains(KeyValuePair<PdfName, PdfObject> item)
        {
            if (collection.Contains(item))
                return true;
            if (item.Value == null)
                return false;
            foreach (KeyValuePair<PdfName, PdfObject> entry in this)
                if (item.Key.Equals(entry.Key) && item.Value.Equals(entry.Value))
                    return true;
            return false;
        }

        public void CopyTo(KeyValuePair<PdfName, PdfObject>[] array, int arrayIndex)
        {
            int count = collection.Count;
            if (count == 0)
                return;
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", arrayIndex, "arrayIndex mut not be negtive");
            if (arrayIndex >= array.Length || count > array.Length - arrayIndex)
                throw new ArgumentException("arrayIndex", "array too small");

            int index = arrayIndex;
            foreach (KeyValuePair<PdfName, PdfObject> item in this)
            {
                if (--count < 0)
                    break;
                array[index++] = item;
            }
        }

        public bool Remove(KeyValuePair<PdfName, PdfObject> item)
        {
            if (collection.Remove(item))
                return true;
            if (item.Value == null)
                return false;
            KeyValuePair<PdfName, PdfObject> toDelete = new KeyValuePair<PdfName, PdfObject>(null, null);
            foreach (KeyValuePair<PdfName, PdfObject> entry in collection)
                if (item.Key.Equals(entry.Key) && PdfObject.EqualContent(item.Value, entry.Value))
                {
                    toDelete = entry;
                    break;
                }

            return toDelete.Key != null && collection.Remove(toDelete);
        }

        public int Count
        {
            get { return collection.Count; }
        }

        public bool IsReadOnly
        {
            get { return collection.IsReadOnly; }
        }

        private class DirectEnumerator : IEnumerator<KeyValuePair<PdfName, PdfObject>>
        {
            private IEnumerator<KeyValuePair<PdfName, PdfObject>> parentEnumerator;

            public DirectEnumerator(IEnumerator<KeyValuePair<PdfName, PdfObject>> parentEnumerator)
            {
                this.parentEnumerator = parentEnumerator;
            }

            public void Dispose()
            {
                parentEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                if (parentEnumerator.MoveNext())
                {
                    KeyValuePair<PdfName, PdfObject> current = parentEnumerator.Current;
                    if (current.Value != null && current.Value.IsIndirectReference())
                    {
                        current = new KeyValuePair<PdfName, PdfObject>(current.Key, ((PdfIndirectReference)current.Value).GetRefersTo(true));
                    }
                    Current = current;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                parentEnumerator.Reset();
            }

            public KeyValuePair<PdfName, PdfObject> Current { get; private set; }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}