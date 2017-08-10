using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using conductor.common.metadata.tasks;
using Newtonsoft.Json;
using Tavis.UriTemplates;
using Task = System.Threading.Tasks.Task;

namespace evs.conductor.client.http
{
  public class ClientBase
  {
    private Uri root;

    public ClientBase(Uri root)
    {
      this.root = root;
    }

    protected T GetForEntity<T>(string template, Dictionary<string, object> queryParams)
    {
      try
      {
        var url = new Uri(root,new UriTemplate(template).AddParameters(queryParams).Resolve());
        return GetAsyncImpl<T>(url.AbsoluteUri).Result;
      }
      catch (Exception e)
      {
        handleException(e);
      }
      return default(T);
    }

    protected void PostForEntity<T>(string template, T request)
    {
      try
      {
        var url = new Uri(root, new UriTemplate(template).Resolve());
        PostAsyncImpl(url, request);
      }
      catch (Exception e)
      {
        handleException(e);
      }
    }

    protected U PostForEntity<T, U>(string template, Dictionary<string, object> queryParams, T request)
    {
      try
      {
        var url = new Uri(root, new UriTemplate(template).AddParameters(queryParams).Resolve());
        return PostAsyncImpl<T, U>(url, request).Result;
      }
      catch (Exception e)
      {
        handleException(e);
      }
      return default(U);
    }

    private void handleException(Exception exception)
    {
      throw exception;
    }

    private static async void PostAsyncImpl<T>(Uri url, T request)
    {
      var content = JsonConvert.SerializeObject(request);
      await PostAsyncRaw(url.AbsoluteUri, content);
    }


    private static async Task<U> PostAsyncImpl<T, U>(Uri url, T request)
    {
      var content = JsonConvert.SerializeObject(request);
      var result = await PostAsyncRaw(url.AbsoluteUri, content);
      return JsonConvert.DeserializeObject<U>(result);
    }

    private static async Task<string> PostAsyncRaw(string url, string content)
    {
      using (var client = new HttpClient())
      {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
      }
    }

    static async Task<T> GetAsyncImpl<T>(string url)
    {
      var result = default(T);
      var response = await GetRawAsync(url);
      if (!string.IsNullOrWhiteSpace(response))
      {
        result = JsonConvert.DeserializeObject<T>(response);
      }
      return result;
    }

    public static async Task<string> GetRawAsync(string url)
    {
      using (var client = new HttpClient())
      {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return await client.GetStringAsync(url);
      }
    }
  }
}