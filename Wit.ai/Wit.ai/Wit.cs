﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using Wit.ai.Models;
using Wit.ai.Utils;

namespace Wit.ai
{
    public class Wit
    {
        public string WIT_API_HOST = "https://api.wit.ai";

        /// <summary>
        /// This parameter is a date that represents the “version” of the Wit API. Default value is 20170107
        /// </summary>
        public string WIT_API_VERSION = "20170307";
        string LEARN_MORE = "Learn more at https://wit.ai/docs/quickstart";

        RestClient client;
        Dictionary<string, int> sessions;
        WitActions actions = new WitActions();

        /// <summary>
        /// Initializes new instance of Wit class
        /// </summary>
        /// <param name="accessToken">Client access token. You can grab it from your Wit console, under Settings\Access Token.</param>
        /// <param name="actions">(optional if you only use message()) the dictionary with your actions. Actions has action names as keys and action implementations as values.</param>
        public Wit(string accessToken, WitActions actions = null)
        {
            client = PrepareRestClient(accessToken);

            sessions = new Dictionary<string, int>();

            if (actions != null)
            {
                this.actions = ValidateActions(actions);
            }
        }

        RestClient PrepareRestClient(string accessToken)
        {
            var restClient = new RestClient(WIT_API_HOST);

            restClient.AddDefaultHeader("Authorization", $"Bearer {accessToken}");
            restClient.AddDefaultHeader("Content-Type", "application/json");
            restClient.AddDefaultHeader("Accept", "application/json");
            restClient.AddDefaultParameter("v", WIT_API_VERSION, ParameterType.QueryString);

            return restClient;
        }

        WitActions ValidateActions(WitActions actions)
        {
            if (!actions.ContainsKey("send"))
            {
                Console.WriteLine("The 'send' action is missing.");
            }

            return actions;
        }

        /// <summary>
        /// Capture intent and entities from a text string
        /// </summary>
        /// <param name="msg">Text string to capture from</param>
        /// <param name="verbose">Calls the API in Verbose Mode</param>
        /// <returns>Message response</returns>
        public MessageResponse Message(string msg, bool verbose = true)
        {
            var request = new RestRequest("message", Method.GET);
            request.AddQueryParameter("q", msg);

            var responseObject = client.Execute(request);
            var response = JsonConvert.DeserializeObject<MessageResponse>(responseObject.Content);

            return response;
        }

        /// <summary>
        /// Sends a voice to the wit.ai, gets a messge
        /// </summary>
        /// <param name="soundLocation">The location of the voice</param>
        /// <returns>Message response</returns>
        public MessageResponse SendVoice(FileInfo soundLocation)
        {
            var request = new RestRequest("speech", Method.POST);
            request.AddHeader("Transfer-encoding", "chunked");
            request.AddFile(soundLocation.Name, soundLocation.FullName, soundLocation.Extension.ToContentType());
            var responseObject = client.Execute(request);
            var response = JsonConvert.DeserializeObject<MessageResponse>(responseObject.Content);
            return response;
        }

        /// <summary>
        /// Returns what your bot should do next. The next step can be either answering to the user, performing an action, or waiting for further requests.
        /// </summary>
        /// <param name="sessionId">A specific ID of your choosing representing the session your query belongs to</param>
        /// <param name="message">A message from the user</param>
        /// <param name="context">Chat context</param>
        /// <param name="verbose">Calls the API in verbose mode</param>
        /// <returns <see cref="ConverseResponse"/>>Converse response</returns>
        public ConverseResponse Converse(string sessionId, string message, WitContext context, bool verbose = true)
        {
            if (context == null)
            {
                context = new WitContext();
            }

            var request = new RestRequest("converse", Method.POST);
            request.AddJsonBody(context);
            if (message != null)
            {
                request.AddQueryParameter("q", message);
            }

            request.AddQueryParameter("session_id", sessionId);

            var responseObject = client.Execute(request);
            var response = JsonConvert.DeserializeObject<ConverseResponse>(responseObject.Content);

            return response;
        }

        /// <summary>
        /// Runs interactive command line chat between user and bot. Runs indefinately until EOF is entered to the prompt.
        /// </summary>
        /// <param name="context">Chat context</param>
        /// <param name="maxSteps">Max number of steps. Set to { } if omitted</param>
        public void Interactive(WitContext context = null, int maxSteps = 5)
        {
            if (actions == null)
            {
                ThrowMustHaveActions();
            }

            if (maxSteps <= 0)
            {
                throw new WitException("max iterations reached");
            }

            if (context == null)
            {
                context = new WitContext();
            }

            string message;
            while (true)
            {
                try
                {
                    message = Console.ReadLine();
                }
                catch (Exception)
                {
                    return;
                }

                var response = RunActions("session-id-01", message, context, maxSteps);
                context = response.Context;
                response.Messages.ForEach(msg => Console.WriteLine(msg));
            }
        }

        /// <summary>
        /// A higher-level method to the Wit converse API
        /// </summary>
        /// <param name="sessionId">A specific ID of your choosing representing the session your query belongs to</param>
        /// <param name="message">A message from the user.</param>
        /// <param name="context">Chat context</param>
        /// <param name="maxSteps">Max number of steps</param>
        /// <param name="verbose">Calls the API in verbose mode</param>
        /// <returns <see cref="BotResponse"/>>The bot response</returns>
        public BotResponse RunActions(string sessionId, string message, WitContext context,
                                                      int maxSteps = 5, bool verbose = true)
        {
            var botResponse = new BotResponse(context);
            if (actions == null)
            {
                ThrowMustHaveActions();
            }

            if (context == null)
            {
                context = new WitContext();
                botResponse.Context = context;
            }

            /** Figuring out whether we need to reset the last turn.
                Each new call increments an index for the session.
                We only care about the last call to run_actions.
                All the previous ones are discarded (preemptive exit).*/
            var currentRequest = 1;
            if (sessions.ContainsKey(sessionId))
            {
                currentRequest = sessions[sessionId] + 1;
            }

            sessions[sessionId] = currentRequest;

            botResponse = _RunActions(sessionId, currentRequest, message, botResponse, maxSteps, verbose);

            // Cleaning up once the last call to RunActions finishes.
            if (currentRequest == sessions[sessionId])
            {
                sessions.Remove(sessionId);
            }

            return botResponse;
        }

        BotResponse _RunActions(string sessionId, int currentRequest,
                                                    string message, BotResponse response, int maxSteps = 5, bool verbose = true)
        {
            if (maxSteps <= 0)
            {
                throw new WitException("Max steps reached, stopping.");
            }

            var json = Converse(sessionId, message, response.Context, verbose);

            if (json.Type == null)
            {
                throw new WitException("Couldn\'t find type in Wit response");
            }

            if (currentRequest != sessions[sessionId])
            {
                return response;
            }

            // backwards-compatibility with API version 20160516
            if (json.Type == "merge")
            {
                json.Type = "action";
                json.Action = "merge";
            }

            if (json.Type == "error")
            {
                throw new Exception("Oops, I don\'t know what to do.");
            }

            if (json.Type == "stop")
            {
                return response;
            }

            var request = new ConverseRequest();

            request.SessionId = sessionId;
            request.Context = response.Context;
            request.Message = message;
            request.Entities = json.Entities;

            switch (json.Type)
            {

                case "msg":
                    ThrowIfActionMissing("send");

                    var converseResponse = new ConverseResponse();
                    converseResponse.Msg = json.Msg;
                    converseResponse.QuickReplies = json.QuickReplies;

                    // SDK is able to handle multiple bot responses at the same time
                    response.Messages.Add(converseResponse.Msg);

                    actions["send"](request, converseResponse);
                    // actions["send"](request);
                    break;
                case "action":
                    var action = json.Action;
                    ThrowIfActionMissing(action);
                    response.Context = actions[action](request, null);
                    // context = this.actions[action](request);
                    if (response.Context == null)
                    {
                        Console.WriteLine("missing context - did you forget to return it?");
                        response.Context = new WitContext();
                    }

                    break;
                default:
                    throw new WitException($"unknown type:  {json.Type}");
            }

            if (currentRequest != sessions[sessionId])
            {
                return response;
            }

            return _RunActions(sessionId, currentRequest, null, response, maxSteps - 1, verbose);
        }

        void ThrowIfActionMissing(string actionName)
        {
            if (!actions.ContainsKey(actionName))
            {
                throw new WitException($"unknown action {actionName}");
            }
        }

        void ThrowMustHaveActions()
        {
            throw new WitException($"You must provide the 'actions' parameter to be able to use runActions. ${LEARN_MORE}");
        }
    }
}