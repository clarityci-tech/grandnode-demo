using System;
namespace SportsNext.Shared
{
	public class ClientException<T> : ClientException
    {
		internal ClientException(string message, System.Net.HttpStatusCode statusCode, T response, Exception inner = null) : base(message, inner)
		{
			this.Response = response;
            this.StatusCode = statusCode;
		}

		public T Response { get; private set; }

        public System.Net.HttpStatusCode StatusCode { get; private set; }
    }

    public class ClientException : Exception
    {
        internal ClientException(string message, Exception inner = null) : base(message, inner)
        {
        }
    }
}

