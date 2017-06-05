using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using DamageBot.Http;
using Flurl.Http;
using Newtonsoft.Json;

namespace DamageBot {
    public class AuthenticationHandler {
        private SimpleHttpServer srv;
        private BotConfig cfg;

        public bool WaitingForAuthToken {
            get;
            private set;
        }

        public AuthenticationHandler(BotConfig cfg) {
            this.srv = new SimpleHttpServer(ServerRequestHandler, "http://localhost:8080/");
            this.cfg = cfg;
        }

        public void BeginAuthProcess() {
            WaitingForAuthToken = true;
            srv.Run();
        }

        public void StopProcess() {
            srv.Stop();
        }

        private string ServerRequestHandler(HttpListenerRequest r) {
            if (r.RawUrl.Contains("handle")) {
                return DoHandlePage(r);
            }
            return DoSuccessPage(r);
        }

        private string DoSuccessPage(HttpListenerRequest r) {
            return "Success. You can return to the setup window now.";
        }

        private string DoHandlePage(HttpListenerRequest r) {
            string code = r.QueryString["code"];

            var values = new Dictionary<string, string> {
                {"client_id", cfg.ApplicationClientId}, 
                {"client_secret", cfg.ApplicationClientSecret},
                {"code", code},
                {"grant_type", "authorization_code"},
                {"redirect_uri", "http://localhost:8080/handle"},
            };
            
            
            var responseString = "https://api.twitch.tv/kraken/oauth2/token"
                .PostUrlEncodedAsync(values)
                .ReceiveString();
            
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString.Result);
            cfg.ApiAuthKey = jsonResponse.access_token.ToString();
            WaitingForAuthToken = false;
            return "Got the key, thank you!";
        }
    }
}