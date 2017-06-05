using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// TST vs. hashing
/// 
/// Hashing.
/// * Need to examine entire key
/// * Search hits and misses cost about the same
/// * Performance relies on hash function
/// * Does not support ordered symbol table operations
/// 
/// TSTs.
/// * Works only for strings (or digital keys)
/// * Only examines just enough key characters
/// * Search miss may involve only a few characters
/// * Supports ordered symbol table operations (plus others!)
/// 
/// Bottom line. TSTs are:
/// * Faster than hashing (especially for search misses);
/// * More flexible than red-black BSTs
/// 
/// - Robert Sedgewick: Algorithms
/// </summary>
namespace DoubleFile
{
    internal class TernarySearchTrie<Value>
    {
        public bool Contains(string key)
            => Get(key) != null;

        public void Put(string key, Value val)
        {
            root = Put(root, Split(key), val, 0);
        }

        public Value Get(string key)
        {
            Node x = Get(root, Split(key), 0);

            if (x == null) return default(Value);
            return x.value;
        }

        public void Delete(string key)
        {
        }

        //public IEnumerable<string> Keys()
        //{
        //    Node node = root;
        //    Node parent = null;

        //    while (null != node)
        //    {
        //        string ret = "";

        //        NextKey(ref ret, ref node, ref parent);
        //        yield return ret;
        //    }
        //}

        //private bool NextKey(ref string key, ref Node node, ref Node parent)
        //{
        //    if (null == node) return false;
        //    key += node.c;
        //    parent = node;
        //    if (null != node.left) node = node.left;
        //    else if (null != node.mid) node = node.mid;
        //    else if (null != node.right) node = node.right;
        //    else return true;

        //    if (EqualityComparer<Value>.Default.Equals(node.value, default(Value)))
        //    {
        //        return true;
        //    }

        //    return NextKey(ref key, ref node, ref parent);
        //}

        //public IEnumerable<string> KeysWithPrefix(string s)
        //{
        //}

        //public IEnumerable<string> KeysThatMatch(string s)
        //{
        //}

        //public string LongestPrefixOf(string query) =>
        //    query.Substring(0, Search(root, query, 0, 0));

        private Node Get(Node x, IList<string> split, int d)
        {
            if (x == null) return null;

            string str = split[d];

            int cmp = str.LocalCompare(x.str);

            if (0 > cmp)
                return Get(x.left, split, d);
            else if (0 < cmp)
                return Get(x.right, split, d);
            else if (d < split.Count - 1)
                return Get(x.mid, split, d + 1);
            else
                return x;
        }

        private Node Put(Node x, IList<string> split, Value val, int d)
        {
            string str = split[d];

            if (x == null) {
                x = new Node()
                {
                    str = str
                };
            }

            int cmp = str.LocalCompare(x.str);

            if (0 > cmp)
                x.left = Put(x.left, split, val, d);
            else if (0 < cmp)
                x.right = Put(x.right, split, val, d);
            else if (d < split.Count - 1)
                x.mid = Put(x.mid, split, val, d + 1);
            else
                x.value = val;

            return x;
        }

        //private int Search(Node start, Node x, string query, int d, int length)
        //{
        //    if (x == null) return length;
        //    if (x.starts?.Contains(start) ?? false) length = d;
        //    if (d == query.Length) return length;
        //    return Search(x.next[query[d]], query, d + 1, length);
        //}

        private IList<string> Split(string key) =>
            key.Split(new[] { '/', '\\' });

        private class Node
        {
            public Value value;
            public string str;
            public Node left, mid, right;
//            public IEnumerable<Node> starts;
        }

        private Node root = new Node();
    }
}
