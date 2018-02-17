using System;
using McMaster.Extensions.CommandLineUtils;

namespace HabiticaTaskBuilder
{
  class Program
  {
    static void Main(string[] args)
    {
      var app = new CommandLineApplication(false) ;
      app.FullName = "Habitica Task Builder";
      app.Description = "Command-line tool to build Habitica tasks for a specific user.";
      app.HelpOption("-? | -h | --help");
      app.VersionOptionFromAssemblyAttributes(typeof(Program).Assembly);

      // Arguments
      var source = app.Argument("source", "The source JSON file with the Tasks to Build");
      var apiuser = app.Argument("apiuser", "The API User to the Habitica API");
      var apikey = app.Argument("apikey", "The API Key to the Habitica API");

      app.OnExecute(() =>
      {
        app.ShowVersion();

        if (source.Value == null || apikey.Value == null || apiuser.Value == null)
        {
          app.ShowHelp();
          return 1;
        }

        new TaskBuilderOperation(source.Value, apiuser.Value, apikey.Value)
          .RunAsync()
          .Wait();

        return 0;
      });


      app.Execute(args);
    }
  }
}
