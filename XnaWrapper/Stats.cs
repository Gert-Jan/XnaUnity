using System;
using System.Collections.Generic;
using UnityEngine;

namespace XnaWrapper
{
	public static class Stats
	{
		const string Global = "_G";

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
				average = array.AverageSeconds;
				highest = array.HighestSeconds;
				lowest = array.LowestSeconds;
				p_average = p_highest = p_lowest = 1;
			}

			internal Info(CircularArray array, Info globalInfo)
			{
				average = array.AverageSeconds;
				highest = array.HighestSeconds;
				lowest = array.LowestSeconds;
				p_average = average / globalInfo.average;
				p_highest = highest / globalInfo.highest;
				p_lowest = lowest / globalInfo.lowest;
			}
			
			string ToPercent(float p_value)
			{
				return (p_average * 100).ToString("F1") + '%';
            }

			public string ToFPS()
			{
				return string.Format("FPS: {0} ( best:{1} / worst:{2} )", 1 / average, 1 / lowest, 1 / highest);
			}

			public string ToPercent()
			{
				return string.Format("%: {0} ( best:{1} / worst:{2} )", ToPercent(p_average), ToPercent(p_lowest), ToPercent(p_highest));
			}

			public string ToMS()
			{
				return string.Format("MS: {0} ( best:{1} / worst:{2} )", average * 1000, lowest * 1000, highest * 1000);
			}
		}
		
		internal struct CircularArray
		{
			const int amountTracked = 120;

			int currentTracked;
			int mostRecent;
			Event[] array;
			int calls;
			int ignored;

			public CircularArray(Event firstValue)
			{
				ignored = 0;
				mostRecent = 0;
				calls = 0;
				currentTracked = 0;
				array = new Event[amountTracked];
				array[mostRecent] = firstValue;
			}

			public void Add(Event newValue)
			{
				++mostRecent;
				if (mostRecent >= amountTracked)
					mostRecent = 0;
				array[mostRecent] = newValue;
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

			public int Calls { get { return calls; } }

			public float AverageSeconds
			{
				get
				{
					float total = 0;
					for (int i = 0; i < currentTracked; ++i)
						total += array[i].deltaS;
					return total / currentTracked;
				}
			}

			public float HighestSeconds
			{
				get
				{
					float highest = array[0].deltaS;
					for (int i = 1; i < currentTracked; ++i)
					{
						float element = array[i].deltaS;
						if (element > highest)
							highest = element;
                    }
					return highest;
				}
			}

			public float LowestSeconds
			{
				get
				{
					float lowest = array[0].deltaS;
					for (int i = 1; i < currentTracked; ++i)
					{
						float element = array[i].deltaS;
						if (element < lowest)
							lowest = element;
					}
					return lowest;
				}
			}
		}

		internal struct Event
		{
			public int calls;
			public float deltaS;
		}

		static Dictionary<string, CircularArray> previousEvents = new Dictionary<string, CircularArray>();
		static Dictionary<string, Event> completedEvents = new Dictionary<string, Event>();
		static Dictionary<string, float> activeEvents = new Dictionary<string, float>();
		
		public static void BeginGlobal()
		{
			Begin(Global);
        }

		public static void EndGlobal()
		{
			End(Global);
		}

		public static void Begin(string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
				eventName = completedEvents.Count.ToString();
			
			activeEvents[eventName] = Time.realtimeSinceStartup;
		}

		public static void End(string eventName)
		{
			float startTime;
			if (activeEvents.TryGetValue(eventName, out startTime))
			{
				float deltaS = (Time.realtimeSinceStartup - startTime);

				Event e;
				if (completedEvents.TryGetValue(eventName, out e))
					e.calls++;
				else
					e.calls = 1;
				e.deltaS += deltaS;
				completedEvents[eventName] = e;

				activeEvents.Remove(eventName);
            }
		}

		public static void Update()
		{
			List<string> ignoredKeys = new List<string>();
			foreach (KeyValuePair<string, CircularArray> pair in previousEvents)
			{
				if (pair.Value.IsIgnored)
					ignoredKeys.Add(pair.Key);
				else
					pair.Value.Ignore();
            }
			foreach (string key in ignoredKeys)
				previousEvents.Remove(key);
			foreach (KeyValuePair<string, Event> pair in completedEvents)
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

			if (activeEvents.Count != 0)
			{
				string events = "";
				foreach (string key in activeEvents.Keys)
					events += key + '|';
				events = events.Substring(0, events.Length - 1);
				throw new Exception("Mismanaged statistics: " + events); // happens when begin() and end() don't correspond
			}
		}
		
		public static void GetInfo(Dictionary<string, Info> outTable)
		{
			Info globalInfo = new Info(previousEvents[Global]);

			foreach (KeyValuePair<string, CircularArray> pair in previousEvents) {
				outTable.Add(pair.Key, new Info(pair.Value, globalInfo));
			}

			previousEvents.Clear();
		}
	}
}
