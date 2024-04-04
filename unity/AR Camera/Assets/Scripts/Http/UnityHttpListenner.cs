using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using Extensions;
using FlutterEmbed.SendToFlutter;
using UnityEngine;
using Utils;
using static ArFoundationController;
using Timer = System.Timers.Timer;

namespace Http
{
    public class UnityHttpListener : MonoBehaviour
    {
        private HttpListener listener;
        private Thread listenerThread;
        private static readonly List<Route> Routes = new();
        private static UnityHttpListener _instance;
        private const float TimeOut = 10f;

        public enum HttpMethod
        {
            Get,
            Post
        }

        private static HttpMethod HttpMethodFromFromString(string method) => method switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            _ => throw new Exception("Invalid method")
        };

        private void Awake() => _instance = this;

        private void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://+:4444/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();

            listenerThread = new Thread(StartListener);
            listenerThread.Start();
            SendToFlutter.Send("Unity server started at http://+:4444/");
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        private void StartListener()
        {
            try
            {
                while (true)
                {
                    var result = listener.BeginGetContext(ListenerCallback, listener);
                    result.AsyncWaitHandle.WaitOne();
                }
            }
            finally
            {
                listener.Stop();
                listenerThread.Abort();
            }
        }

        private void OnApplicationQuit()
        {
            if (listener is { IsListening: true }) listener.Stop();
            if (listenerThread is { IsAlive: true }) listenerThread.Abort();
            SendToFlutter.Send("Server Stopped");
        }

        private void OnDestroy()
        {
            OnApplicationQuit();
        }

        private void ListenerCallback(IAsyncResult result)
        {
            var context = listener.EndGetContext(result);

            if (context.Request == null) throw new Exception("Request is null");

            var request = context.Request;
            var response = context.Response;

            try
            {
                RegisterCorsHeaders(response);

                var route = Routes.Find(r => r.Path == request.Url.LocalPath && r.Method == HttpMethodFromFromString(request.HttpMethod));

                if (route != null)
                {
                    if (route.Method != HttpMethodFromFromString(request.HttpMethod))
                    {
                        response.Send("Method not allowed", 405);
                        return;
                    }

                    if (route.DispatchInUnityMainThread)
                    {
                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            try
                            {
                                route.Handler(response, request);
                            }
                            catch (Exception e)
                            {
                                SendToFlutter.Send(OnErrorPrefix + e.Message);
                                response.Send(e.Message, 400);
                            }
                        });
                        return;
                    }

                    route.Handler(response, request);
                }
                else
                {
                    response.Send("Not Found", 404);
                }
            }
            catch (Exception e)
            {
                SendToFlutter.Send(OnErrorPrefix + e.Message);
                response.Send(e.Message, 400);
            }
        }


        private static void RegisterCorsHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
        }

        public static void AddRoute(string path, Action handler, HttpMethod method = HttpMethod.Get, bool dispatchInUnityMainThread = false)
        {
            AddRoute(path, (_, _) => handler(), method, dispatchInUnityMainThread);
        }

        private static void TimeOutDispatch(HttpListenerResponse response, Action<Timer> action)
        {
            var timeOut = new Timer(TimeOut * 1000);
            action(timeOut);
            timeOut.Elapsed += (_, _) =>
            {
                response.Send("Request Timeout", 408);
                timeOut.Stop();
            };
            timeOut.Start();
        }

        private static void TimeOutDispatchCoroutine(HttpListenerResponse response, Func<Timer, IEnumerator> action)
        {
            var timeOut = new Timer(TimeOut * 1000);
            var coroutine = _instance.StartCoroutine(action(timeOut));
            timeOut.Elapsed += (_, _) =>
            {
                _instance.StopCoroutine(coroutine);
                response.Send("Request Timeout", 408);
                timeOut.Stop();
            };
            timeOut.Start();
        }

        public static void AddRoute<T>(string path, Action<T> handler, HttpMethod method = HttpMethod.Get, bool dispatchInUnityMainThread = false)
        {
            AddRoute(path, (_, request) =>
            {
                var body = request.GetBody<T>();
                handler(body);
            }, method, dispatchInUnityMainThread);
        }

        public static void AddRoute(string path, Action<HttpListenerResponse, HttpListenerRequest> handler, HttpMethod method = HttpMethod.Get, bool dispatchInUnityMainThread = false)
        {
            Routes.Add(new Route
            {
                Path = path,
                Method = method,
                Handler = (res, req) =>
                {
                    TimeOutDispatch(res, timeOut =>
                    {
                        handler(res, req);
                        res.Close();
                        timeOut.Stop();
                    });
                },
                DispatchInUnityMainThread = dispatchInUnityMainThread
            });
        }

        public static void AddRoute<T>(string path, Func<T, IEnumerator> handler, HttpMethod method = HttpMethod.Get)
        {
            AddRoute(path, Routine, method);
            return;

            IEnumerator Routine(HttpListenerResponse response, HttpListenerRequest request)
            {
                var body = request.GetBody<T>();
                return handler(body);
            }
        }


        public static void AddRoute(string path, Func<HttpListenerResponse, HttpListenerRequest, IEnumerator> handler, HttpMethod method = HttpMethod.Get)
        {
            Routes.Add(new Route
            {
                Path = path,
                Method = method,
                Handler = (res, req) => TimeOutDispatchCoroutine(res, timeOut => Routine(res, req, timeOut)),
                DispatchInUnityMainThread = true
            });
            return;

            IEnumerator Routine(HttpListenerResponse res, HttpListenerRequest req, Timer timeoutTimer)
            {
                yield return handler(res, req);
                res.Close();
                timeoutTimer.Stop();
            }
        }


        private class Route
        {
            public Action<HttpListenerResponse, HttpListenerRequest> Handler { get; set; }
            public string Path { get; set; }
            public HttpMethod Method { get; set; }
            public bool DispatchInUnityMainThread { get; set; }
        }
    }
}