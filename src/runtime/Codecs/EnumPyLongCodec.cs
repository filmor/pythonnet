using System;

namespace Python.Runtime.Codecs
{
    [Obsolete]
    public sealed class EnumPyLongCodec : IPyObjectEncoder, IPyObjectDecoder
    {
        public static EnumPyLongCodec Instance { get; } = new EnumPyLongCodec();

        public bool CanDecode(PyObject objectType, Type targetType)
        {
            return targetType.IsEnum
                && objectType.IsSubclass(new BorrowedReference(Runtime.PyLongType));
        }

        public bool CanEncode(Type type)
        {
            return type == typeof(object) || type == typeof(ValueType) || type.IsEnum;
        }

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            value = default;
            if (!typeof(T).IsEnum) return false;

            Type etype = Enum.GetUnderlyingType(typeof(T));

            if (!PyLong.IsLongType(pyObj)) return false;

            object result;
            try
            {
                result = pyObj.AsManagedObject(etype);
            }
            catch (InvalidCastException)
            {
                return false;
            }

            if (Enum.IsDefined(typeof(T), result) || typeof(T).IsFlagsEnum())
            {
                value = (T)Enum.ToObject(typeof(T), result);
                return true;
            }

            return false;
        }

        public PyObject TryEncode(object value)
        {
            if (value is null) return null;

            var enumType = value.GetType();
            if (!enumType.IsEnum) return null;

            try
            {
                return new PyLong((long)value);
            }
            catch (InvalidCastException)
            {
                return new PyLong((ulong)value);
            }
        }

        private EnumPyLongCodec() { }
    }
}
