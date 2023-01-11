// MinGyu Jung
// September, 9th, 2022

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        //Dictionary for dependents
        private Dictionary<string, HashSet<string>> dependentDic;
        //Dictionary for dependees
        private Dictionary<string, HashSet<string>> dependeeDic;
        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {

            dependentDic = new Dictionary<string, HashSet<string>>();

            dependeeDic = new Dictionary<string, HashSet<string>>();

        }

        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            // Reference: https://stackoverflow.com/questions/11954608/count-values-in-dictionary-using-linq-and-linq-extensions
            get { return dependentDic.Values.Sum(list => list.Count); }
        }

        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get
            {
                if (dependeeDic.ContainsKey(s))
                {
                    return dependeeDic[s].Count;
                }

                return 0;
            }
        }

        /// <summary>
        ///  Reports whether dependents(s) is non-empty.
        /// </summary>
        /// <param name="s"> checking string in dependentsDic </param>
        /// <returns> if Dependents does not contain string s, return false.  </returns>
        public bool HasDependents(string s)
        {
            return dependentDic.ContainsKey(s) && dependentDic[s].Count > 0;
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        /// <param name="s"> checking string in dependeesDic.  </param>
        /// <returns> if Dependees does not contain string s, return false.  </returns>
        public bool HasDependees(string s)
        {
            return dependeeDic.ContainsKey(s) && dependeeDic[s].Count > 0;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// If a node does not exist in the graph, 
        /// it is considered to have an empty set 
        /// of dependents and dependees.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (dependentDic.ContainsKey(s))
            {
                return dependentDic[s];
            }

            return new HashSet<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// If a node does not exist in the graph, 
        /// it is considered to have an empty set 
        /// of dependents and dependees.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (dependeeDic.ContainsKey(s))
            {
                return dependeeDic[s];
            }
            return new HashSet<string>();
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            if (!dependentDic.ContainsKey(s))
            {
                dependentDic.Add(s, new HashSet<string>() { t });
            }
            else
            {
                dependentDic[s].Add(t);
            }

            if (!dependeeDic.ContainsKey(t))
            {
                dependeeDic.Add(t, new HashSet<string>() { s });
            }
            else
            {
                dependeeDic[t].Add(s);
            }
        }

        /// <summary>   e
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if (dependentDic.ContainsKey(s))
            {
                dependentDic[s].Remove(t);
            }

            if (dependeeDic.ContainsKey(t))
            {
                dependeeDic[t].Remove(s);
            }

        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            foreach (string targetVal in GetDependents(s))
            {
                RemoveDependency(s, targetVal);
            }
            foreach (string newVal in newDependents)
            {
                AddDependency(s, newVal);
            }
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            foreach (string targetVal in GetDependees(s))
            {
                RemoveDependency(targetVal, s);
            }

            foreach (string newVal in newDependees)
            {
                AddDependency(newVal, s);
            }
        }

    }

}