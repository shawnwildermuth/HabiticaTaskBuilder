using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace HabiticaTaskBuilder
{
  internal class TaskBuilderOperation
  {
    private readonly string _source;
    private readonly string _apiuser;
    private readonly string _apikey;
    private readonly HttpClient _client;

    public TaskBuilderOperation(string source, string apiuser, string apikey)
    {
      this._source = source;
      _apiuser = apiuser;
      this._apikey = apikey;

      _client = new HttpClient();
      _client.DefaultRequestHeaders.Add("x-api-user", _apiuser);
      _client.DefaultRequestHeaders.Add("x-api-key", _apikey);
    }

    public async Task RunAsync()
    {
      var json = await File.ReadAllTextAsync(_source);
      var tasks = JsonConvert.DeserializeObject<IEnumerable<HabiticaTask>>(json);

      var priorities = new Dictionary<string, string>();
      priorities["Trivial"] = "0.1";
      priorities["Easy"] = "1";
      priorities["Medium"] = "1.5";
      priorities["Hard"] = "2";


      var tags = await GetCreateTags(tasks.SelectMany(t => t.Tags).Distinct().ToList());

      var settings = new JsonSerializerSettings()
      {
        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
        Formatting = Formatting.Indented
      };

      foreach (var task in tasks)
      {
        var updatedTask = new
        {
          text = task.Text,
          date = task.Date,
          priority = priorities[task.Priority],
          type = task.Type,
          tags = task.Tags.Select(t => tags[t]).ToArray()
        };

        var taskJson = JsonConvert.SerializeObject(updatedTask, settings);

        var content = new StringContent(taskJson, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("https://habitica.com/api/v3/tasks/user", content);
        if (response.IsSuccessStatusCode)
        {
          Console.WriteLine("Created Task");
        }
        else
        {
          Console.WriteLine("Failed to Create Task");
        }
      }

    }

    private async Task<Dictionary<string, string>> GetCreateTags(List<string> list)
    {
      var results = new Dictionary<string, string>();

      var response = await _client.GetAsync("https://habitica.com/api/v3/tags");

      if (response.IsSuccessStatusCode)
      {
        var json = await response.Content.ReadAsStringAsync();
        var existingTags = JObject.Parse(json);
        var data = (JArray)existingTags["data"];
        foreach (var d in data)
        {
          string name = d["name"].ToString();
          if (list.IndexOf(name) > -1)
          {
            results[name] = d["id"].ToString();
            list.Remove(name);
          }
        }

        if (list.Any())
        {
          foreach (var tagName in list)
          {
            var request = new StringContent($@"{{ ""name"": ""{tagName}"" }}", Encoding.UTF8, "application/json");
            var tagResponse = await _client.PostAsync("https://habitica.com/api/v3/tags", request);
            if (tagResponse.IsSuccessStatusCode)
            {
              var tagJson = await tagResponse.Content.ReadAsStringAsync();
              var newTag = JObject.Parse(tagJson);
              results[tagName] = newTag["data"]["id"].ToString();
            }
          }
        }

      }
      else
      {
        Console.WriteLine("Failed to get Tags");
        throw new InvalidProgramException("Failed to get tags");
      }

      return results;

    }
  }
}