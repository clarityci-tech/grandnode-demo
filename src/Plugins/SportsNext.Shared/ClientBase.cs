using Newtonsoft.Json;
using SportsNext.Shared.Binders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SportsNext.Shared
{
    public abstract class ClientBase<T400,T401,T403,T500>
    {
        protected HttpClient Client;

        protected ClientBase(HttpClient client)
        {
            Client = client;
        }

        protected abstract List<string> TypeNames { get; }

        private string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
                SerializationBinder = new TypeNameSerializationBinder(this.TypeNames)
            });
        }

        protected async Task<TResult> GetAsync<TResult>(string path, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
        {
            using (HttpResponseMessage response = await Client.GetAsync(path, httpCompletionOption))
            {
                return await DeserializeResponse<TResult>(response);
            }
        }

        protected async Task<HttpResponseMessage> GetAsync(string path, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
        {
            return await Client.GetAsync(path, httpCompletionOption);
        }

        protected async Task<TResult> PostAsync<TResult>(string path, HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            using (HttpResponseMessage response = await Client.PostAsync(path, content))
            {
                return await DeserializeResponse<TResult>(response);
            }
        }

        protected async Task<TResult> PostAsync<TResult>(string path, object content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string json = this.SerializeObject(content);

            if (string.IsNullOrWhiteSpace(json))
            {
                return default(TResult);
            }

            using (StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                return await PostAsync<TResult>(path, jsonContent);
            }
        }

        protected async Task<HttpResponseMessage> PostAsync(string path, HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            try
            {
                var message = await Client.PostAsync(path, content);
                return message;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected async Task<HttpResponseMessage> PostAsync(string path, object content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string json = this.SerializeObject(content);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            using (StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                return await PostAsync(path, jsonContent);
            }
        }

        protected async Task<TResult> PutAsync<TResult>(string path, HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            using (HttpResponseMessage response = await Client.PutAsync(path, content))
            {
                return await DeserializeResponse<TResult>(response);
            }
        }

        protected async Task<TResult> PutAsync<TResult>(string path, object content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string json = this.SerializeObject(content);

            using (StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                return await PutAsync<TResult>(path, jsonContent);
            }
        }

        protected async Task<HttpResponseMessage> PutAsync(string path, HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return await Client.PutAsync(path, content);
        }

        protected async Task<HttpResponseMessage> PutAsync(string path, object content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string json = this.SerializeObject(content);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            using (StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                return await PutAsync(path, jsonContent);
            }
        }

        protected async Task<HttpResponseMessage> DeleteAsync(string path)
        {
            return await Client.DeleteAsync(path);
        }

        protected async Task<TResult> DeserializeResponse<TResult>(HttpResponseMessage responseMessage)
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                SerializationBinder = new TypeNameSerializationBinder(this.TypeNames)
            };

            if (responseMessage != null && responseMessage.IsSuccessStatusCode)
            {
                using (HttpContent responseContent = responseMessage.Content)
                {
                    string stringContent = await responseContent.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(stringContent))
                    {
                        return JsonConvert.DeserializeObject<TResult>(stringContent, settings);
                    }
                }
            }
            else
            {
                ClientException exception = null;

                try
                {
                    var stringContent = await responseMessage.Content.ReadAsStringAsync();

                    if (responseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest) // 400
                    {
                        var response = JsonConvert.DeserializeObject<T400>(stringContent);
                        exception = new ClientException<T400>("A bad request has been supplied.", responseMessage.StatusCode, response);
                    }
                    else if (responseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest) // 401
                    {
                        var response = JsonConvert.DeserializeObject<T401>(stringContent);
                        exception = new ClientException<T401>("Authentication failed.", responseMessage.StatusCode, response);
                    }
                    else if (responseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest) // 403
                    {
                        var response = JsonConvert.DeserializeObject<T403>(stringContent);
                        exception = new ClientException<T403>("This action is forbidden.", responseMessage.StatusCode, response);
                    }
                    else
                    {
                        var response = JsonConvert.DeserializeObject<T500>(stringContent);
                        exception = new ClientException<T500>("An unexpected error has occurred. Please contact an administrator to investigate further.", responseMessage.StatusCode, response);
                    }
                }
                catch(Exception ex)
                {
                    throw new ClientException("Unexpected error trying to interpret the response. " + ex.GetType(), ex);
                }

                throw exception;
            }

            return default(TResult);
        }
    }
}

