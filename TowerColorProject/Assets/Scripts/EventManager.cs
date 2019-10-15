using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager> {
	private Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();

	public static void SetEventListener(string eventName, Action<object> listener, bool register) {
		if (register) {
			StartListening(eventName, listener);
		} else {
			StopListening(eventName, listener);
		}
	}

	public static void StartListening(string eventName, Action<object> listener) {
		Action<object> thisEvent;
		if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent)) {
			//Add more event to the existing one
			thisEvent += listener;

			//Update the Dictionary
			Instance.eventDictionary[eventName] = thisEvent;
		} else {
			//Add event to the Dictionary for the first time
			thisEvent += listener;
			Instance.eventDictionary.Add(eventName, thisEvent);
		}
	}

	public static void StopListening(string eventName, Action<object> listener) {
		Action<object> thisEvent;
		if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent)) {
			//Remove event from the existing one
			thisEvent -= listener;

			//Update the Dictionary
			Instance.eventDictionary[eventName] = thisEvent;
		}
	}

	public static void TriggerEvent(string eventName, object parameter = null) {
		Action<object> thisEvent = null;
		if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent)) {
			thisEvent.Invoke(parameter);
			// OR USE instance.eventDictionary[eventName]();
		}
	}
}

public class EventList
{
	public static string OnGamePhaseChanged = "OnGamePhaseChanged";
	public static string OnShotFired = "OnShotFired";
	public static string OnDrawNewBall = "OnDrawNewBall";
	public static string OnBlocDestroyedFromHit = "OnBlocDestroyedFromHit";
	public static string OnBlocDestroyed = "OnBlocDestroyed";
	public static string OnScoreUpdated = "OnScoreUpdated";
	public static string OnComboCountUpdated = "OnComboCountUpdated";
	public static string OnFailTimerUpdate = "OnFailTimerUpdate";
	public static string OnMaxComboCountShow = "OnMaxComboCountShow";
}