﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GNFSCore.IntegerMath;

namespace GNFSCore
{
	public class SquaresMethod
	{
		public BigInteger N = BigInteger.MinusOne;
		public BigInteger[] squares = new BigInteger[0];
		public List<BigInteger> combinations = new List<BigInteger>();
		public IEnumerable<BigInteger> lifted = null;


		public BigInteger Scale { get { return combinations.Max(); } }

		public SquaresMethod(BigInteger n, IEnumerable<BigInteger> smoothSquares)
		{
			combinations = new List<BigInteger>();

			N = n;
			squares = smoothSquares.ToArray();
			combinations.AddRange(squares);
		}

		public BigInteger[] Attempt(int permutations = 2)
		{
			if (lifted == null)
			{
				Permute(permutations);

				int toRaise = (int)BigInteger.Log(N, (double)Scale);

				lifted = combinations.Select(bi => BigInteger.Pow(bi, toRaise));


			}
			else
			{
				lifted = lifted.Select(bi => BigInteger.Pow(bi, 2));
			}

			return SiftFactors(lifted);
		}

		public void Permute(int permutations)
		{
			int counter = permutations;
			while (counter > 0)
			{
				Permute();
				counter--;
			}
		}

		public void Permute()
		{
			IEnumerable<BigInteger> cumulate = Combinatorics.GetCombination(combinations).Select(arr => arr.Product()).Distinct();
			combinations.AddRange(cumulate);

			//Remove duplicates
			Dedupe();
		}

		private void Dedupe()
		{
			List<BigInteger> distinct = combinations.Distinct().ToList();
			if (combinations.Count != distinct.Count)
			{
				combinations = distinct;
			}
		}

		public BigInteger[] SiftFactors(IEnumerable<BigInteger> squares)
		{
			IEnumerable<Tuple<BigInteger, BigInteger>> congruentSquares = squares.Select(bi => new Tuple<BigInteger, BigInteger>(bi, (bi % N)));
			congruentSquares = congruentSquares.Where(tup => tup.Item1 != tup.Item2);
			congruentSquares = congruentSquares.Where(tup => tup.Item2.IsSquare());

			if (congruentSquares.Any())
			{
				//congruentSquares = congruentSquares.OrderBy(tup => tup.Item1);
				IEnumerable<Tuple<BigInteger, BigInteger>> roots = congruentSquares.Select(tup => new Tuple<BigInteger, BigInteger>(tup.Item1.SquareRoot(), tup.Item2.SquareRoot()));

				List<BigInteger> plusminusRoots = new List<BigInteger>();
				plusminusRoots.AddRange(roots.Select(tup => BigInteger.Add(tup.Item1, tup.Item2)));
				plusminusRoots.AddRange(roots.Select(tup => BigInteger.Subtract(tup.Item1, tup.Item2)));

				IEnumerable<BigInteger> factors = plusminusRoots.Select(bi => GCD.FindGCD(N, bi)).Where(gcd => (gcd > 1) && (gcd != N));
				factors = factors.Distinct().OrderByDescending(bi => bi);

				if (factors.Any())
				{
					return factors.ToArray();
				}
			}

			return new BigInteger[0];
		}
	}
}
