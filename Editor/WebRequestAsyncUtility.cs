using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using System.Text;

public enum HTTPVerb { GET, POST };

public static class WebRequestAsyncUtility {

    public static class WebRequestAsync<T> {

        /// <summary>
        /// Performs a WebRequest with the given URL. The response is expected to be a JSON serialized
        /// object of type <typeparamref name="T"/> and is automatically being deserialized and returned.
        /// </summary>
        /// <typeparam name="T">the type that this web request returns.</typeparam>
        public static async Task<T> SendWebRequestAsync(string url, HTTPVerb verb = HTTPVerb.GET, string postData = null, params Tuple<string, string>[] requestHeaders) {
            UnityWebRequest wr = GetRequest(url, verb, postData, requestHeaders);
            if (wr == null) {
                return default;
            }

            try {
                var asyncOp = wr.SendWebRequest();
                while (!asyncOp.webRequest.isDone) {
                    await Task.Yield();
                }

                switch (asyncOp.webRequest.result) {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        return JsonConvert.DeserializeObject<T>(asyncOp.webRequest.downloadHandler.text);
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"{asyncOp.webRequest.result}: {asyncOp.webRequest.error}\nURL: {asyncOp.webRequest.url}");
                        Debug.LogError($"{asyncOp.webRequest.downloadHandler.text}");
                        break;
                    default:
                        break;
                }

            } catch (Exception e) {
                Debug.LogError(e);
            } finally {
                wr.Dispose();
            }
            return default;
        }
    }

    /// <summary>
    /// Simple WebRequest without JSON parsing. Returns plain text.
    /// </summary>
    public class WebRequestSimpleAsync {

        public static async Task<string> SendWebRequestAsync(string url, HTTPVerb verb = HTTPVerb.GET, string postData = null, params Tuple<string, string>[] requestHeaders) {
            UnityWebRequest wr = GetRequest(url, verb, postData, requestHeaders);
            if (wr == null) {
                return default;
            }

            try {
                var asyncOp = wr.SendWebRequest();
                while (!asyncOp.webRequest.isDone) {
                    await Task.Yield();
                }

                switch (asyncOp.webRequest.result) {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        return asyncOp.webRequest.downloadHandler.text;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"{asyncOp.webRequest.result}: {asyncOp.webRequest.error}\nURL: {asyncOp.webRequest.url}");
                        Debug.LogError($"{asyncOp.webRequest.downloadHandler.text}");
                        break;
                    default:
                        break;
                }

            } catch (Exception e) {
                Debug.LogError(e);
            } finally {
                wr.Dispose();
            }
            return default;
        }
    }

    /// <summary>
    /// WebRequest for binary data.
    /// </summary>
    public class WebRequestBinaryAsync {

        public static async Task<byte[]> SendWebRequestAsync(string url, HTTPVerb verb = HTTPVerb.GET, string postData = null, params Tuple<string, string>[] requestHeaders) {
            UnityWebRequest wr = GetRequest(url, verb, postData, requestHeaders);
            if (wr == null) {
                return default;
            }

            try {
                var asyncOp = wr.SendWebRequest();
                while (!asyncOp.webRequest.isDone) {
                    await Task.Yield();
                }

                switch (asyncOp.webRequest.result) {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        return asyncOp.webRequest.downloadHandler.data;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"{asyncOp.webRequest.result}: {asyncOp.webRequest.error}\nURL: {asyncOp.webRequest.url}");
                        Debug.LogError($"{asyncOp.webRequest.downloadHandler.text}");
                        break;
                    default:
                        break;
                }

            } catch (Exception e) {
                Debug.LogError(e);
            } finally {
                wr.Dispose();
            }
            return default;
        }
    }

    private static UnityWebRequest GetRequest(string url, HTTPVerb verb, string postData, params Tuple<string, string>[] requestHeaders) {
        UnityWebRequest webRequest;

        switch (verb) {
            case HTTPVerb.GET:
                webRequest = UnityWebRequest.Get(url);
                break;
            case HTTPVerb.POST:
#if UNITY_2022_2_OR_NEWER
                webRequest = UnityWebRequest.PostWwwForm(url, postData);
#else
                webRequest = UnityWebRequest.Post(url, postData);
#endif
                byte[] rawBody = Encoding.UTF8.GetBytes(postData);
                webRequest.uploadHandler = new UploadHandlerRaw(rawBody);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                break;
            default:
                Debug.LogError("Invalid HTTP request method.");
                return null;
        }

        // set optional headers
        if (requestHeaders != null) {
            foreach (var header in requestHeaders) {
                webRequest.SetRequestHeader(header.Item1, header.Item2);
            }
        }

        return webRequest;
    }
}