﻿using System;
using System.Threading;

namespace GNFS_Winforms
{
	using GNFSCore;

	public partial class GnfsUiBridge
	{
		public static GNFS FindRelations(bool oneRound, GNFS gnfs, CancellationToken cancelToken)
		{
			while (!cancelToken.IsCancellationRequested)
			{
				if (gnfs.CurrentRelationsProgress.SmoothRelationsCounter >= gnfs.CurrentRelationsProgress.Quantity)
				{
					gnfs.CurrentRelationsProgress.IncreaseQuantity(100);
				}

				gnfs.CurrentRelationsProgress.GenerateRelations(cancelToken);

				Logging.LogMessage();
				Logging.LogMessage($"Sieving progress saved at:");
				Logging.LogMessage($"   A = {gnfs.CurrentRelationsProgress.A}");
				Logging.LogMessage($"   B = {gnfs.CurrentRelationsProgress.B}");
				Logging.LogMessage();

				if (oneRound)
				{
					break;
				}

				if (gnfs.CurrentRelationsProgress.SmoothRelationsCounter >= gnfs.CurrentRelationsProgress.Quantity)
				{
					break;
				}
			}

			return gnfs;
		}
	}
}
