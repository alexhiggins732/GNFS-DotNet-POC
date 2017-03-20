﻿using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GNFSCore.PrimeSignature
{
	public class BitMatrix
	{
		public int Width;
		public BitVector[] Rows;
		ReadOnlyCollection<bool[]> Columns { get { return Enumerable.Range(0, Width).Select(i => GetColumn(i)).ToList().AsReadOnly(); } }

		public int[] RowSums { get { return Enumerable.Range(0, Rows.Length).Select(i => RowSum(i)).ToArray(); } }
		public int[] ColumnSums { get { return Enumerable.Range(0, Width).Select(i => ColumnSum(i)).ToArray(); } }

		public BitMatrix(IEnumerable<int> array, int width)
		{
			Width = width;
			Rows = array.Select(i => new BitVector(i, width)).ToArray();
			Rows = Rows.Where(bv => bv.Elements.Any(b => b)).ToArray();
			SortRows();
		}

		private int ColumnSum(int index)
		{
			return Rows.Select(bv => bv[index] ? 1 : 0).Sum();
		}

		private int RowSum(int index)
		{
			int result = Rows[index].FactorCount();
			return result;
		}

		private void SortRows()
		{
			Rows = Rows.OrderByDescending(bv => bv.IndexOfRightmostElement())
						.ThenBy(bv => bv.IndexOfLeftmostElement())
						.ThenByDescending(bv => bv.FactorCount())
						.ToArray();
		}

		private bool[] GetColumn(int index)
		{
			return Rows.Select(bv => bv[index]).ToArray();
		}

		public override string ToString()
		{
			SortRows();

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Join(",", ColumnSums));
			sb.AppendLine();
			sb.AppendLine(string.Join(Environment.NewLine, Rows.Select(i => i.ToString())));
			return sb.ToString();
		}
	}
}
