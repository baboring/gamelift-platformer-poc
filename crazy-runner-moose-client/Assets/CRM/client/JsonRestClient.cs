
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections.Generic;


public class JsonRestClient {

  private static readonly Tuple<string, string>[] NO_HEADERS = {};
  private readonly MonoBehaviour parent;
  public JsonRestClient(MonoBehaviour parent) {
    this.parent = parent;
  }
  
  public Task<T> Get<T>(string url, Tuple<string, string>[] headers = null) {
    var taskSource = new TaskCompletionSource<T>();
    parent.StartCoroutine(this.Get(taskSource, url, headers));
    return taskSource.Task;
  }

  public Task<T> Post<T, V>(string url, V body, Tuple<string, string>[] headers = null) {
    var taskSource = new TaskCompletionSource<T>();
    parent.StartCoroutine(this.Post(taskSource, url, body, headers));
    return taskSource.Task;
  }

  public Task<bool> Delete(string url, Tuple<string, string>[] headers = null) {
    var taskSource = new TaskCompletionSource<bool>();
    parent.StartCoroutine(this.Delete(taskSource, url, headers));
    return taskSource.Task;
  }

  private IEnumerator<YieldInstruction> Get<T>(TaskCompletionSource<T> taskSource, string url, Tuple<string, string>[] headers = null) {
    var request = UnityWebRequest.Get(url);
    foreach(var header in headers ?? NO_HEADERS){
      request.SetRequestHeader(header.Item1, header.Item2);
    };
    request.SetRequestHeader("Accept", "application/json");
    yield return request.SendWebRequest();
    if (request.isNetworkError || request.isHttpError) {
      taskSource.SetException(new Exception("GET at '" + url + "' Failed", new Exception(request.error)));
    } else {
      taskSource.SetResult(JsonUtility.FromJson<T>(request.downloadHandler.text));
    }
  }

  private IEnumerator<YieldInstruction> Post<T, V>(TaskCompletionSource<T> taskSource, string url, V body, Tuple<string, string>[] headers = null) {
    var postBody = JsonUtility.ToJson(body);
    var request = UnityWebRequest.Put(url, postBody); //avoid URL encoding sillyness, seriously unity, why?
    request.method = "POST";
    foreach(var header in headers ?? NO_HEADERS){
      request.SetRequestHeader(header.Item1, header.Item2);
    };
    request.SetRequestHeader("Accept", "application/json");
    request.SetRequestHeader("Content-Type", "application/json");
    yield return request.SendWebRequest();
    if (request.isNetworkError || request.isHttpError) {
      taskSource.SetException(new Exception("POST at '" + url + "' with '" + postBody + "' Failed", new Exception(request.error)));
    } else {
      taskSource.SetResult(JsonUtility.FromJson<T>(request.downloadHandler.text));
    }
  }

  private IEnumerator<YieldInstruction> Delete(TaskCompletionSource<bool> taskSource, string url, Tuple<string, string>[] headers = null) {
    var request = UnityWebRequest.Delete(url);
    foreach(var header in headers ?? NO_HEADERS){
      request.SetRequestHeader(header.Item1, header.Item2);
    };
    request.SetRequestHeader("Accept", "application/json");
    yield return request.SendWebRequest();
    if (request.isNetworkError || request.isHttpError) {
      taskSource.SetException(new Exception("DELETE at '" + url + "' Failed", new Exception(request.error)));
    } else {
      taskSource.SetResult(true);
    }
  }

}

