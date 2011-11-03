using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;

namespace MyAnimePlugin3
{
	public enum SearchMode {text, t9};
	class SearchCollection
	{
		public class SearchMatch
		{
			protected internal string _text;
			public string Text
			{
				get { return _text; }
			}

			protected internal int _index;
			public int Index
			{
				get { return _index; }
			}

			protected internal int _start;
			public int Start
			{
				get { return _start; }
			}

			protected internal int _length;
			public int Length
			{
				get { return _length; }
			}

			public SearchMatch() { }

			public SearchMatch(Capture match, int index)
			{
				_text = match.Value;
				_index = index;
				_start = match.Index;
				_length = match.Length;
			}
		}

		private object _list;
		public object List
		{
			get { return _list; }
			set { _list = value; }
		}

		private string _listItemSearchProperty;
		public string ListItemSearchProperty
		{
			get { return _listItemSearchProperty; }
			set { _listItemSearchProperty = value; }
		}

		private SearchMode _mode = SearchMode.t9;
		public SearchMode Mode
		{
			get { return _mode; }
			set
			{
				_mode = value;
				_regex = null;
			}
		}

		private bool _caseSensitive = false;
		public bool CaseSensitive
		{
			get { return _caseSensitive; }
			set
			{
				_caseSensitive = value;
				_regex = null;
			}
		}

		private string _input = string.Empty;
		public string Input
		{
			get { return _input; }
			set
			{
				_input = value;
				_regex = null;
			}
		}

		private bool _startWord = true;
		public bool StartWord
		{
			get { return _startWord; }
			set
			{
				_startWord = value;
				_regex = null;
			}
		}

		private string[] t9_keymap = {
			/*0*/ @"(0|[ .,;\-:~]+)", //0 or multiple special characters
			/*1*/ @"[1]",
			/*2*/ @"[2abc]",
			/*3*/ @"[3def]",
			/*4*/ @"[4ghi]",
			/*5*/ @"[5jkl]",
			/*6*/ @"[6mno]",
			/*7*/ @"[7pqrs]",
			/*8*/ @"[8tuv]",
			/*9*/ @"[9wxyz]",
		};

		private int GetItemCountValue()
		{
			Type t = _list.GetType();
			PropertyInfo pi = t.GetProperty("Count");
			object pv = pi.GetValue(_list, null);
			return (int)pv;
		}

		private string GetItemSearchPropertyValue(int index)
		{
			object item = _list.GetType().InvokeMember("Item", BindingFlags.Default|BindingFlags.GetProperty, null, _list, new object[] { index });

			PropertyInfo pi = item.GetType().GetProperty(_listItemSearchProperty);
			object pv = pi.GetValue(item, null);
			return pv.ToString();
		}

		Regex _regex = null;
		public SearchMatch GetMatch(int start)
		{
			if (string.IsNullOrEmpty(_input))
				return null;

			if (_list == null || GetItemCountValue() <= 0)
				return null;

			//(re)build regex
			if (_regex == null)
				UpdateRegEx();

			//find first match in [start,END]
			int index = -1;
			Match match = null;
			for (index = start; index < GetItemCountValue(); index++)
			{
				match = _regex.Match(GetItemSearchPropertyValue(index));
				if (match.Success)
					break;
			}

			//find math in [BEGIN,start[
			if (match == null || !match.Success)
			{
				for (index = 0; index < start; index++)
				{
					match = _regex.Match(GetItemSearchPropertyValue(index));
					if (match.Success)
						break;
				}
			}

			//process result
			SearchMatch searchmatch = null;
			if (match != null && match.Success)
			{
				searchmatch = new SearchMatch();
				searchmatch._text = match.Groups[0].Value;
				searchmatch._index = index;
				searchmatch._start = match.Groups[0].Index;
				searchmatch._length = match.Groups[0].Length;
			}

			return searchmatch;
		}

		public bool GetMatches(int start, List<int> lstMatches, ref SearchMatch firstMatch)
		{
			if (string.IsNullOrEmpty(_input))
				return false;

			if (_list == null || GetItemCountValue() <= 0)
				return false;

			//(re)build regex
			if (_regex == null)
				UpdateRegEx();

			//find all matches in
			if (lstMatches == null)
				lstMatches = new List<int>();
			else
				lstMatches.Clear();
			firstMatch = null; //first match after start
			SearchMatch veryFirstMatch = null; //first match in list
			Match match = null;
			for (int index = 0; index < GetItemCountValue(); index++)
			{
				match = _regex.Match(GetItemSearchPropertyValue(index));
				if (match.Success)
				{
					lstMatches.Add(index);
					if (veryFirstMatch == null)
						veryFirstMatch = new SearchMatch(match.Groups[0], index);
					if (firstMatch == null && index >= start)
						firstMatch = new SearchMatch(match.Groups[0], index);
				}
			}

			if (firstMatch == null && veryFirstMatch != null)
				firstMatch = veryFirstMatch;

			return (lstMatches != null) && (lstMatches.Count > 0);
		}

		private void UpdateRegEx()
		{
			StringBuilder sbRegEx = new StringBuilder();
			if (_startWord)
				sbRegEx.Append(@"\b");
			sbRegEx.Append('(');
			if (_mode == SearchMode.t9)
			{
				foreach (char c in _input)
				{
					if (c >= '0' && c <= '9')
						sbRegEx.Append(t9_keymap[c - '0']);
					else
						sbRegEx.Append(Regex.Escape(c.ToString()));
				}
			}
			else if (_mode == SearchMode.text)
			{
				sbRegEx.Append(Regex.Escape(_input));
			}
			sbRegEx.Append(')');

			_regex = new Regex(sbRegEx.ToString(), _caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
		}
	}
}
