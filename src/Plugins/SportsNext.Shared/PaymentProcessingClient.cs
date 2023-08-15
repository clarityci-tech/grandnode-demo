using GolfNow.Payment.Processing.API.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SportsNext.Shared
{
	public class PaymentProcessingClient : ClientBase<Models.ErrorResponse, string, string, string>
	{
		public PaymentProcessingClient(string url, string clientId, string secret) : this(new HttpClient())
		{
			this.Client.BaseAddress = new Uri(url);
			this.Client.DefaultRequestHeaders.Clear();

            var authenticationString = $"{clientId}:{secret}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
            this.Client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64EncodedAuthenticationString);
        }


        protected override List<string> TypeNames
        {
            get
            {
                return new List<string> {

                "GolfNow.Payment.Processing.API.Manage.Model.{0}, GolfNow.Payment.Processing.API.Manage.Model",

                "GolfNow.Payment.Processing.API.Model.{0}, GolfNow.Payment.Processing.API.Model",

                "System.Collections.Generic",

                "GolfNow.Payment.Processing.API.Common.Model"
                    };
            }
        }

    public PaymentProcessingClient(HttpClient client) : base(client)
		{
		}

		public Task<Capture> ProcessItem(SingleItem body)
		{
            return this.PostAsync<Capture>("api/transaction/item/process", body);
        }

        public Task<Reversal> PartialTransactionItemCredit(SingleItemReversal body)
		{
            return this.PutAsync<Reversal>("api/transaction/item/credit/partial", body);
        }

		public Task<List<Transaction>> GetTransactions(string confirmationID, string externalOrderID, string merchantKey)
		{
            return this.GetAsync<List<Transaction>>($"api/transaction?confirmationID={confirmationID}&externalOrderID={externalOrderID}&merchantKey={merchantKey}");
        }
    }
}

