using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XnaWrapper
{
	public static class Stats
	{
		const float TimeScalar = 1000000.0f;
		const float MilliSecondsScalarInv = 1000.0f / TimeScalar;
		const float kBScalar = 1.0f / 1000.0f;

		public struct Info
		{
			public float average;
			public float highest;
			public float lowest;
			public float p_average;
			public float p_highest;
			public float p_lowest;

			internal Info(CircularArray array)
			{
				average = array.Average;
				highest = array.Highest;
				lowest = array.Lowest;
				p_average = p_highest = p_lowest = 1;
			}

			internal Info(CircularArray array, Info globalInfo)
			{
				average = array.Average;
				highest = array.Highest;
				lowest = array.Lowest;
				p_average = average / globalInfo.average;
				p_highest = highest / globalInfo.highest;
				p_lowest = lowest / globalInfo.lowest;
			}
			
			string ToPercent(float p_value)
			{
				return (p_average * 100).ToString("F1") + '%';
            }

			public override string ToString()
			{
				return average + " ( best:" + lowest + " / worst:" + highest + " )";
            }

			public string ToMs()
			{
				return (average * MilliSecondsScalarInv) + "ms ( best:" + (lowest * MilliSecondsScalarInv) + " / worst:" + (highest * MilliSecondsScalarInv) + " )";
			}

			public string ToKB()
			{
				return (average * kBScalar) + "kB ( best:" + (lowest * kBScalar) + " / worst:" + (highest * kBScalar) + " )";
			}

			public string ToPercent()
			{
				return string.Format("%: {0} ( best:{1} / worst:{2} )", ToPercent(p_average), ToPercent(p_lowest), ToPercent(p_highest));
			}
		}
		
		internal struct CircularArray
		{
			const int amountTracked = 120;

			int currentTracked;
			int mostRecent;
			uint[] valueArray;
			int ignored;

			public CircularArray(uint firstValue)
			{
				ignored = 0;
				mostRecent = 0;
				currentTracked = 0;
				valueArray = new uint[amountTracked];
				valueArray[mostRecent] = firstValue;
			}

			public void Add(uint newValue)
			{
				++mostRecent;
				if (mostRecent >= amountTracked)
					mostRecent = 0;
				valueArray[mostRecent] = newValue;
				ignored = 0;

				if (currentTracked >= amountTracked)
					currentTracked = amountTracked;
				else
					++currentTracked;
            }

			public void Ignore()
			{
				++ignored;
            }
			
			public bool IsIgnored { get{ return ignored > amountTracked; } }
			
			public uint Average
			{
				get
				{
					double total = 0;
					for (int i = 0; i < currentTracked; ++i)
						total += valueArray[i];
					return (uint)(total / currentTracked);
				}
			}

			public uint Highest
			{
				get
				{
					uint highest = valueArray[0];
					for (int i = 1; i < currentTracked; ++i)
					{
						uint element = valueArray[i];
						if (element > highest)
							highest = element;
                    }
					return highest;
				}
			}

			public uint Lowest
			{
				get
				{
					uint lowest = valueArray[0];
					for (int i = 1; i < currentTracked; ++i)
					{
						uint element = valueArray[i];
						if (element < lowest)
							lowest = element;
					}
					return lowest;
				}
			}
		}
		
		static Dictionary<string, CircularArray> previousTimeEvents = new Dictionary<string, CircularArray>();
		static Dictionary<string, uint> completedTimeEvents = new Dictionary<string, uint>();
		static Dictionary<string, uint> activeTimeEvents = new Dictionary<string, uint>();

		static Dictionary<string, CircularArray> previousMonoEvents = new Dictionary<string, CircularArray>();
		static Dictionary<string, uint> completedMonoEvents = new Dictionary<string, uint>();
		static Dictionary<string, uint> activeMonoEvents = new Dictionary<string, uint>();
		
		// tracks microseconds
		public const uint TRACKER_TIME = 0;
		// tracks mono heap
		public const uint TRACKER_MONO = 1;

		public static readonly uint TRACKER_COUNT;
		public static readonly Dictionary<uint, string> TRACKER_NAMES = new Dictionary<uint, string>();
		private static readonly Dictionary<uint, Tracker> trackers = new Dictionary<uint, Tracker>();

		static Stats()
		{
			AddTracker(TRACKER_TIME, "Time", GetTime);
			AddTracker(TRACKER_MONO, "Mono memory", GetMono);
			
			TRACKER_COUNT = (uint)trackers.Count;
		}

		private static uint GetTime()
		{
			return (uint)(Time.realtimeSinceStartup * TimeScalar);
		}

		private static uint GetMono()
		{
			return Profiler.GetMonoUsedSize();
		}

		private static void AddTracker(uint id, string trackerName, Func<uint> valueGetterFunction)
		{
			trackers[id] = new Tracker(id, valueGetterFunction);
			TRACKER_NAMES[id] = trackerName;
		}

		private class Tracker
		{
			readonly Func<uint> valueGetterFunction;
			readonly uint trackedValue;

			Dictionary<string, CircularArray> previousEvents = new Dictionary<string, CircularArray>();
			Dictionary<string, uint> completedEvents = new Dictionary<string, uint>();
			Dictionary<string, uint> activeEvents = new Dictionary<string, uint>();
			List<string> ignoredKeys = new List<string>();

			CircularArray previousGlobals = new CircularArray(0);
			uint globalValue;

			public Tracker(uint trackedValue, Func<uint> valueGetterFunction)
			{
				this.trackedValue = trackedValue;
                this.valueGetterFunction = valueGetterFunction;
			}

			public void Begin(ref string eventName)
			{
				activeEvents[eventName] = valueGetterFunction();
			}
			
			public void End(ref string eventName)
			{
				uint start;
				if (activeEvents.TryGetValue(eventName, out start))
				{
					uint newValue = valueGetterFunction();
					if (newValue < start)
					{
						// must have reset somewhere, so don't record
					}
					else
					{

						uint value;
						if (!completedEvents.TryGetValue(eventName, out value))
							value = 0;
						value += newValue - start;
						completedEvents[eventName] = value;
					}
					activeEvents.Remove(eventName);
				}
			}
			
			public void Update()
			{
				uint newGlobal = valueGetterFunction();
				if (newGlobal >= globalValue)
					previousGlobals.Add(newGlobal - globalValue);

				foreach (KeyValuePair<string, CircularArray> pair in previousEvents)
				{
					if (pair.Value.IsIgnored)
						ignoredKeys.Add(pair.Key);
					else
						pair.Value.Ignore();
				}
				foreach (string key in ignoredKeys)
					previousEvents.Remove(key);
				foreach (KeyValuePair<string, uint> pair in completedEvents)
				{
					CircularArray array;
					if (previousEvents.TryGetValue(pair.Key, out array))
					{
						array.Add(pair.Value);
						previousEvents[pair.Key] = array;
					}
					else
						previousEvents[pair.Key] = new CircularArray(pair.Value);
				}
				completedEvents.Clear();
				ignoredKeys.Clear();

				// check mismanagement
				if (activeEvents.Count != 0)
				{
					string events = "";
					foreach (string key in activeEvents.Keys)
						events += key + '|';
					events = events.Substring(0, events.Length - 1);
					throw new Exception("Mismanaged \"" + TRACKER_NAMES[trackedValue] + "\" statistics: " + events);// happens when begin() and end() don't correspond
				}
				globalValue = newGlobal;
			}

			public void GetInfoAsText(StringBuilder builder, Func<Info, string> toStringFunc)
			{
				Info globalInfo = new Info(previousGlobals);
				builder.Append(TRACKER_NAMES[trackedValue]);
				builder.Append(": ");
				builder.Append(toStringFunc(globalInfo));
				builder.AppendLine(); 
				foreach (KeyValuePair<string, CircularArray> pair in previousEvents)
				{
					Info info = new Info(pair.Value, globalInfo);
					builder.Append("   ");
					builder.Append(pair.Key);
					builder.Append(": ");
					builder.Append(toStringFunc(info));
					builder.AppendLine();
				}
				previousEvents.Clear();
			}

		}

		public static void Begin(uint trackerId, string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
				throw new ArgumentNullException("Measured block must be named");

			trackers[trackerId].Begin(ref eventName);
		}

		public static void End(uint trackerId, string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
				throw new ArgumentNullException("Measured block must be named");

			trackers[trackerId].End(ref eventName);
		}

		public static void Update()
		{
			foreach (Tracker tracker in trackers.Values)
				tracker.Update();
		}

		public static void GetInfoAsText(uint trackedValue, StringBuilder builder, Func<Info, string> toStringFunc)
		{
			trackers[trackedValue].GetInfoAsText(builder, toStringFunc);
		}
	}
}
