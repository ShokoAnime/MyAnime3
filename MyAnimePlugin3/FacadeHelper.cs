using System;
using System.Collections.Generic;
using System.Text;

using BinaryNorthwest;
using MyAnimePlugin3.ViewModel;


namespace MyAnimePlugin3
{
	public class FacadeHelper
	{

		public delegate void ForEachOperation<T>(T element, int currIndex);

		/// <summary>
		/// Performs an operation for each element in the list, by starting with a specific index and working its way around it (eg: n, n+1, n-1, n+2, n-2, ...)
		/// </summary>
		/// <typeparam name="T">The Type of elements in the IList</typeparam>
		/// <param name="elements">All elements, this value cannot be null</param>
		/// <param name="startElement">The starting point for the operation (0 operates like a traditional foreach loop)</param>
		/// <param name="operation">The operation to perform on each element</param>
		public static void ProximityForEach<T>(IList<T> elements, int startElement, ForEachOperation<T> operation)
		{
			if (elements == null)
				throw new ArgumentNullException("elements");
			if ((startElement >= elements.Count && elements.Count > 0) || startElement < 0)
				throw new ArgumentOutOfRangeException("startElement", startElement, "startElement must be > 0 and <= elements.Count (" + elements.Count + ")");
			if (elements.Count > 0)                                      // if empty list, nothing to do, but legal, so not an exception
			{
				T item;
				for (int lower = startElement, upper = startElement + 1; // start with the selected, and then go down before going up
					 upper < elements.Count || lower >= 0;               // only exit once both ends have been reached
					 lower--, upper++)
				{
					if (lower >= 0)                                      // are lower elems left?
					{
						item = elements[lower];
						operation(item, lower);
						elements[lower] = item;
					}
					if (upper < elements.Count)                          // are higher elems left?
					{
						item = elements[upper];
						operation(item, upper);
						elements[upper] = item;
					}
				}
			}
		}

        


		public static List<GroupFilterVM> GetGroupFilters()
		{
			return GroupFilterHelper.AllGroupFilters;
		}
	}
}
