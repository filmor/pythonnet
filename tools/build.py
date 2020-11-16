import sys, os
import os
import shutil
from subprocess import check_output, check_call
from distutils.command.build_ext import build_ext
from distutils.core import Distribution, Extension
from dataclasses import dataclass

from logging import getLogger

logger = getLogger("pythonnet-build")


class DotnetLib:
    def __init__(self, name, path, *, runtime, output, rename=None):
        self.name = name
        self.path = path
        self.runtime = runtime
        self.output = output
        self.rename = rename

    def build(self):
        rename = self.rename or {}
        output = self.output

        opts = []
        opts.extend(["--runtime", self.runtime])
        # opts.extend(["--configuration", self.dotnet_config])
        opts.extend(["--output", output])

        logger.info("Running dotnet build...")
        check_call(["dotnet", "build", lib.path] + opts)

        for k, v in rename.items():
            source = os.path.join(output, k)
            dest = os.path.join(output, v)

            if os.path.isfile(source):
                try:
                    os.remove(dest)
                except OSError:
                    pass

                shutil.move(src=source, dst=dest)
            else:
                logger.warn(
                    "Can't find file to rename: %s, current dir: %s",
                    source, os.getcwd()
                )

libs = [
    DotnetLib(
        "clrmodule-amd64",
        "src/clrmodule/clrmodule.csproj",
        runtime="win-x64",
        output="pythonnet/netfx/amd64",
        rename={"clr.dll": "clr.pyd"},
    ),
    DotnetLib(
        "clrmodule-x86",
        "src/clrmodule/clrmodule.csproj",
        runtime="win-x86",
        output="pythonnet/netfx/x86",
        rename={"clr.dll": "clr.pyd"},
    ),
]


def build_monoclr():
    try:
        mono_libs = check_output("pkg-config --libs mono-2", shell=True, encoding="utf8")
        mono_cflags = check_output(
            "pkg-config --cflags mono-2", shell=True, encoding="utf8"
        )
        cflags = mono_cflags.strip()
        libs = mono_libs.strip()

        # build the clr python module
        clr_ext = Extension(
            "clr",
            language="c++",
            sources=["src/monoclr/pynetinit.c", "src/monoclr/clrmod.c"],
            extra_compile_args=cflags.split(" "),
            extra_link_args=libs.split(" "),
        )

        distribution = Distribution({"name": "clr", "ext_modules": [clr_ext]})
        distribution.package_dir = "pythonnet"

        cmd = build_ext(distribution)
        cmd.ensure_finalized()
        cmd.run()

        # Copy built extensions back to the project
        for output in cmd.get_outputs():
            os.makedirs("pythonnet/mono")
            shutil.copyfile(output, "pythonnet/mono")

    except Exception:
        logger.info(
            "Failed to find mono libraries via pkg-config, "
            "skipping the Mono CLR loader"
        )
        # If no mono loader is built, the created package is pure


if __name__ == "__main__":
    build_monoclr()

    check_call([sys.executable, "tools/configure.py"])

    for lib in libs:
        lib.build()
