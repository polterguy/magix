/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Net;
using System.Web.UI;
using System.Reflection;
using System.Collections.Generic;

namespace Magix.Core
{
    /**
     * Class contains methods for raising events and other helpers, like for instance helpers
     * to load controls and such. Though often you'll not use this directly, but rather use
     * it through helper methods on your ActiveControllers and ActiveModules
     */
    public sealed class ActiveEvents
    {
        private readonly Dictionary<string, List<Tuple<MethodInfo, object>>> _staticEvents =
            new Dictionary<string, List<Tuple<MethodInfo, object>>>();

        private static ActiveEvents _instance;

        private static Dictionary<string, string> _eventMappers = new Dictionary<string, string>();

        private static Dictionary<string, string> _urlMappers = new Dictionary<string, string>();

        private static Dictionary<string, bool> _remotelyActivated = new Dictionary<string, bool>();

        private delegate void AsyncDelegate(object sender, ActiveEventArgs e);

        private ActiveEvents()
        { }

        /**
         * This is our Singleton to access our only ActiveEvents object. This is
         * the property you'd use to gain access to the only existing ActiveEvents
         * object in your application pool
         */
        public static ActiveEvents Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(ActiveEvents))
                    {
                        if (_instance == null)
                            _instance = new ActiveEvents();
                    }
                }
                return _instance;
            }
        }

        /**
         * Will override the given 'from' ActiveEvent name and make it so that every time
         * anyone tries to raise the 'from' event, then the 'to' event will be raised instead.
         * Useful for 'overriding functionality' in Magix. This can also be accomplished through
         * doing the mapping in the system's web.config file
         */
        public void CreateEventMapping(string from, string to)
        {
            _eventMappers[from] = to;
        }

        /**
         * Will destroy the given [key] Active Event Mapping
         */
        public void RemoveMapping(string key)
        {
            _eventMappers.Remove(key);
        }

        /**
         * Will return the given Value for the given Key Active Event Override
         */
        public string GetEventMappingValue(string key)
        {
			if (_eventMappers.ContainsKey(key))
				return _eventMappers[key];
			return key;
        }

        /**
         * Returns an Enumerable of all the Keys in the Event Mapping Collection. Basically
         * which Active Events are overridden
         */
        public IEnumerable<string> EventMappingKeys
        {
            get
            {
                foreach (string idx in _eventMappers.Keys)
                {
                    yield return idx;
                }
            }
        }

        /**
         * Returns an Enumerable of all the Values in the Event Mapping Collection. Basically
         * where he Overridden Active Events are 'pointing'
         */
        public IEnumerable<string> EventMappingValues
        {
            get
            {
                foreach (string idx in _eventMappers.Values)
                {
                    yield return idx;
                }
            }
        }

		// TODO: Try to remove or make internal somehow...?
        public void RemoveListener(object context)
        {
			List<string> toRemoKeys = new List<string>();
            // Removing all event handler with the given context (object instance)
            foreach (string idx in InstanceMethod.Keys)
            {
                List<Tuple<MethodInfo, object>> idxCur = InstanceMethod[idx];
                List<Tuple<MethodInfo, object>> toRemove = 
                    new List<Tuple<MethodInfo, object>>();
                foreach (Tuple<MethodInfo, object> idxObj in idxCur)
                {
                    if (idxObj.Item2 == context)
                        toRemove.Add(idxObj);
                }
                foreach (Tuple<MethodInfo, object> idxObj in toRemove)
                {
                    idxCur.Remove(idxObj);
					if (idxCur.Count == 0)
					{
						toRemoKeys.Add (idx);
					}
                }
            }
			foreach (string idx in toRemoKeys)
			{
				InstanceMethod.Remove (idx);
			}
        }

		/**
		 * Will return all Active Events possible to raise within the system
		 */
		public IEnumerable<string> ActiveEventHandlers
		{
			get
			{
				foreach (string idx in _staticEvents.Keys)
				{
					if (!_eventMappers.ContainsKey (idx))
						yield return idx;
				}
				foreach (string idx in InstanceMethod.Keys)
				{
					if (!_eventMappers.ContainsKey (idx) && 
					    !_staticEvents.ContainsKey (idx))
						yield return idx;
				}
				foreach (string idx in _eventMappers.Keys)
				{
					yield return idx;
				}
			}
		}

		/**
		 * Makes an active event override to a remote server, such that once
		 * you raise it internally within your system, it will go polymorphistically
		 * and transparently towards your URL end-point, and raise the event instead
		 */
		public void OverrideRemotely (string activeEvent, string url)
		{
			_urlMappers[activeEvent] = url;
		}

		/**
		 * Removes a remotely overridden event, meaning once raise, the event will NOT go towards
		 * the URL end-point anymore
		 */
		public void RemoveRemoteOverride (string activeEvent)
		{
			if (!_urlMappers.ContainsKey (activeEvent))
				return;
			_urlMappers.Remove (activeEvent);
		}

		/**
		 * Returns the URL of the remote server the active event is
		 * overridden to, or null if no remote override exists
		 */
		public string RemotelyOverriddenURL (string activeEvent)
		{
			if (_urlMappers.ContainsKey (activeEvent))
				return _urlMappers[activeEvent];
			return null;
		}

		/**
		 * Returns true if the event is allowed to be remotely invoked by another server,
		 * using remotely activated events, to raise events on your server
		 */
		public bool IsAllowedRemotely (string str)
		{
			return _remotelyActivated.ContainsKey(str);
		}

		/**
		 * Marks an active event become a remotely activatable event, which means
		 * that other servers can invoke this Active Event on your server over
		 * HTTP
		 */
		public void MakeRemotable (string str)
		{
			_remotelyActivated[str] = true;
		}

		/**
		 * Shuts down an existing remotely activated event, such that it can no longer
		 * be raised by other servers
		 */
		public void RemoveRemotable (string str)
		{
			if (!_remotelyActivated.ContainsKey (str))
				return;
			_remotelyActivated.Remove (str);
		}

		/**
		 * Returns true if Active Event is an override, meaning somebody have overridden the
		 * original active event, and are pointing you to another handler
		 */
		public bool IsOverride (string key)
		{
			return _eventMappers.ContainsKey(key) && _eventMappers[key] != key;
		}

		// TODO: WTF ...?
		/**
		 * Returns true if Active Event is an override of an existing active event
		 */
		public bool IsOverrideSystem (string key)
		{
			bool retVal = _eventMappers.ContainsKey(key);
			if (retVal)
			{
				retVal = InstanceMethod.ContainsKey(key);
				if (!retVal)
					retVal = _staticEvents.ContainsKey(key);
			}
			return retVal;
		}

        /**
         * Raises an active event with an empty node as the initialization parameter.
         * This will dispatch control to all the ActiveEvent that are marked with
         * the Name attribute matching the name parameter of this method call.
         */
        public void RaiseActiveEvent(object sender, string name)
        {
			Node node = new Node();
            RaiseActiveEvent(sender, name, node);
        }

        private List<Tuple<MethodInfo, object>> SlurpAllEventHandlers(string eventName, bool forceNoNull)
        {
            List<Tuple<MethodInfo, object>> retVal = new List<Tuple<MethodInfo, object>>();

            // Adding static methods (if any)
            if (_staticEvents.ContainsKey(eventName))
            {
                foreach (Tuple<MethodInfo, object> idx in _staticEvents[eventName])
                {
                    retVal.Add(idx);
                }
            }

            // Adding instance methods (if any)
            if (InstanceMethod.ContainsKey(eventName))
            {
                foreach (Tuple<MethodInfo, object> idx in InstanceMethod[eventName])
                {
                    retVal.Add(idx);
                }
            }

			return retVal;
		}

		List<Tuple<MethodInfo, object>> SlurpAllNullEventHandlers(string eventName, bool forceNoNull)
		{
            List<Tuple<MethodInfo, object>> retVal = new List<Tuple<MethodInfo, object>>();

			if (forceNoNull)
				return retVal;

            // Adding NULL static methods (if any)
            if (_staticEvents.ContainsKey(""))
            {
                foreach (Tuple<MethodInfo, object> idx in _staticEvents[""])
                {
                    retVal.Add(idx);
                }
            }

            // Adding NULL instance methods (if any)
            if (InstanceMethod.ContainsKey(""))
            {
                foreach (Tuple<MethodInfo, object> idx in InstanceMethod[""])
                {
                    retVal.Add(idx);
                }
            }
			return retVal;
        }

        private void ExecuteEventMethod(
            MethodInfo method, 
            object context, 
            object sender, 
            ActiveEventArgs e)
        {
            method.Invoke(context, new[] { sender, e });
		}

        /**
         * Raises an event. This will dispatch control to all the ActiveEvent that are marked with
         * the Name attribute matching the name parameter of this method call.
         */
        public void RaiseActiveEvent(
            object sender, 
            string name, 
            Node pars)
        {
			RaiseActiveEvent (sender, name, pars, false);
        }

        /**
         * Raises an event. This will dispatch control to all the ActiveEvent that are marked with
         * the Name attribute matching the name parameter of this method call.
         */
        public void RaiseActiveEvent(
            object sender, 
            string name, 
            Node pars,
			bool forceNoOverride)
        {
            pars = RaiseEventImplementation(sender, name, pars, forceNoOverride);
        }

		private Node ExtractParsAndRaise (string name, Node pars, string parameters, object sender)
		{
			parameters = parameters.Trim ();
			if (parameters.Length != 0 && parameters.IndexOf ("[") != 0)
			{
				// This is a Reference to another Active Event
				// Extract Event, Recursively Invoke with any potential Parameters, 
				// before executing outer most Active Event
				pars = RaiseEventImplementation(sender, parameters, pars, false);
				pars = RaiseEventImplementation(sender, name, pars, false);
			}
			else
			{
				if (parameters.Length != 0)
				{
					// Traversing tree according to Expression
					object expressionValue = Expressions.GetExpressionValue(parameters, pars, pars);
					if (name == null)
						return expressionValue as Node;
					pars = RaiseEventImplementation(sender, name, (Node)expressionValue, false);
				}
				else
				{
					pars = RaiseEventImplementation(sender, name, pars, false);
				}
			}
			return pars;
		}

        private static List<string> ExtractTokens(string json)
        {
            List<string> tokens = new List<string>();
			string buffer = "";
            for (int idx = 0; idx < json.Length; idx++)
            {
                switch (json[idx])
                {
                case '&':
                case '(':
                case ')':
					if (!string.IsNullOrEmpty (buffer))
					{
						tokens.Add (buffer);
						buffer = "";
					}
                    tokens.Add(new string(json[idx], 1));
                    break;
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    break;
                default:
					buffer += json[idx];
					break;
                }
            }
			if (!string.IsNullOrEmpty (buffer))
			{
				tokens.Add (buffer);
			}
            return tokens;
        }

		private void ExecuteRemotelyEvent (string evt, Node pars, string url)
		{
            HttpWebRequest req = WebRequest.Create(url) as System.Net.HttpWebRequest;
            req.Method = "POST";
            string refer = string.Format(
                "{0}://{1}{2}",
                HttpContext.Current.Request.Url.Scheme,
                HttpContext.Current.Request.ServerVariables["HTTP_HOST"],
                (HttpContext.Current.Request.ApplicationPath.Equals("/")) ? 
                    "/" : 
                    HttpContext.Current.Request.ApplicationPath + "/").ToLowerInvariant();
            req.Referer = refer;
            req.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write("event=" + System.Web.HttpUtility.UrlEncode(evt));
                writer.Write("&params=" + System.Web.HttpUtility.UrlEncode(pars.ToJSONString()));
            }
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
                    {
                        string val = reader.ReadToEnd();
                        if (!val.StartsWith("return:"))
                            throw new Exception(
                                "Something went wrong when connecting to '" +
                                url +
                                "'. Server responded with: " + val);

                        if (val.Length > 7)
						{
							Node tmp = Node.FromJSONString(val.Substring(7));
                            pars.ReplaceChildren (tmp);
							pars.Name = tmp.Name;
							pars.Value = tmp.Value;
						}
                    }
					else
						throw new ArgumentException("Couldn't find event '" + evt + "' on " + url);
                }
            }
		}

		private void RaiseEventDoneParsing (
			object sender, 
			string name, 
			List<string> tokens, 
			Node pars,
			bool forceNoOverride)
		{
			if (!forceNoOverride && _urlMappers.ContainsKey(name))
			{
				ExecuteRemotelyEvent(name, pars, _urlMappers[name]);
				return;
			}

			// Calling single Active Event with no more tokens
			string originalName = name;
			if (!forceNoOverride)
			{
				name = GetEventMappingValue(name);
				if (name != originalName && _urlMappers.ContainsKey (name))
				{
					ExecuteRemotelyEvent(name, pars, _urlMappers[name]);
					return;
				}
			}

			ActiveEventArgs e = new ActiveEventArgs(originalName, pars);

            // We must run this in two operations since events clear controls out
            // and hence make "dead references" to Event Handlers and such...
            // Therefor we first iterate and find all event handlers interested in
            // this event before we start calling them one by one. But every time in
            // between calling the next one, we must verify that it still exists within
            // the collection...
            List<Tuple<MethodInfo, object>> tmp = SlurpAllEventHandlers(":before", forceNoOverride);

            // Looping through all methods...
            foreach (Tuple<MethodInfo, object> idx in tmp)
            {
                // Since events might load and clear controls we need to check if the event 
                // handler still exists after *every* event handler we dispatch control to...
                List<Tuple<MethodInfo, object>> recheck = SlurpAllEventHandlers(":before", forceNoOverride);

				bool exists = false;
                foreach (Tuple<MethodInfo, object> idx2 in recheck)
                {
                    if (idx.Equals(idx2))
                    {
						exists = true;
                        break;
                    }
                }
				if (exists)
                	ExecuteEventMethod(idx.Item1, idx.Item2, sender, e);
            }

			tmp = SlurpAllNullEventHandlers(name, forceNoOverride);

            tmp.AddRange(SlurpAllEventHandlers(name, forceNoOverride));

            // Looping through all methods...
            foreach (Tuple<MethodInfo, object> idx in tmp)
            {
                // Since events might load and clear controls we need to check if the event 
                // handler still exists after *every* event handler we dispatch control to...
                List<Tuple<MethodInfo, object>> recheck = SlurpAllNullEventHandlers(name, forceNoOverride);

				recheck.AddRange(SlurpAllEventHandlers(name, forceNoOverride));

				bool exists = false;
                foreach (Tuple<MethodInfo, object> idx2 in recheck)
                {
                    if (idx.Equals(idx2))
                    {
						exists = true;
                        break;
                    }
                }
				if (exists)
                	ExecuteEventMethod(idx.Item1, idx.Item2, sender, e);
            }

            tmp = SlurpAllEventHandlers(":after", forceNoOverride);

            // Looping through all methods...
            foreach (Tuple<MethodInfo, object> idx in tmp)
            {
                // Since events might load and clear controls we need to check if the event 
                // handler still exists after *every* event handler we dispatch control to...
                List<Tuple<MethodInfo, object>> recheck = SlurpAllEventHandlers(":after", forceNoOverride);

				bool exists = false;
                foreach (Tuple<MethodInfo, object> idx2 in recheck)
                {
                    if (idx.Equals(idx2))
                    {
						exists = true;
                        break;
                    }
                }
				if (exists)
                	ExecuteEventMethod(idx.Item1, idx.Item2, sender, e);
            }
		}

		private Node RaiseSingleEventWithTokens (
			object sender, 
			string name, 
			List<string> tokens, 
			Node pars,
			bool forceNoOverride)
		{
			if (tokens.Count == 0 || 
			    (tokens.Count >= 2 && tokens[0] == "(" && tokens[1] == ")"))
			{
				// Either only an event name, or an event name followed by two parantheses
				if (tokens.Count >= 2 && tokens[0] == "(" && tokens[1] == ")")
				{
					tokens.RemoveAt (0);
					tokens.RemoveAt (0);
				}
				RaiseEventDoneParsing(sender, name, tokens, pars, forceNoOverride);
				if (tokens.Count > 0)
				{
					if(tokens[0] == "&")
					{
						tokens.RemoveAt (0);
						string nextName = tokens[0];
						tokens.RemoveAt (0);
						return RaiseSingleEventWithTokens (sender, nextName, tokens, pars, false);
					}
					else
						throw new ArgumentException("Don't know how to parse parameters at end");
				}
			}
			else if (tokens.Count > 2 && 
			         tokens[0] == "(" && 
			         tokens[1] != ")" && 
			         tokens[1].IndexOf("[") != 0)
			{
				// Another Active Event inside of Parantheses
				// First raise Inner Event(s), then raise current
				tokens.RemoveAt (0);
				int parantheses = 0;
				int index = 0;
				while (true)
				{
					if(parantheses == 0 && tokens[index] == ")")
					{
						tokens.RemoveAt (index);
						break;
					}
					else if(tokens[index] == "(")
						parantheses += 1;
					else if(tokens[index] == ")")
						parantheses -= 1;
					index += 1;
				}
				string nameInner = tokens[0];
				tokens.RemoveAt (0);

				// Raising "Inner" event(s) first
				pars = RaiseSingleEventWithTokens (sender, nameInner, tokens, pars, false);

				// Then raising "this" event
				RaiseEventDoneParsing(sender, name, tokens, pars, forceNoOverride);
				if (tokens.Count > 0)
				{
					if(tokens[0] == "&")
					{
						tokens.RemoveAt (0);
						string nextName = tokens[0];
						tokens.RemoveAt (0);
						return RaiseSingleEventWithTokens (sender, nextName, tokens, pars, false);
					}
					else
						throw new ArgumentException("Don't know how to parse parameters at end");
				}
			}
			else if (tokens.Count > 2 && 
			         tokens[0] == "(" && 
			         tokens[1] != ")" && 
			         tokens[1].IndexOf("[") == 0)
			{
				// Expression inside of Parantheses
				// First parse Expression, then raise current
				tokens.RemoveAt (0);
				int parantheses = 0;
				int index = 0;
				while (true)
				{
					if(parantheses == 0 && tokens[index] == ")")
					{
						tokens.RemoveAt (index);
						break;
					}
					else if(tokens[index] == "(")
						parantheses += 1;
					else if(tokens[index] == ")")
						parantheses -= 1;
					index += 1;
				}

				object exprValue = Expressions.GetExpressionValue(tokens[0], pars, pars);

				tokens.RemoveAt (0);

				// Then raising "this" event
				RaiseEventDoneParsing(sender, name, tokens, (Node)exprValue, forceNoOverride);

				if (tokens.Count > 0)
				{
					if(tokens[0] == "&")
					{
						tokens.RemoveAt (0);
						string nextName = tokens[0];
						tokens.RemoveAt (0);
						return RaiseSingleEventWithTokens (sender, nextName, tokens, pars, false);
					}
					else
						throw new ArgumentException("Don't know how to parse parameters at end");
				}
			}
			return pars;
		}

		private Node RaiseEventImplementation (object sender, 
			string code, 
			Node pars,
		    bool forceNoOverride)
		{
			List<string> tokens = ExtractTokens(code);
			string name = tokens.Count == 0 ? "" : tokens[0];
			if (tokens.Count > 0)
				tokens.RemoveAt(0);
			return RaiseSingleEventWithTokens(sender, name, tokens, pars, forceNoOverride);
        }

        internal void AddListener(object context, MethodInfo method, string name)
        {
            if (context == null)
            {
                // Static event handler, will *NEVER* be cleared until application
                // itself is restarted
                if (!_staticEvents.ContainsKey(name))
                    _staticEvents[name] = new List<Tuple<MethodInfo, object>>();
                _staticEvents[name].Add(new Tuple<MethodInfo, object>(method, context));
            }
            else
            {
                // Request "instance" event handler, will be tossed away when
                // request is over if Viewport is correctly wired
                if (!InstanceMethod.ContainsKey(name))
                    InstanceMethod[name] = new List<Tuple<MethodInfo, object>>();
                InstanceMethod[name].Add(new Tuple<MethodInfo, object>(method, context));
            }
        }

        private Dictionary<string, List<Tuple<MethodInfo, object>>> InstanceMethod
        {
            get
            {
				if (HttpContext.Current == null)
					return new Dictionary<string, List<Tuple<MethodInfo, object>>>();// returning empty list when no page [threading ...]
                Page page = (Page)HttpContext.Current.Handler;
                if (!page.Items.Contains("__Magix.Brix.Loader.ActiveEvents._requestEventHandlers"))
                {
                    page.Items["__Magix.Brix.Loader.ActiveEvents._requestEventHandlers"] =
                        new Dictionary<string, List<Tuple<MethodInfo, object>>>();
                }
                return (Dictionary<string, List<Tuple<MethodInfo, object>>>)
                    page.Items["__Magix.Brix.Loader.ActiveEvents._requestEventHandlers"];
            }
        }
    }
}
