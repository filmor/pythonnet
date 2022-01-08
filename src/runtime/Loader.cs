using System;
using System.Text;

namespace Python.Runtime
{
    using static Runtime;

    [Obsolete("Only to be used from within Python")]
    static class Loader
    {
        public unsafe static int Initialize(IntPtr data, int size)
        {
            try
            {
                var dllPath = Encoding.UTF8.GetString((byte*)data.ToPointer(), size);

                if (!string.IsNullOrEmpty(dllPath))
                {
                    PythonDLL = dllPath;
                }
                else
                {
                    PythonDLL = null;
                }

                using var _ = new Py.GILState();
                PythonEngine.InitExt();
            }
            catch (Exception exc)
            {
                Console.Error.Write(
                    $"Failed to initialize pythonnet: {exc}\n{exc.StackTrace}"
                );
                return 1;
            }
            
            return 0;
        }

        public unsafe static int Shutdown(IntPtr data, int size)
        {
            try
            {
                var command = Encoding.UTF8.GetString((byte*)data.ToPointer(), size);

                if (command == "full_shutdown")
                {
                    using var _ = new Py.GILState();
                    PythonEngine.Shutdown();
                }
            }
            catch (Exception exc)
            {
                Console.Error.Write(
                    $"Failed to shutdown pythonnet: {exc}\n{exc.StackTrace}"
                );
                return 1;
            }

            return 0;
        }
    }
}
