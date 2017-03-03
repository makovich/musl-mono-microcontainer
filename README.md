# musl-mono-microcontainer [![](https://images.microbadger.com/badges/image/makovich/musl-mono-microcontainer.svg)](https://microbadger.com/images/makovich/musl-mono-microcontainer)

This is a simple OWIN web application that uses `FROM scratch` in its `Dockerfile`. The application packaging is an example of The Build Container pattern where container's build procedure is represented as a Two-Phase process:

1. Prepare binary components from which application consists of ([`Dockerfile.build`](Dockerfile.build))
2. Build the runtime image by using previous step output ([`Dockerfile`](Dockerfile))

Run `build-image.sh` and you'll see the described procees in a console.

## Notes

### The first phase base image

[`alpine-mono-sdk`](https://github.com/makovich/alpine-mono-sdk) is a base image that produces application binaries. They are packed in a single file `artifacts/runtime.tar` which represents the root filesystem in a runtime image.

This image in turn based on Alpine Linux one and have all needed dependencies to compile .NET application including Mono.

Mono is compiled from [sources](https://github.com/makovich/alpine-mono-sdk/blob/master/Dockerfile#L31) against `musl` libc library. It also includes `mkbundle` [patch](https://github.com/makovich/alpine-mono-sdk/blob/master/01-mono-mkbundle-assemblies-lookup.patch).

### The application Build script

`build.sh`'s role is to restore dependencies and run [FAKE](http://fsharp.github.io/FAKE/) script. Unfortunately, FAKE can not instantiate F# interpreter host, so temporary workaround for now just to feed `fsi.exe` directly with `build.fsx`. F# interpreter comes from `FSharp.Compiler.Tools` package (I was unable to build F# from sources on Alpine image; perhaps 80K `musl`'s stack size is a showstopper).

### Important build steps

[`MakeAppBundle`](https://github.com/makovich/musl-mono-microcontainer/blob/master/build.fsx#L42) creates a single executable file which is under certain circumstances is the only file needed to run the application.

I intentionally introduced `Mono.Posix` dependency to show how to deal with native dependencies.

> It is possible to statically link `glibc` and `libgcc` &mdash; runtime dependencies &mdash; with bundle by playing with `mkbundle` and `gcc` command line arguments. It is important to note, that linking should be made with `-Wl,-Bdynamic` anyway to allow CLR load assemblies even from bundle itself).

[`PackRuntime`](https://github.com/makovich/musl-mono-microcontainer/blob/master/build.fsx#L65) archives application bundle with native Linux dependencies. As you can see, it includes Mono runtime deps, as well as `Mono.Posix` specific (`libMonoPosixHelper.so` and `libz`). `machine.config` could be included in bundle itself but to manage, for instance, `machineKey` it is better to leave it outside of executable.

### Docker Hub/Cloud hooks

Under the [`hooks`](hooks) directory you could find scripts that help to leverage Two-Phase build within Docker infrastructure.

## Cudos and futher reading

- [A Nancy .NET Microservice Running On Docker In Under 20mb](http://www.onegeek.com.au/articles/a-nancy-net-microservice-running-on-docker-in-under-20mb)
- [Tiny Docker images with musl libc and no package manager](http://mwcampbell.us/blog/tiny-docker-musl-images.html)
- [Docker Pattern: The Build Container](http://blog.terranillius.com/post/docker_builder_pattern/)
