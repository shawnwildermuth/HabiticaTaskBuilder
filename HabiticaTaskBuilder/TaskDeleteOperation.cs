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
  internal class TaskDeleteOperation
  {
    private readonly string _tag;
    private readonly string _apiuser;
    private readonly string _apikey;
    private readonly HttpClient _client;

    public TaskDeleteOperation(string tag, string apiuser, string apikey)
    {
      this._tag = tag;
      _apiuser = apiuser;
      this._apikey = apikey;

      _client = new HttpClient();
      _client.DefaultRequestHeaders.Add("x-api-user", _apiuser);
      _client.DefaultRequestHeaders.Add("x-api-key", _apikey);
    }

    public async Task RunAsync()
    {
      //https://habitica.com/api/v3/tasks/:taskId
      //https://habitica.com/api/v3/tasks/user

      var results = await _client.GetAsync("https://habitica.com/api/v3/tasks/user?type=todos");
      if (results.IsSuccessStatusCode)
      {
        var tagResult = await _client.GetAsync("https://habitica.com/api/v3/tags");

        if (tagResult.IsSuccessStatusCode)
        {

          var foundTag = JObject.Parse(await tagResult.Content.ReadAsStringAsync())["data"].First(t => t["name"].ToString() == _tag);
          var tagId = foundTag["id"].ToString();

          var json = await results.Content.ReadAsStringAsync();
          var readJson = JObject.Parse(json);
          var coll = (JArray)readJson["data"];
          foreach (var item in coll)
          {
            if (item["tags"].Any(t => t.ToString() == tagId))
            {
              var delResult = await _client.DeleteAsync($"https://habitica.com/api/v3/tasks/{item["id"]}");
              if (delResult.IsSuccessStatusCode)
              {
                Console.WriteLine("Deleted Task");
              }
            }
          }
        }
      }
    }

    private async Task CreateSubTask(string subtask, string taskid)
    {
      var url = $"https://habitica.com/api/v3/tasks/{taskid}/checklist";
      await _client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(new { text = subtask }), Encoding.UTF8, "application/json"));
      Console.WriteLine($"  Subtask Created: {subtask}");
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