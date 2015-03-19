﻿/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/
#define MULTI_THREAD

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using MatterHackers.Agg;
using MatterHackers.VectorMath;

namespace MatterHackers.GCodeVisualizer
{
    public abstract class GCodeFile
    {
		static readonly Vector4 VelocitySameAsStopMmPerS = new Vector4(8, 8, .4, 5);
		static readonly Vector4 MaxAccelerationMmPerS2 = new Vector4(1000, 1000, 100, 5000);
		static readonly Vector4 MaxVelocityMmPerS = new Vector4(500, 500, 5, 25);

#if	__ANDROID__
		protected const int Max32BitFileSize = 10000000; // 10 megs
#else
		//protected const int Max32BitFileSize = 50; // 50 bytes
		protected const int Max32BitFileSize = 100000000; // 100 megs
#endif

		static bool RunningIn32Bit()
		{
			if (IntPtr.Size == 4)
			{
				return true;
			}

			return false;
		}

		public static void AssertDebugNotDefined()
		{
#if DEBUG
			throw new Exception("DEBUG is defined and should not be!");
#endif
		}

		public abstract double TotalSecondsInPrint { get; }
		public abstract int Count { get; }
		public abstract void Insert(int indexToStartInjection, PrinterMachineInstruction printerMachineInstruction);
		public abstract void Add(PrinterMachineInstruction printerMachineInstruction);
		public abstract void Clear();
		public abstract Vector2 GetWeightedCenter();
		public abstract RectangleDouble GetBounds();
		public abstract double GetFilamentCubicMm(double p);
		public abstract bool IsExtruding(int i);
		public abstract double GetLayerHeight();
		public abstract double GetFirstLayerHeight();
		public abstract double GetFilamentUsedMm(double p);
		public abstract int GetInstructionIndexAtLayer(int layerIndex);
		public abstract double GetFilamentDiamter();
		public abstract double GetFilamentWeightGrams(double p, double density);
		public abstract int GetLayerIndex(int instructionIndex);
		public abstract int NumChangesInZ { get; }
		public abstract PrinterMachineInstruction Instruction(int i);
		public abstract double Ratio0to1IntoContainedLayer(int instructionIndex);
		public abstract double PercentComplete(int instructionIndex);
		
		public static bool FileTooBigToLoad(string fileName)
		{
			if (File.Exists(fileName)
				&& RunningIn32Bit())
			{
				FileInfo info = new FileInfo(fileName);
				// Let's make sure we can load a file this big
				if (info.Length > Max32BitFileSize)
				{
					// It is too big to load
					return true;
				}
			}

			return false;
		}

		protected static double GetSecondsThisLine(Vector3 deltaPositionThisLine, double deltaEPositionThisLine, double feedRateMmPerMin)
		{
			double startingVelocityMmPerS = VelocitySameAsStopMmPerS.x;
			double endingVelocityMmPerS = VelocitySameAsStopMmPerS.x;
			double maxVelocityMmPerS = Math.Min(feedRateMmPerMin / 60, MaxVelocityMmPerS.x);
			double acceleration = MaxAccelerationMmPerS2.x;
			double lengthOfThisMoveMm = Math.Max(deltaPositionThisLine.Length, deltaEPositionThisLine);

			double distanceToMaxVelocity = GetDistanceToReachEndingVelocity(startingVelocityMmPerS, maxVelocityMmPerS, acceleration);
			if (distanceToMaxVelocity <= lengthOfThisMoveMm / 2)
			{
				// we will reach max velocity then run at it and then decelerate
				double accelerationTime = GetTimeToAccelerateDistance(startingVelocityMmPerS, distanceToMaxVelocity, acceleration) * 2;
				double runningTime = (lengthOfThisMoveMm - (distanceToMaxVelocity * 2)) / maxVelocityMmPerS;
				return accelerationTime + runningTime;
			}
			else
			{
				// we will accelerate to the center then decelerate
				double accelerationTime = GetTimeToAccelerateDistance(startingVelocityMmPerS, lengthOfThisMoveMm / 2, acceleration) * 2;
				return accelerationTime;
			}
		}

		static double GetTimeToAccelerateDistance(double startingVelocityMmPerS, double distanceMm, double accelerationMmPerS2)
		{
			// d = vi * t + .5 * a * t^2;
			// t = (√(vi^2+2ad)-vi)/a
			double startingVelocityMmPerS2 = startingVelocityMmPerS * startingVelocityMmPerS;
			double distanceAcceleration2 = 2 * accelerationMmPerS2 * distanceMm;
			return (Math.Sqrt(startingVelocityMmPerS2 + distanceAcceleration2) - startingVelocityMmPerS) / accelerationMmPerS2;
		}

		static double GetDistanceToReachEndingVelocity(double startingVelocityMmPerS, double endingVelocityMmPerS, double accelerationMmPerS2)
		{
			double endingVelocityMmPerS2 = endingVelocityMmPerS * endingVelocityMmPerS;
			double startingVelocityMmPerS2 = startingVelocityMmPerS * startingVelocityMmPerS;
			return (endingVelocityMmPerS2 - startingVelocityMmPerS2) / (2.0 * accelerationMmPerS2);
		}

		public static GCodeFile Load(string fileName)
		{
			if (FileTooBigToLoad(fileName))
			{
				return new GCodeFileStreamed(fileName);
			}
			else
			{
				return new GCodeFileLoaded(fileName);
			}
		}

		public static int CalculateChecksum(string commandToGetChecksumFor)
		{
			int checksum = 0;
			if (commandToGetChecksumFor.Length > 0)
			{
				checksum = commandToGetChecksumFor[0];
				for (int i = 1; i < commandToGetChecksumFor.Length; i++)
				{
					checksum ^= commandToGetChecksumFor[i];
				}
			}
			return checksum;
		}

		public static bool GetFirstStringAfter(string stringToCheckAfter, string fullStringToLookIn, string separatorString, ref string nextString, int startIndex = 0)
		{
			int stringPos = fullStringToLookIn.IndexOf(stringToCheckAfter, startIndex);
			if (stringPos != -1)
			{
				int separatorPos = fullStringToLookIn.IndexOf(separatorString, stringPos);
				if (separatorPos != -1)
				{
					nextString = fullStringToLookIn.Substring(stringPos + stringToCheckAfter.Length, separatorPos - (stringPos + stringToCheckAfter.Length));
					return true;
				}
			}

			return false;
		}

		public static bool GetFirstNumberAfter(string stringToCheckAfter, string stringWithNumber, ref double readValue, int startIndex = 0)
		{
			int stringPos = stringWithNumber.IndexOf(stringToCheckAfter, startIndex);
			if (stringPos != -1)
			{
				stringPos += stringToCheckAfter.Length;
				readValue = agg_basics.ParseDouble(stringWithNumber, ref stringPos);

				return true;
			}

			return false;
		}

		public static string ReplaceNumberAfter(char charToReplaceAfter, string stringWithNumber, double numberToPutIn)
		{
			int charPos = stringWithNumber.IndexOf(charToReplaceAfter);
			if (charPos != -1)
			{
				int spacePos = stringWithNumber.IndexOf(" ", charPos);
				if (spacePos == -1)
				{
					string newString = string.Format("{0}{1:0.#####}", stringWithNumber.Substring(0, charPos + 1), numberToPutIn);
					return newString;
				}
				else
				{
					string newString = string.Format("{0}{1:0.#####}{2}", stringWithNumber.Substring(0, charPos + 1), numberToPutIn, stringWithNumber.Substring(spacePos));
					return newString;
				}
			}

			return stringWithNumber;
		}
	}
}
