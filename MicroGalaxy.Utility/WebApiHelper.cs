using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace MicroGalaxy.Utility
{
    public class WebApiHelper
    {
        public static T1 CallPostWebApi<T1, T2>(string path, T2 request, string serviceUrl, int? timeOut = 10)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, timeOut.HasValue ? timeOut.Value : 10);
                client.BaseAddress = new Uri(serviceUrl);
                JsonMediaTypeFormatter.DefaultMediaType.CharSet = "UTF-8";
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("charset=utf-8"));
                MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
                HttpContent content = new ObjectContent<T2>(request, jsonFormatter);
               
                var taskRes = client.PostAsync(path, content);
                try
                {
                    var response = taskRes.Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        T1 result = response.Content.ReadAsAsync<T1>().Result;
                        return result;
                    }
                    else
                    {
                        return default(T1);
                    }
                }
                catch (Exception)
                {
                    return default(T1);
                }
            }
        }

        public static T1 CallGetWebApi<T1>(string path, string serviceUrl, int? timeOut = 20)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, timeOut.HasValue ? timeOut.Value : 10);
                client.BaseAddress = new Uri(serviceUrl);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var taskRes = client.GetAsync(path);
                try
                {
                    var response = taskRes.Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        T1 result = response.Content.ReadAsAsync<T1>().Result;
                        return result;
                    }
                    else
                    {
                        return default(T1);
                    }
                }
                catch (Exception)
                {
                    return default(T1);
                }
            }
        }

        public static List<TResponse> CallWebApiBatch<TRequest, TResponse>(HttpMethod method, string endpoint, List<TRequest> batchRequestModels, string path, string serviceUrl, int? timeOut = 10)
        {
            var result = new List<TResponse>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(serviceUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var multiContents = new MultipartContent("mixed");
                if (method == HttpMethod.Post)
                {
                    foreach (var batchRequestModel in batchRequestModels)
                    {
                        multiContents.Add(new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, serviceUrl + path)
                        {
                            Content = new ObjectContent<TRequest>(batchRequestModel, new JsonMediaTypeFormatter())
                        }));
                    }
                }
                if (method == HttpMethod.Get)
                {
                    foreach (var batchRequestModel in batchRequestModels)
                    {
                        multiContents.Add(new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, serviceUrl + path + batchRequestModel)));
                    }
                }
                var batchRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = multiContents
                };
                var batchResponse = client.SendAsync(batchRequest).Result;
                var streamProvider = batchResponse.Content.ReadAsMultipartAsync().Result;
                foreach (var content in streamProvider.Contents)
                {
                    var responseMessage = content.ReadAsHttpResponseMessageAsync().Result;
                    var response = responseMessage.Content.ReadAsAsync<TResponse>(new[] { new JsonMediaTypeFormatter() }).Result;
                    result.Add(response);
                }
                return result;
            }
        }

        public static async Task<T1> CallPostWebApiAsync<T1, T2>(string path, T2 request, string serviceUrl, int? timeOut = 10)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, timeOut.HasValue ? timeOut.Value : 10);
                client.BaseAddress = new Uri(serviceUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
                HttpContent content = new ObjectContent<T2>(request, jsonFormatter);
                var taskRes = client.PostAsync(path, content);
                var response = await taskRes;
                T1 result = await response.Content.ReadAsAsync<T1>();
                return result;
            }
        }

        public static async Task<T1> CallGetWebApiAsync<T1>(string path, string serviceUrl, int? timeOut = 10)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, timeOut.HasValue ? timeOut.Value : 10);
                client.BaseAddress = new Uri(serviceUrl);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var taskRes = client.GetAsync(path);
                var response = await taskRes;
                T1 result = await response.Content.ReadAsAsync<T1>();
                return result;
            }
        }

        public static async Task<List<TResponse>> CallWebApiBatchAsync<TRequest, TResponse>(HttpMethod method, string endpoint, List<TRequest> batchRequestModels, string path, string serviceUrl, int? timeOut = 10)
        {
            var result = new List<TResponse>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(serviceUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var multiContents = new MultipartContent("mixed");
                if (method == HttpMethod.Post)
                {
                    foreach (var batchRequestModel in batchRequestModels)
                    {
                        multiContents.Add(
                            new HttpMessageContent(new HttpRequestMessage(HttpMethod.Post, serviceUrl + path)
                            {
                                Content = new ObjectContent<TRequest>(batchRequestModel, new JsonMediaTypeFormatter())
                            }));
                    }
                }
                if (method == HttpMethod.Get)
                {
                    foreach (var batchRequestModel in batchRequestModels)
                    {
                        multiContents.Add(new HttpMessageContent(new HttpRequestMessage(HttpMethod.Get, serviceUrl + path + batchRequestModel)));
                    }
                }
                var batchRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = multiContents
                };
                var batchResponse = await client.SendAsync(batchRequest);
                var streamProvider = await batchResponse.Content.ReadAsMultipartAsync();
                foreach (var content in streamProvider.Contents)
                {
                    var responseMessage = await content.ReadAsHttpResponseMessageAsync();
                    var response =
                        responseMessage.Content.ReadAsAsync<TResponse>(new[] { new JsonMediaTypeFormatter() }).Result;
                    result.Add(response);
                }
                return result;
            }
        }
    }
}
