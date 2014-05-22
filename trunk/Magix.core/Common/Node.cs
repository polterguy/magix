/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;

namespace Magix.Core
{
    /*
     * Helper class to pass around data in a "JSON kind of way" without having
     * to convert to JSON strings. Create a new instance, and just start appending
     * items to it like this;
     * <pre>
     *   Node n = new Node();
     *   n["Customer"]["Name"] = "John Doe";
     *   n["Customer"]["Adr"] = "NY";
     * </pre>
     * This is at the core of Magix, being the 'protocol' we're using to pass
     * data around within the system. If you don't understand this class, you're
     * in trouble! Make sure you understand, at least roughly, what this class does
     * if you'd like to code C# for Magix
     */
    [Serializable]
    public class Node : IList<Node>
    {
        // Implementation of list
        private readonly List<Node> _children = new List<Node>();

        // Parent node
        private Node _parent;

        private string _name;
        private object _value;

        /*
         * Default CTOR, creates a new node with no name and no value and no children
         */
        public Node()
            : this(null)
        { }

        /*
         * Creates a new node with the given name
         */
        public Node(string name)
            : this(name, null)
        { }

        /*
         * Creates a new node with the given name and the given value
         */
        public Node(string name, object value)
            : this(name, value, null)
        { }

        private Node(string name, object value, Node parent)
        {
            _name = name;
            _value = value;
            _parent = parent;
        }

		public IEnumerable<Node> Children
		{
			get
			{
				foreach (Node idx in _children)
					yield return idx;
			}
		}

		/*
		 * Contains a genetically unique code you might say, according to the 
		 * node's position in the tree
		 */
		public string Dna
		{
			get
			{
				string dna = "";
				if (Parent != null)
				{
					dna = Parent.Dna + "-";
					int idxNo = 0;
					foreach (Node idx in Parent.Children)
					{
						if (idx.Equals(this))
						{
							dna += idxNo;
							break;
						}
						idxNo++;
					}
				}
				else
					dna = "root";
				return dna;
			}
		}

		/*
		 * returns the node with the given dna
		 */
		public Node FindDna(string dna)
		{
			Node tmp = RootNode();
			List<string> dnas = new List<string>(dna.Split('-'));
			dnas.RemoveAt(0);

			while (dnas.Count > 0)
			{
				int idxNo = int.Parse(dnas[0]);
				tmp = tmp[idxNo];
				dnas.RemoveAt(0);
			}
			return tmp;
		}

		/*
		 * adds a node before the this node in its parent collection, if possible
		 */
		public void AddBefore(Node node)
		{
			if (Parent == null)
				throw new ArgumentException("cannot add a node before the root node of a tree");

			string[] dnas = Dna.Split('-');
			int dnaNo = int.Parse(dnas[dnas.Length - 1]);

			Parent.Insert(dnaNo, node);
		}

		/*
		 * adds a node before the this node in its parent collection, if possible
		 */
		public void AddAfter(Node node)
		{
			if (Parent == null)
				throw new ArgumentException("cannot add a node after the root node of a tree");

			string[] dnas = Dna.Split('-');
			int dnaNo = int.Parse(dnas[dnas.Length - 1]);

			Parent.Insert(dnaNo + 1, node);
		}

		/*
		 * Compares the nodes in the "this" pointer to 
		 * see if they contain all the nodex that exists in our "prototype"
		 * Node. If so, it will return true, else false
		 */
		public bool HasNodes(Node prototype)
		{
			foreach (Node idx in prototype)
			{
				if (!Exists(
					delegate(Node idxThis)
					{
						if (idx.Name != idxThis.Name)
							return false;
						if (idx.Value != null && idxThis.Value == null)
							return false;
						if (idx.Value == null) // null returns true always
							return idxThis.HasNodes(idx);
						if (idxThis.Value == null)
							return false;
						if (idx.Value is string && idx.Get<string>().Contains("%"))
						{
							string[] parts = idx.Get<string>().Split(new char[] {'%'}, StringSplitOptions.RemoveEmptyEntries);
							int idxNo = 0;
							foreach (string idxQuery in parts)
							{
								idxNo = idxThis.Get<string>().IndexOf(idxQuery, idxNo);
								if (idxNo == -1)
									return false;
								idxNo++;
							}
						}
						else if (!idx.Value.Equals(idxThis.Value))
							return false;
						if (!idxThis.HasNodes(idx))
							return false;
						return true;
					}))
					return false;
			}
			return true;
		}

        /*
         * Will de-serialize the given JSON string into a Node structure. PS!
         * Even though Nodes can be serialized to JSON, the type information is lost,
         * meaning you can not go back again 100% correctly, since you're 'loosing
         * your types' when converting from Node to JSON. This is on our road map
         * for fixing, but currently not finished
         */
        public static Node FromJSONString(string json)
        {
            List<string> tokens = ExtractTokens(json);

            if (tokens.Count == 0)
                throw new ArgumentException("json string was empty");

            if (tokens[0] != "{")
                throw new ArgumentException("json string wasn't an object, missing opening token at character 0");

            int idxToken = 1;
            Node retVal = new Node();

            while (tokens[idxToken] != "}")
            {
                ParseToken(tokens, ref idxToken, retVal);
            }

            return retVal;
        }

        private static void ParseToken(IList<string> tokens, ref int idxToken, Node node)
        {
            if (tokens[idxToken] == "{")
            {
                // Opening new object...
                Node next = new Node();
                node._children.Add(next);
                next._parent = node;
                idxToken += 1;
                while (tokens[idxToken] != "}")
                {
                    ParseToken(tokens, ref idxToken, next);
                }
            }
            else if (tokens[idxToken] == ":")
            {
                switch(tokens[idxToken - 1])
                {
                    case "\"Name\"":
                        node.Name = tokens[idxToken + 1].Substring (1, tokens[idxToken + 1].Length - 2).Replace("\\\"", "\"");
                        break;
                    case "\"Value\"":
                        node.Value = tokens[idxToken + 1].Substring (1, tokens[idxToken + 1].Length - 2).Replace("\\\"", "\"");
                        break;
                    case "\"TypeName\"":
                        node.Value = Convert(node.Get<string>(), tokens[idxToken + 1]);
                        break;
                    case "\"Children\"":
                        idxToken += 1;
                        while (tokens[idxToken] != "]")
                        {
                            ParseToken(tokens, ref idxToken, node);
                        }
                        break;
                }
            }
            idxToken += 1;
            return;
        }

        private static object Convert(string value, string type)
        {
            object retVal = value;
            switch (type.Trim('\\').Trim('"'))
            {
                case "System.DateTime":
                    retVal = DateTime.ParseExact(value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                    break;
                case "System.Boolean":
                    retVal = bool.Parse(value);
                    break;
                case "System.Int32":
                    retVal = int.Parse(value);
                    break;
                case "System.Decimal":
                    retVal = decimal.Parse(value, CultureInfo.InvariantCulture);
                    break;
            }
            return retVal;
        }

        private static List<string> ExtractTokens(string json)
        {
            List<string> tokens = new List<string>();
            for (int idx = 0; idx < json.Length; idx++)
            {
                switch (json[idx])
                {
                    case '{':
                    case '}':
                    case ':':
                    case ',':
                    case '[':
                    case ']':
                        tokens.Add(new string(json[idx], 1));
                        break;
                    case '"':
                        {
                            string str = "\"";
                            idx += 1;
                            while (true)
                            {
                                if (json[idx] == '"' && str.Substring(str.Length - 1, 1) != "\\")
                                {
                                    break;
                                }
                                str += json[idx];
                                idx += 1;
                            }
                            str += "\"";
                            tokens.Add(str);
                        } break;
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        break;
                    default:
                        throw new ArgumentException(
                            string.Format(
                                "illegal token found in json string at character {0}, token was {1}, string was; \"{2}\"",
                                idx,
                                json[idx],
                                json.Substring(Math.Max(0, idx - 5))));
                }
            }
            return tokens;
        }

        /*
         * Returns the Parent node of the current node in the hierarchy. Normally
         * you'd never need to know the 'Parent' of a node, due to the intrinsic
         * logic of Magix, so be careful. If you're using this method, you're probably
         * doing something wrong on an architectural level. Be warned ...
         */
        public Node Parent
        {
            get { return _parent; }
        }

        /*
         * Assigns a Parent to the current node. Useful for 
         * moving nodes around
         */
        public void SetParent(Node n)
        {
            _parent = n;
        }

        /*
         * Returns the name of the node
         */
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /*
         * Returns the value of the object. Use the Get method
         * to retrieve typed objects
         */
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /*
         * Returns the value of the object to type of T. Will try to 
         * convert the value, if it is another type then asked for
         */
        public T Get<T>()
        {
            if (_value == null)
                return default(T);

            if (typeof(T) != _value.GetType())
            {
                switch (typeof(T).FullName)
                {
                    case "System.Int32":
                        return (T)(object)int.Parse(GetString(_value), CultureInfo.InvariantCulture);
                    case "System.Boolean":
                        return (T)(object)bool.Parse(GetString(_value));
                    case "System.Decimal":
                        return (T)(object)decimal.Parse(GetString(_value), CultureInfo.InvariantCulture);
                    case "System.DateTime":
                        return (T)(object)DateTime.ParseExact(GetString(_value), "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                    case "System.String":
                        return (T)(object)GetString(_value);
					default:
                        if (_value is T)
                            return (T)_value;
					    throw new ArgumentException("cannot convert node to given type; " + typeof(T).Name);
                }
            }

            return (T)_value;
        }

        private string GetString(object _value)
        {
            switch (_value.GetType().FullName)
            {
                case "System.Int32":
                    return _value.ToString();
                case "System.Boolean":
                    return _value.ToString();
                case "System.Decimal":
                    return ((decimal)_value).ToString(CultureInfo.InvariantCulture);
                case "System.DateTime":
                    return ((DateTime)_value).ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                default:
                    return _value.ToString();
            }
        }

        /*
         * Returns the value of the object to type of T, and if
         * object is null it will return 
         * the "default" value. Be careful, might throw if type is wrong
         */
        public T Get<T>(T defaultValue)
        {
            if (_value == null)
                return defaultValue;

            return Get<T>();
        }

        /*
         * Returns the first node that matches the given Predicate. Will
         * search recursively. Be careful here, if you're dereferencing nodes that
         * don't exist inside your function, you might very well never return
         * from this method ... ;)
         */
        public Node FindDescendent(Predicate<Node> functor)
        {
            foreach (Node idx in _children)
            {
				bool found = functor(idx);
				if (found)
					return idx;
				Node tmp = idx.FindDescendent(functor);
				if (tmp != null)
					return tmp;
            }
            return null;
        }

        /*
         * Returns true if node exists as a direct child only, and not search
         * recursive
         */
        public bool Exists(Predicate<Node> functor)
        {
            foreach (Node idx in this._children)
            {
                if (functor(idx))
                    return true;

            }
            return false;
        }

        /*
         * Will "disconnect" the node from its parent node. Useful
         * for removing nodes and trees out of Node structures
         */
        public Node UnTie()
        {
			if (_parent != null)
				_parent.Remove(this);
            _parent = null;
            return this;
        }

        /*
         * Returns the node with the given Name. If that node doesn't exist
         * a new node will be created with the given name and appended into the
         * Children collection and then be returned. Please notice that this
         * method CREATES NODES during DE-REFERENCING. And it is its INTENT TOO! ;)
         */
        public Node this[string name]
        {
            get
            {
                Node retVal = _children.Find(
                    delegate(Node idx)
                    {
                        return idx.Name == name;
                    });
                if (retVal == null)
                {
                    retVal = new Node(name, null, this);
                    _children.Add(retVal);
                }
                return retVal;
            }
        }

        /*
         * Returns the n'th node, with the given Name. If that node doesn't exist
         * a new node will be created with the given name and appended into the
         * Children collection and then be returned. Please notice that this
         * method CREATES NODES during DE-REFERENCING. And it is its INTENT TOO! ;)
         */
        public Node this[string name, int n]
        {
            get
            {
				int idxNo = 0;
                Node retVal = _children.Find(
                    delegate(Node idx)
                    {
						if (idx.Name == name)
						{
							if (idxNo == n)
								return true;
							idxNo ++;
						}
						return false;
                    });
                if (retVal == null)
                {
                    retVal = new Node(name, null, this);
                    _children.Add(retVal);
                }
                return retVal;
            }
        }

        /*
         * Returns the index of the given item, if it exists within the children
         * collection. Otherwise it returns -1
         */
        public int IndexOf(Node item)
        {
            return _children.IndexOf(item);
        }

        /*
         * Inserts a new item into the children collection
         */
        public void Insert(int index, Node item)
        {
			if (item == null)
				return;
            _children.Insert(index, item);
            item._parent = this;
        }

        /*
         * Removes node at given index
         */
        public void RemoveAt(int index)
        {
            _children[index]._parent = null;
            _children.RemoveAt(index);
        }

        /*
         * Returns the n'th node
         */
        public Node this[int index]
        {
            get
            {
                return _children[index];
            }
            set
            {
				if (value == null)
					return;
	            _children[index] = value;
	            value._parent = this;
            }
        }

        /*
         * Adds a new node to the collection
         */
        public void Add(Node item)
        {
			if (item == null)
				return;
            _children.Add(item);
            item._parent = this;
        }

        /*
         * Adds a new node to the collection
         */
        public Node Add(string name)
        {
			return Add(name, null);
        }

        /*
         * Adds a new node to the collection
         */
        public Node Add(string name, object val)
        {
			Add(new Node(name, val));
			return _children[_children.Count - 1];
        }

        /*
         * Adds a range of nodes to collection
         */
        public void AddRange(IEnumerable<Node> items)
        {
            foreach (Node idx in items)
            {
				if (idx != null)
				{
                	Add(idx);
					idx._parent = this;
				}
            }
        }

        /*
         * Entirely empties the collection
         */
        public void Clear()
        {
            foreach (Node idx in _children)
            {
                idx._parent = null;
            }
            _children.Clear();
        }

        /*
         * Returns true if node exists within child collection [flat]
         */
        public bool Contains(Node item)
        {
            return _children.Contains(item);
        }

        /*
         * Returns true if node exists within child collection [flat]
         */
        public bool Contains(string itemName)
        {
            return _children.Exists(
                delegate(Node idx)
                {
                    return idx.Name == itemName;
                });
        }

        /*
         * Returns true if node exists within child collection and node has value
         */
        public bool ContainsValue(string itemName)
        {
            return _children.Exists(
                delegate(Node idx)
                {
                    return idx.Name == itemName && idx.Value != null;
                });
        }

        public void CopyTo(Node[] array, int arrayIndex)
        {
            foreach (Node idx in _children)
            {
                idx._parent = null;
            }
            _children.CopyTo(array, arrayIndex);
        }

        /*
         * Returns the number of items in the children collection
         */
        public int Count
        {
            get { return _children.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /*
         * Removes the given node from the child collection
         */
        public bool Remove(Node item)
        {
            bool retVal = _children.Remove(item);

            if (retVal)
                item._parent = null;

            return retVal;
        }

        /*
         * Supports enumerating items
         */
        public IEnumerator<Node> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        /*
         * Will return name/value and number of children as a string
         */
        public override string ToString()
        {
            string retVal = "";
            retVal += _name;

            if (_value != null)
                retVal += ":" + _value;

            if (_children != null)
                retVal += ":" + _children.Count;

            return retVal;
        }

		public override int GetHashCode ()
		{
			string tmp = ToString ();
			foreach (Node idx in _children)
			{
				tmp += idx.ToString ();
			}
			return tmp.GetHashCode ();
		}

        /*
         * Will translate the Node structure to a JSON string. Useful
         * for passing stuff around to other systems, and integrating with client-side
         * etc. Be warned! No TYPE information is being passed, so you cannot build
         * the same Node structure by reversing the method and call FromJSON after
         * creating JSON out of your node
         */
        public string ToJSONString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            bool hasChild = false;

            if(!string.IsNullOrEmpty(Name))
            {
                builder.AppendFormat(@"""Name"":""{0}""", Name);
                hasChild = true;
            }
            if(Value != null)
            {
                if (hasChild)
                    builder.Append(",");
                string value = "";

                // ORDER COUNTS !!
                switch(Value.GetType().FullName)
                {
                    case "System.String":
                        value = Value.ToString().Replace("\"", "\\\"");
                        break;
                    case "System.DateTime":
                        value = ((DateTime)Value).ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "System.Decimal":
                        value = ((Decimal)Value).ToString(CultureInfo.InvariantCulture);
                        break;
                    default:
                        value = Value.ToString().Replace("\"", "\\\"");
                        break;
                }
                builder.AppendFormat(@"""Value"":""{0}""", value);
                hasChild = true;

                string typeName = Value.GetType().FullName;
                if (typeName != typeof(string).FullName)
                {
                    builder.Append(",");
                    builder.AppendFormat(@"""TypeName"":""{0}""", typeName);
                }
            }
            if(_children.Count > 0)
            {
                if (hasChild)
                    builder.Append(",");
                builder.Append(@"""Children"":[");
                bool first = true;

                foreach(Node idx in _children)
                {
                    if (!first)
                        builder.Append(",");
                    first = false;
                    builder.Append(idx.ToJSONString());
                }
                builder.Append("]");
            }
            builder.Append("}");
            return builder.ToString();
        }

        /*
         * Will sort the nodes according to your given comparison delegate
         */
        public void Sort(Comparison<Node> del)
        {
            _children.Sort(del);
        }

        /*
         * Level3 Returns the outer most parent node, the top node of the hierarchy
         */
        public Node RootNode()
        {
            Node tmp = this;
            while (tmp.Parent != null)
                tmp = tmp.Parent;
            return tmp;
        }

        /*
         * Clones the given node. Deep copy of all nodes
         */
        public Node Clone()
        {
            return Clone(this);
        }

        private static Node Clone(Node node)
        {
            Node r = new Node();
            r.Name = node.Name;
            r.Value = node.Value;
            foreach (Node idx in node)
            {
                r.Add(Clone(idx));
            }
            return r;
        }

		private bool CompareChildren(Node rhs)
		{
			if (Count != rhs.Count)
				return false;
			for (int idx = 0; idx < Count; idx++)
			{
				if (!this[idx].Equals (rhs[idx]))
					return false;
			}
			return true;
		}

		/*
		 * Returns true if the given object is equal of
		 * the this object. Equality is determined to true 
		 * if the Name, Value and all Children nodes are equal to
		 * the this object
		 */
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Node))
				return false;

			Node rhs = obj as Node;
			return Name == rhs.Name && 
				 (Value == null && rhs.Value == null || 
				   (Value != null && Value.Equals(rhs.Value))) && 
				 CompareChildren(rhs);
		}

        /*
         * returns the next node in the children collection of its parent
         */
        public Node Next()
        {
            if (Parent == null)
                throw new ArgumentException("cannot return next node of root node");

            string[] dnas = Dna.Split('-');
            int dnaNo = int.Parse(dnas[dnas.Length - 1]);

            if (Parent.Count <= dnaNo + 1)
                return null;

            return Parent[dnaNo + 1];
        }

        /*
         * returns the previous node in the children collection of its parent
         */
        public Node Previous()
        {
            if (Parent == null)
                throw new ArgumentException("cannot return previous node of root node");

            string[] dnas = Dna.Split('-');
            int dnaNo = int.Parse(dnas[dnas.Length - 1]);

            if (dnaNo == 0)
                return null;

            return Parent[dnaNo - 1];
        }
    }
}
