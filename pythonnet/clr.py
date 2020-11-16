"""
Legacy Python.NET loader for backwards compatibility
"""

def _get_netfx_path():
    import os, sys

    if sys.maxsize > 2 ** 32:
        arch = "amd64"
    else:
        arch = "x86"

    return os.path.join(os.path.dirname(__file__), "netfx", arch, "clr.pyd")


def _get_mono_path():
    import os

    return os.path.join(os.path.dirname(__file__), "mono", "clr.so")


def _load_clr():
    import sys
    if sys.platform == "win32":
        path = _get_netfx_path()
    else:
        path = _get_mono_path()

    del sys.modules["clr"]

    spec = util.spec_from_file_location("clr", path)
    clr = util.module_from_spec(spec)
    spec.loader.exec_module(clr)

    sys.modules["clr"] = clr


_load_clr()
