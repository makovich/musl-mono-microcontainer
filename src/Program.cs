namespace App
{
  using System;
  using Microsoft.Owin.Hosting;
  using Mono.Unix;
  using Mono.Unix.Native;
  using Owin;

  public static class Host
  {
    public static int Main(string[] args)
    {
      var server = WebApp.Start($"http://+:5000", appBuilder => {
        appBuilder.Run(ctx => {
          ctx.Response.StatusCode = 200;
          return ctx.Response.WriteAsync("<h1>٩(｡•́‿•̀｡)۶</h1>");
        });
      });

      Console.WriteLine("Listening http://0.0.0.0:5000/");
      Console.WriteLine("Press Ctrl+C, send INT or TERM signal to stop.");

      Console.Error.WriteLine("Hello from STDERR.");
      
      UnixSignal.WaitAny(new []
      {
          new UnixSignal(Signum.SIGINT),
          new UnixSignal(Signum.SIGTERM)
      }, -1);

      Console.WriteLine("Stopping...");

      server.Dispose();

      return 0;
    }
  }
}
