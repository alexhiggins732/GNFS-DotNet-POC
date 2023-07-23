﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GNFSCore
{
	using Factors;
	using IntegerMath;
	using Matrix;

	using Interfaces;

	public class Relation : IEquatable<Relation>, IEqualityComparer<Relation>
	{
		[JsonProperty(Order = 0)]
		public BigInteger A { get; protected set; }

		/// <summary>
		/// Root of f(x) in algebraic field
		/// </summary>
		[JsonProperty(Order = 1)]
		public BigInteger B { get; protected set; }

		/// <summary> ƒ(b) ≡ 0 (mod a); Calculated as: ƒ(-a/b) * -b^deg </summary>
		[JsonProperty(Order = 2)]
		public BigInteger AlgebraicNorm { get; protected set; }
		/// <summary>  a + bm </summary>
		[JsonProperty(Order = 3)]
		public BigInteger RationalNorm { get; protected set; }

		[JsonProperty(Order = 4)]
		internal BigInteger AlgebraicQuotient;
		[JsonProperty(Order = 5)]
		internal BigInteger RationalQuotient;

		[JsonProperty(Order = 6)]
		public CountDictionary AlgebraicFactorization { get; private set; }
		[JsonProperty(Order = 7)]
		public CountDictionary RationalFactorization { get; private set; }

		[JsonProperty(Order = 8)]
		public bool IsSmooth { get { return (IsRationalQuotientSmooth && IsAlgebraicQuotientSmooth); } }

		[JsonProperty(Order = 9)]
		public bool IsRationalQuotientSmooth { get { return (RationalQuotient == 1 || RationalQuotient == 0); } }

		[JsonProperty(Order = 10)]
		public bool IsAlgebraicQuotientSmooth { get { return (AlgebraicQuotient == 1 || AlgebraicQuotient == 0); } }


		[JsonIgnore]
		public bool IsPersisted { get; set; }

		public Relation()
		{
			IsPersisted = false;
			RationalFactorization = new CountDictionary();
			AlgebraicFactorization = new CountDictionary();
		}

		public Relation(GNFS gnfs, BigInteger a, BigInteger b)
			: this()
		{
			A = a;
			B = b;

			AlgebraicNorm = Normal.Algebraic(A, B, gnfs.CurrentPolynomial); // b^deg * f( a/b )
			RationalNorm = Normal.Rational(A, B, gnfs.PolynomialBase); // a + bm

			AlgebraicQuotient = BigInteger.Abs(AlgebraicNorm);
			RationalQuotient = BigInteger.Abs(RationalNorm);

			if (AlgebraicNorm.Sign == -1)
			{
				AlgebraicFactorization.Add(BigInteger.MinusOne);
			}

			if (RationalNorm.Sign == -1)
			{
				RationalFactorization.Add(BigInteger.MinusOne);
			}
		}

		/*
		public Relation(Relation relation)
		{
			this.A = relation.A;
			this.B = relation.B;
			this.AlgebraicNorm = relation.AlgebraicNorm;
			this.RationalNorm = relation.RationalNorm;
			this.AlgebraicQuotient = BigInteger.Abs(relation.AlgebraicQuotient);
			this.RationalQuotient = BigInteger.Abs(relation.RationalQuotient);
			this.AlgebraicFactorization = relation.AlgebraicFactorization;
			this.RationalFactorization = relation.RationalFactorization;
			this.IsPersisted = relation.IsPersisted;
		}

		public Relation(BigInteger a, BigInteger b, BigInteger algebraicNorm, BigInteger rationalNorm, CountDictionary algebraicFactorization, CountDictionary rationalFactorization)
		{
			A = a;
			B = b;

			AlgebraicNorm = algebraicNorm;
			RationalNorm = rationalNorm;

			AlgebraicQuotient = 1;
			RationalQuotient = 1;

			AlgebraicFactorization = algebraicFactorization;
			RationalFactorization = rationalFactorization;
		}
		*/

		public BigInteger Apply(BigInteger x)
		{
			return BigInteger.Add(A, BigInteger.Multiply(B, x));
		}

		public void Sieve(PolyRelationsSieveProgress relationsSieve)
		{
			Sieve(relationsSieve._gnfs.PrimeFactorBase.RationalFactorBase, ref RationalQuotient, RationalFactorization);

			if (IsRationalQuotientSmooth) // No sense wasting time on factoring the AlgebraicQuotient if the relation is ultimately going to be rejected anyways.
			{
				Sieve(relationsSieve._gnfs.PrimeFactorBase.AlgebraicFactorBase, ref AlgebraicQuotient, AlgebraicFactorization);
			}
		}

		static bool useArrays = true;
		private static void Sieve(IEnumerable<BigInteger> primeFactors, ref BigInteger quotientValue, CountDictionary dictionary)
		{
			if (!useArrays)
			{
				SieveHeavy(primeFactors, ref quotientValue, dictionary);
			}
			else
			{
				SieveLight(primeFactors.ToList(), ref quotientValue, dictionary);
			}
		}
		private static void SieveLight(List<BigInteger> primeFactors, ref BigInteger quotientValue, CountDictionary dictionary)
		{
			// counts and hits should be a thread safe static that can be reused to preven object instantiation for each rel.
			var counts = new int[primeFactors.Count()];

			//index to counts to avoid having loop through all factors in factors to check for counts[i]>0;
			var hits = counts.ToArray();
			//var maxQuotient = primeFactors.Max();


			int i = 0;
			int hit = 0;
			bool done = false;
			int factorCount;
			foreach (BigInteger factor in primeFactors)
			{

				if (quotientValue % factor == 0)
				{
					hits[hit++] = i;
					factorCount = 1;
					quotientValue /= factor;
					while ((quotientValue % factor == 0))
					{
						factorCount++;
						quotientValue /= factor;
					}
					done = quotientValue < factor;
					counts[i] = factorCount;
					if (done)
						break;
				}
				i++;
				if (done)
					break;
			}
			if (quotientValue > 1)
			{

				var idx = primeFactors.IndexOf(quotientValue);
				if (idx != -1)
				{
					// this should never happen.
					counts[idx]++;
					quotientValue /= primeFactors[idx];
				}
			}
			// if you still need the dictionary, convert as one time operation.
			if (quotientValue < 2)
			{
				for (i = 0; i < hit; i++)
				{
					var idx = hits[i];
					dictionary.Add(primeFactors[idx], counts[idx]);
				}
			}


		}
		private static void SieveHeavy(IEnumerable<BigInteger> primeFactors, ref BigInteger quotientValue, CountDictionary dictionary)
		{
			if (quotientValue.Sign == -1 || primeFactors.Any(f => f.Sign == -1))
			{
				throw new Exception("There shouldn't be any negative values either in the quotient or the factors");
			}

			foreach (BigInteger factor in primeFactors)
			{
				if (quotientValue == 1)
				{
					return;
				}

				if ((factor * factor) > quotientValue)
				{
					if (primeFactors.Contains(quotientValue))
					{
						dictionary.Add(quotientValue);
						quotientValue = 1;
					}
					return;
				}

				while (quotientValue != 1 && quotientValue % factor == 0)
				{
					dictionary.Add(factor);
					quotientValue = BigInteger.Divide(quotientValue, factor);
				}
			}

			/*
			if (quotientValue != 0 && quotientValue != 1)
			{
				if (FactorizationFactory.IsProbablePrime(quotientValue))
				{
					if (quotientValue < (primeFactors.Last() * 2))
					{
						dictionary.Add(quotientValue);
						quotientValue = 1;
					}
				}
			}
			*/
		}

		#region IEquatable / IEqualityComparer

		public override bool Equals(object obj)
		{
			Relation other = obj as Relation;

			if (other == null)
			{
				return false;
			}
			else
			{
				return this.Equals(other);
			}
		}

		public bool Equals(Relation x, Relation y)
		{
			return x.Equals(y);
		}

		public bool Equals(Relation other)
		{
			return (this.A == other.A && this.B == other.B);
		}

		public int GetHashCode(Relation obj)
		{
			return obj.GetHashCode();
		}

		public override int GetHashCode()
		{
			return Tuple.Create(this.A, this.B).GetHashCode();
		}

		#endregion

		public override string ToString()
		{
			return
				$"(a:{A.ToString().PadLeft(4)}, b:{B.ToString().PadLeft(2)})\t"
				+ $"[ƒ(b) ≡ 0 (mod a):{AlgebraicNorm.ToString().PadLeft(10)} (AlgebraicNorm) IsSquare: {AlgebraicNorm.IsSquare()},\ta+b*m={RationalNorm.ToString().PadLeft(4)} (RationalNorm) IsSquare: {RationalNorm.IsSquare()}]\t";
		}

	}
}
