using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace HabiticaTaskBuilder
{
  class Program
  {
    static void Main(string[] args)
    {
      var app = new CommandLineApplication(false) ;
      app.FullName = "Habitica Task Builder";
      app.Description = "Command-line tool to build and delete Habitica tasks for a specific user.";

      var create = app.Command("create", config =>
      {

        config.Description = "Creates new tasks based on a JSON file";
        config.HelpOption("-? | -h | --help");

        var source = config.Argument("source", "The source JSON file with the Tasks to Build", c => c.IsRequired());
        var apiuser = config.Argument("apiuser", "The API User to the Habitica API", c => c.IsRequired());
        var apikey = config.Argument("apikey", "The API Key to the Habitica API", c => c.IsRequired());

        config.OnExecute(() =>
        {

          new TaskBuilderOperation(source.Value, apiuser.Value, apikey.Value)
            .RunAsync()
            .Wait();

          return 0;
        });
      });

      var del = app.Command("delete", config =>
      {
        config.Description = "Deletes tasks based on a tag name.";
        config.HelpOption("-? | -h | --help");

        var tag = config.Argument("tag", "name of the tag to delete", c=> c.IsRequired());
        var apiuser = config.Argument("apiuser", "The API User to the Habitica API", c => c.IsRequired());
        var apikey = config.Argument("apikey", "The API Key to the Habitica API", c => c.IsRequired());

        config.OnExecute(() =>
        {

          new TaskDeleteOperation(tag.Value, apiuser.Value, apikey.Value)
            .RunAsync()
            .Wait();

          return 0;
        });
      });

      
      app.HelpOption("-? | -h | --help");
      app.VersionOptionFromAssemblyAttributes(typeof(Program).Assembly);

      // Failover
      app.OnExecute(() =>
      {
        app.ShowHelp();
        return 1;
      });

      Environment.Exit(app.Execute(args));
    }
  }
}
