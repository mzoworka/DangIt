﻿using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ippo
{
	[RequireComponent(typeof(AudioSource))]
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class AlarmManager : MonoBehaviour
	{
		public Dictionary<FailureModule, int> loops;

		public void Start()
		{
			print("[DangIt] [AlarmManager] Starting...");
			print("[DangIt] [AlarmManager] Setting Volume...");
            //this.GetComponent<AudioSource>().panLevel = 0f; //This disable the game scaling volume with distance from source
            this.GetComponent<AudioSource>().volume = 1f;
            
			print ("[DangIt] [AlarmManager] Creating Clip");
            this.GetComponent<AudioSource>().clip = GameDatabase.Instance.GetAudioClip("DangIt/Sounds/alarm"); //Load alarm sound

			print ("[DangIt] [AlarmManager] Creating Dictionary");
			this.loops=new Dictionary<FailureModule, int>(); //Reset counter, so on logic pass we play it
		}

		public void UpdateSettings(){
			float scaledVolume = DangIt.Instance.CurrentSettings.AlarmVolume / 100f;
			print ("[DangIt] [AlarmManager] Rescaling Volume (at UpdateSettings queue)..., now at " + scaledVolume);
            this.GetComponent<AudioSource>().volume = scaledVolume;
		}

		public void AddAlarm(FailureModule fm, int number)
		{
            this.GetComponent<AudioSource>().volume = DangIt.Instance.CurrentSettings.GetMappedVolume(); //This seems like an OK place for this, because if I put it in the constructor...
			                                                                       // ...you would have to reboot to change it, but I don't want to add lag by adding it to each frame in Update()
			if (number != 0) {
				print ("[DangIt] [AlarmManager] Adding '" + number.ToString () + "' alarms from '" + fm.ToString () + "'");
				loops.Add (fm, number);
			} else {
				print ("[DangIt] [AlarmManager] No alarms added: Would have added 0 alarms");
			}
		}

		public void Update()
		{
			if (this.GetComponent<AudioSource>() != null) {
				if (!this.GetComponent<AudioSource>().isPlaying){
					if (loops.Count > 0) {
						var element = loops.ElementAt (0);
						loops.Remove (element.Key);
						print ("[DangIt] [AlarmManager] Playing Clip");
                        this.GetComponent<AudioSource>().Play();
						if (element.Value != 0 && element.Value != 1) {
							if (element.Key.vessel == FlightGlobals.ActiveVessel) {
								loops.Add (element.Key, element.Value - 1); //Only re-add if still has alarms
							} else {
								element.Key.AlarmsDoneCallback ();
							}
						} else {
							element.Key.AlarmsDoneCallback ();
						}
					}
				}
			}
		}

		public void RemoveAllAlarmsForModule(FailureModule fm)
		{
			print ("[DangIt] [AlarmManager] Removing alarms...");
			if (this.loops.Keys.Contains (fm))
			{
				fm.AlarmsDoneCallback ();
				loops.Remove (fm);
			}
		}

		public bool HasAlarmsForModule(FailureModule fm)
		{
			if (this.loops.Keys.Contains (fm))
			{
				int i;
				loops.TryGetValue (fm, out i);
				if (i != 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}

