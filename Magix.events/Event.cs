/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Helper class for serializing and de-serializing events into persistent
	 * storage for later retrieval
	 */
	public class Event
	{
		private string _key;
		private Node _node;
		private bool _remotable;

		public Event(Node node, string key, bool remotable)
		{
			_node = node;
			_key = key;
			_remotable = remotable;
		}

		public string Key
		{
			get { return _key; }
			set { _key = value; }
		}

		public Node Node
		{
			get { return _node; }
			set { _node = value; }
		}

		public bool Remotable
		{
			get { return _remotable; }
			set { _remotable = value; }
		}
	}
}

