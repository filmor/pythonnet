using System;

namespace Python.Runtime
{
    /// <summary>
    /// Implements a Python event binding type, similar to a method binding.
    /// </summary>
    [Serializable]
    internal class EventBinding : ExtensionType
    {
        private EventObject e;
        private IntPtr target;

        public EventBinding(EventObject e, IntPtr target)
        {
            Runtime.XIncref(target);
            this.target = target;
            this.e = e;
        }


        /// <summary>
        /// EventBinding += operator implementation.
        /// </summary>
        public static IntPtr nb_inplace_add(IntPtr ob, IntPtr arg)
        {
            var self = (EventBinding)GetManagedObject(ob);

            if (Runtime.PyCallable_Check(arg) < 1)
            {
                Exceptions.SetError(Exceptions.TypeError, "event handlers must be callable");
                return IntPtr.Zero;
            }

            if (!self.e.AddEventHandler(self.target, arg))
            {
                return IntPtr.Zero;
            }

            Runtime.XIncref(self.pyHandle);
            return self.pyHandle;
        }


        /// <summary>
        /// EventBinding -= operator implementation.
        /// </summary>
        public static IntPtr nb_inplace_subtract(IntPtr ob, IntPtr arg)
        {
            var self = (EventBinding)GetManagedObject(ob);

            if (Runtime.PyCallable_Check(arg) < 1)
            {
                Exceptions.SetError(Exceptions.TypeError, "invalid event handler");
                return IntPtr.Zero;
            }

            if (!self.e.RemoveEventHandler(self.target, arg))
            {
                return IntPtr.Zero;
            }

            Runtime.XIncref(self.pyHandle);
            return self.pyHandle;
        }


        /// <summary>
        /// EventBinding  __hash__ implementation.
        /// </summary>
        public static nint tp_hash(IntPtr ob)
        {
            var self = (EventBinding)GetManagedObject(ob);
            nint x = 0;

            if (self.target != IntPtr.Zero)
            {
                x = Runtime.PyObject_Hash(self.target);
                if (x == -1)
                {
                    return x;
                }
            }

            nint y = Runtime.PyObject_Hash(self.e.pyHandle);
            if (y == -1)
            {
                return y;
            }

            return x ^ y;
        }


        /// <summary>
        /// EventBinding __repr__ implementation.
        /// </summary>
        public static IntPtr tp_repr(IntPtr ob)
        {
            var self = (EventBinding)GetManagedObject(ob);
            string type = self.target == IntPtr.Zero ? "unbound" : "bound";
            string s = string.Format("<{0} event '{1}'>", type, self.e.name);
            return Runtime.PyString_FromString(s);
        }

        protected override void Clear()
        {
            Runtime.Py_CLEAR(ref this.target);
            base.Clear();
        }
    }
}
