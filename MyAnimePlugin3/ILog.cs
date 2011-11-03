using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAnimePlugin3
{
	public interface ILog
	{
		void Write(string entry);
		void Write(string entry, params object[] args);
		void WriteMultiLine(string entry);
		void Write(string entry, int param);
		void Write(string entry, bool singleLine);
	}
}
