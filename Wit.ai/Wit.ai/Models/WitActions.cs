using System;
using System.Collections.Generic;

namespace Wit.ai.Models
{
    public class WitActions : Dictionary<string, Func<ConverseRequest, ConverseResponse, WitContext>> { }
}