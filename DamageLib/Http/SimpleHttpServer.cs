using System;
using System.Net;
using System.Text;
using System.Threading;

namespace DamageBot.Http {
    /// <summary>
    /// Used mostly to fetch the authentication tokens.
    /// Because, y'know. Kinda wasteful to let some apache boot up just to fetch stuff on s imple page.
    /// May or may not be ripped out of https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server
    /// </summary>
    public class SimpleHttpServer {
        private readonly HttpListener listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> responderMethod;

        public SimpleHttpServer(string[] prefixes, Func<HttpListenerRequest, string> method) {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later. What kind of backwater Operating System are you running on?");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (string s in prefixes)
                listener.Prefixes.Add(s);

            responderMethod = method;
            
            listener.Start();
        }

        public SimpleHttpServer(Func<HttpListenerRequest, string> method, params string[] prefixes) : this(prefixes, method) {
        }

        public void Run() {
            ThreadPool.QueueUserWorkItem((o) => {
                Console.WriteLine("Webserver running...");
                try {
                    while (listener.IsListening) {
                        ThreadPool.QueueUserWorkItem((c) => {
                            var ctx = c as HttpListenerContext;
                            try {
                                string rstr = responderMethod(ctx.Request);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch (Exception e){
                                Console.WriteLine(e.Message);
                            } 
                            finally {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, listener.GetContext());
                    }
                }
                catch {
                } // suppress any exceptions
            });
        }

        public void Stop() {
            listener.Stop();
            listener.Close();
        }
    }
}