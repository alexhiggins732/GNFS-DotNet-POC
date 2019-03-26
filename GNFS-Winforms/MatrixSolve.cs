﻿using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;

namespace GNFS_Winforms
{
	using GNFSCore;
	using GNFSCore.Matrix;
	using GNFSCore.IntegerMath;

	public partial class GnfsUiBridge
	{
		public GNFS MatrixSolveGaussian(CancellationToken cancelToken, GNFS gnfs)
		{
			Serialization.Save.Relations.Smooth.Append(gnfs); // Persist any relations not already persisted to disk


			// Because some operations clear this collection after persisting unsaved relations (to keep memory usage light)...
			// We completely reload the entire relations collection from disk.
			// This ensure that all the smooth relations are available for the matrix solving step.
			Serialization.Load.Relations.Smooth(ref gnfs);


			List<Relation> smoothRelations = gnfs.CurrentRelationsProgress.SmoothRelations.ToList();

			int smoothCount = smoothRelations.Count;

			int maxRelationsToSelect =
				PrimeFactory.GetIndexFromValue(gnfs.PrimeFactorBase.RationalFactorBaseMax)
				+ PrimeFactory.GetIndexFromValue(gnfs.PrimeFactorBase.AlgebraicFactorBaseMax)
				+ gnfs.QuadraticFactorPairCollection.Count
				+ 3;


			Logging.LogMessage($"Total relations: {smoothCount}");
			Logging.LogMessage($"MaxRelationsToSelect: {maxRelationsToSelect}");
			Logging.LogMessage($"ttl / max = {smoothCount / maxRelationsToSelect}");

			while (smoothRelations.Count >= maxRelationsToSelect)
			{

				// Randomly select n relations from smoothRelations
				List<Relation> selectedRelations = new List<Relation>();
				while (
						selectedRelations.Count < maxRelationsToSelect 
						||
						selectedRelations.Count % 2 != 0 // Force number of relations to be even
					)
				{
					int randomIndex = StaticRandom.Next(0, smoothRelations.Count);
					selectedRelations.Add(smoothRelations[randomIndex]);
					smoothRelations.RemoveAt(randomIndex);
				}

				GaussianMatrix gaussianReduction = new GaussianMatrix(gnfs, selectedRelations);
				gaussianReduction.TransposeAppend();
				gaussianReduction.Elimination();

				int number = 1;
				int solutionCount = gaussianReduction.FreeVariables.Count(b => b) - 1;
				List<List<Relation>> solution = new List<List<Relation>>();
				while (number <= solutionCount)
				{
					List<Relation> relations = gaussianReduction.GetSolutionSet(number);
					number++;

					BigInteger algebraic = relations.Select(rel => rel.AlgebraicNorm).Product();
					BigInteger rational = relations.Select(rel => rel.RationalNorm).Product();

					CountDictionary algCountDict = new CountDictionary();
					foreach (var rel in relations)
					{
						algCountDict.Combine(rel.AlgebraicFactorization);
					}

					bool isAlgebraicSquare = algebraic.IsSquare();
					bool isRationalSquare = rational.IsSquare();

					Logging.LogMessage("---");
					Logging.LogMessage($"Relations count: {relations.Count}");
					Logging.LogMessage($"(a,b) pairs: {string.Join(" ", relations.Select(rel => $"({rel.A},{rel.B})"))}");
					Logging.LogMessage($"Rational  ∏(a+mb): IsSquare? {isRationalSquare} : {rational}");
					Logging.LogMessage($"Algebraic ∏ƒ(a/b): IsSquare? {isAlgebraicSquare} : {algebraic}");
					Logging.LogMessage($"Algebraic (factorization): {algCountDict.FormatStringAsFactorization()}");

					if (isAlgebraicSquare && isRationalSquare)
					{
						solution.Add(relations);
						gnfs.CurrentRelationsProgress.AddFreeRelationSolution(relations);
					}

					if (cancelToken.IsCancellationRequested)
					{
						break;
					}
				}

				var productTuples =
					solution
						.Select(relList =>
							new Tuple<BigInteger, BigInteger>(
								relList.Select(rel => rel.AlgebraicNorm).Product(),
								relList.Select(rel => rel.RationalNorm).Product()
							)
						)
						.ToList();

				if (cancelToken.IsCancellationRequested)
				{
					break;
				}
			}

			Logging.LogMessage();

			return gnfs;
		}

	}
}
