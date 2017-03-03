#!/bin/sh

if command -v paket;
then

  paket restore
  exit_code=$?
  if test $exit_code -ne 0; then
    exit $exit_code
  fi

else

  mono .paket/paket.bootstrapper.exe
  exit_code=$?
  if test $exit_code -ne 0; then
    exit $exit_code
  fi

  mono .paket/paket.exe restore
  exit_code=$?
  if test $exit_code -ne 0; then
    exit $exit_code
  fi

fi

# FAKE.exe is not working (F# interpreter hosting code - Yaaf.FSharp.Scripting - throws):
#
# /src # mono packages/tools/FAKE/tools/FAKE.exe build.fsx
# FsiEvaluationSession could not be created.
#
#
# Build failed.
# Error:
# Error while compiling or executing fsharp snippet.
#
#
# Using fsi.exe though:
#
mono packages/tools/FSharp.Compiler.Tools/tools/fsi.exe --exec build.fsx
