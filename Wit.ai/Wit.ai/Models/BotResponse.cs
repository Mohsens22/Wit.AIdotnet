﻿using System.Collections.Generic;
using Wit.ai.Models;

namespace Wit.ai
{
    /// <summary>
    /// Represents a bot response
    /// </summary>
    public class BotResponse
    {
        /// <summary>
        /// Wit context. This is just a wrapper of a Dictionary
        /// </summary>
        public WitContext Context { get; set; }

        /// <summary>
        /// Messages coming from the Bot. This is a List because the bot can send multiple messages at the same time
        /// </summary>
        public List<string> Messages { get; set; }

        public BotResponse(WitContext context)
        {
            Context = context;
            Messages = new List<string>();
        }
    }
}