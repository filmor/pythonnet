using System;
using System.Collections.Generic;
using System.Collections;

namespace Python.Runtime.CollectionWrappers
{
    internal class IterableWrapper<T> : IEnumerable<T>
    {
        protected readonly PyObject pyObject;

        public IterableWrapper(PyObject pyObj)
        {
            if (pyObj == null)
                throw new ArgumentNullException();
            pyObject = new PyObject(pyObj.Reference);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            PyIter iterObject;
            using (new Py.GILState())
            {
                iterObject = PyIter.GetIter(pyObject);
            }

            using var _ = iterObject;
            while (true)
            {
                using var _g = new Py.GILState();

                if (!iterObject.MoveNext())
                {
                    iterObject.Dispose();
                    break;
                }
                yield return iterObject.Current.As<T>()!;
            }
        }
    }
}
