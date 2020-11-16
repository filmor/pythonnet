import sys, os
from subprocess import check_call

PY_MAJOR = sys.version_info[0]
PY_MINOR = sys.version_info[1]

CONFIGURED_PROPS = "configured.props"


def _get_interop_filename():
    interop_filename = "interop{0}{1}{2}.cs".format(
        PY_MAJOR, PY_MINOR, getattr(sys, "abiflags", "")
    )
    return os.path.join("src", "runtime", interop_filename)


def _write_configure_props(root: str, output: str):
    # Up to Python 3.2 sys.maxunicode is used to determine the size of
    # Py_UNICODE, but from 3.3 onwards Py_UNICODE is a typedef of wchar_t.
    import ctypes

    unicode_width = ctypes.sizeof(ctypes.c_wchar)

    defines = [
        "PYTHON{0}{1}".format(PY_MAJOR, PY_MINOR),
        "UCS{0}".format(unicode_width),
    ]

    if sys.platform == "win32":
        defines.append("WINDOWS")

    if hasattr(sys, "abiflags"):
        if "d" in sys.abiflags:
            defines.append("PYTHON_WITH_PYDEBUG")
        if "m" in sys.abiflags:
            defines.append("PYTHON_WITH_PYMALLOC")

    # check the interop file exists, and create it if it doesn't
    interop_filename = _get_interop_filename()
    interop_file = os.path.join(root, interop_filename)
    if not os.path.exists(interop_file):
        print("Creating {0}".format(interop_filename))
        geninterop = os.path.join(root, "tools", "geninterop", "geninterop.py")
        check_call([sys.executable, geninterop, interop_file])

    print("Writing {} with interop file {} and defines {}".format(
        os.path.basename(output), interop_filename, defines
    ))

    import xml.etree.ElementTree as ET

    proj = ET.Element("Project")
    props = ET.SubElement(proj, "PropertyGroup")
    f = ET.SubElement(props, "PythonInteropFile")
    f.text = os.path.basename(interop_file)

    c = ET.SubElement(props, "ConfiguredConstants")
    c.text = " ".join(defines)

    ET.ElementTree(proj).write(output)


if __name__ == "__main__":
    root = os.path.join(os.path.dirname(__file__), "..")

    configured_props = os.path.join(root, CONFIGURED_PROPS)
    _write_configure_props(root, configured_props)
