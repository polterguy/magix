/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 * However, this file, and the project files within this project,
 * as a whole is GPL, since it is linking towards Db4o, which is
 * being consumed as GPL
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Helper class for serializing and de-serializing objects back and forth too
	 * data-storage ...
	 */
	public class Storage
	{
		private string _key;
		private Node _node;

		public Storage(Node node, string key)
		{
			_node = node;
			_key = key;
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

		public override bool Equals (object obj)
		{
			if (obj == null || !(obj is Storage))
				return false;

			Storage rhs = obj as Storage;
			return Key == rhs.Key && Node.Equals (rhs.Node);
		}

		public override int GetHashCode ()
		{
			return Key.GetHashCode () + Node.GetHashCode ();
		}
	}
}

