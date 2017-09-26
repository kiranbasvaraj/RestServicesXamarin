using Newtonsoft.Json;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestClientClass
{
    public sealed class RestClient : IRestClient
    {
        private Guid authToken;
        private static readonly RestClient instance = new RestClient();
        static RestClient()
        {
        }
        public static RestClient Instance
        {
            get
            {
                return instance;
            }
        }
        private HttpClient GetClient(HttpClientHandler handler = null)
        {
            var client = handler == null ? new HttpClient() : new HttpClient(handler, true);
            client.Timeout = TimeSpan.FromSeconds(360);
            return client;
        }
        private void Dispose()
        {
        }

        public string msg = "PLease Connect To internet and try again";
        private async Task<HttpResponseMessage> RequestAsync(HttpMethod method, string url, object payload = null)
        {
            HttpResponseMessage  _message=null;


            if (CrossConnectivity.Current.IsConnected)
            {
                try
                {
                    var request = PrepareRequest(method, url, payload);

                    if (authToken != default(Guid))
                    {
                        request.Headers.Add("authToken", authToken.ToString());
                    }
                    _message=await GetClient().SendAsync(request, HttpCompletionOption.ResponseContentRead);
                  
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error while Sending Request : " + ex.Message);
                }
            }
            else
            {
              //  return null;
            }
            return _message;
               
            // Application.Current.MainPage = new Views.HomePages.ErrorPage();
            //msg.ToToast(ToastNotificationType.Success);




        }
        private HttpRequestMessage PrepareRequest(HttpMethod method, string url, object payload)
        {
            var uri = PrepareUri(url);
            var request = new HttpRequestMessage(method, uri);
            if (payload != null)
            {
                var json = JsonConvert.SerializeObject(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            return request;
        }
        private Uri PrepareUri(string url)
        {
            return new Uri(url);
        }

        private readonly Action<HttpStatusCode, string> _defaultErrorHandler = (statusCode, body) =>
        {
            if (statusCode < HttpStatusCode.OK || statusCode >= HttpStatusCode.BadRequest)
            {
                Debug.WriteLine(string.Format("Request responded with status code={0}, response={1}", statusCode, body));
                throw new RestClientException(statusCode, body);
            }
        };
        private void HandleIfErrorResponse(HttpStatusCode statusCode, string content, Action<HttpStatusCode, string> errorHandler = null)
        {
            if (errorHandler != null)
            {
                errorHandler(statusCode, content);
            }
            else
            {
                _defaultErrorHandler(statusCode, content);
            }
        }
        private T GetValue<T>(String value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        public string GetResponse { get; set; }
        public async Task<T> GetAsync<T>(string url, bool useAuthToken = false)
        {
            try
            {
                #region Set AuthToken
                if (useAuthToken && AppConstants.USER_AUTH_TOKEN != default(Guid))
                    authToken = AppConstants.USER_AUTH_TOKEN;
                #endregion
                HttpResponseMessage response = await RequestAsync(HttpMethod.Get, url).ConfigureAwait(false);
                GetResponse = response.ToString();
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                if (typeof(T) == typeof(string))
                {
                    return GetValue<T>(content);
                }
                #region Reset AuthToken
                if (useAuthToken && authToken != default(Guid))
                    authToken = default(Guid);
                #endregion
                var X = JsonConvert.DeserializeObject<T>(content);
                return X;
                //return JsonConvert.DeserializeObject<T>(content);
            }
            catch (System.Net.WebException ex)
            {
                throw ex;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Error in GET Request :" + ex.Message);
            }
            return default(T);
        }
        public async Task<T> PostAsync<T>(string url, object payload, bool useAuthToken = true)
        {
            try
            {
                #region Set AuthToken
                if (useAuthToken && AppConstants.USER_AUTH_TOKEN != default(Guid))
                    authToken = AppConstants.USER_AUTH_TOKEN;
                #endregion
                HttpResponseMessage response = await RequestAsync(HttpMethod.Post, url, payload).ConfigureAwait(false);

                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                if (typeof(T) == typeof(string))
                {
                    return GetValue<T>(content);
                }
                #region Reset AuthToken
                if (useAuthToken && authToken != default(Guid))
                    authToken = default(Guid);
                #endregion
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Error in POST Request :" + ex.Message);
            }
            catch (Exception ex)
            {

            }
            return default(T);
        }
        public async Task<T> PutAsync<T>(string url, object payload, bool useAuthToken = true)
        {
            try
            {
                #region Set AuthToken
                if (useAuthToken && AppConstants.USER_AUTH_TOKEN != default(Guid))
                    authToken = AppConstants.USER_AUTH_TOKEN;
                #endregion
                HttpResponseMessage response = await RequestAsync(HttpMethod.Put, url, payload).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                if (typeof(T) == typeof(string))
                {
                    return GetValue<T>(content);
                }
                #region Reset AuthToken
                if (useAuthToken && authToken != default(Guid))
                    authToken = default(Guid);
                #endregion
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Error in PUT Request :" + ex.Message);
            }
            return default(T);
        }
        public async Task<T> PostAsyncWithoutReturnContent<T>(string url, object payload, bool useAuthToken = true)
        {
            try
            {
                #region Set AuthToken
                if (useAuthToken && AppConstants.USER_AUTH_TOKEN != default(Guid))
                    authToken = AppConstants.USER_AUTH_TOKEN;
                #endregion

                HttpResponseMessage response = await RequestAsync(HttpMethod.Post, url, payload).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                if (typeof(T) == typeof(string))
                {
                    return GetValue<T>(content);
                }
                #region Reset AuthToken
                if (useAuthToken && authToken != default(Guid))
                    authToken = default(Guid);
                #endregion

            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Error in POST Request :" + ex.Message);
            }
            return default(T);
        }


        public async Task<T> PutAsyncWithoutReturnContent<T>(string url, object payload, bool useAuthToken = false)
        {
            try
            {
                #region Set AuthToken
                if (useAuthToken && AppConstants.USER_AUTH_TOKEN != default(Guid))
                    authToken = AppConstants.USER_AUTH_TOKEN;
                #endregion

                HttpResponseMessage response = await RequestAsync(HttpMethod.Post, url, payload).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                if (typeof(T) == typeof(string))
                {
                    return GetValue<T>(content);
                }
                #region Reset AuthToken
                if (useAuthToken && authToken != default(Guid))
                    authToken = default(Guid);
                #endregion

            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Error in POST Request :" + ex.Message);
            }
            return default(T);
        }
        public async Task<T> DeleteAsync<T>(string url, bool useAuthToken = false)
        {
            try
            {
                #region Set AuthToken
                if (useAuthToken && AppConstants.USER_AUTH_TOKEN != default(Guid))
                    authToken = AppConstants.USER_AUTH_TOKEN;
                #endregion
                HttpResponseMessage response = await RequestAsync(HttpMethod.Delete, url).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                if (typeof(T) == typeof(string))
                {
                    return GetValue<T>(content);
                }
                #region Reset AuthToken
                if (useAuthToken && authToken != default(Guid))
                    authToken = default(Guid);
                #endregion
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine("Error in DELETE Request :" + ex.Message);
            }
            return default(T);
        }

    }
}