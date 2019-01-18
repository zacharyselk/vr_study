//#define DEBUG
#undef DEBUG

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// game classes inherit from Stimuli to tap into the main framework
public class ToneMemoryOld : Stimuli
{
	#region Private variables

	private int correctAnswers;
	private int wrongAnswers;

	private int toneType;
	private bool didReact;

	private int trigger = 0;
	public static int[] colour = new int[3];

	private int[,] trial_dist = new int[2,250];

	GameObject Target_One;
	GameObject Target_Two;
	GameObject Target_Three;
	GameObject Target_Four;
	GameObject Target_Five;
	GameObject Fixation;
	GameObject Fixation_Cube;
    GameObject Break_Info;
    GameObject User_Info;
    Text User_Info_Text;

    Vector3 Target_One_Pos = new Vector3(-0.2f, 1.45f, -0.162f);
    Vector3 Target_Two_Pos = new Vector3(-0.23f, 1.73f, 3.086f);
    Vector3 Target_Three_Pos = new Vector3(-0.243f, 2.0f, 6.334f);
    Vector3 Target_Four_Pos = new Vector3(-0.263f, 2.277f, 9.582f);
    Vector3 Target_Five_Pos = new Vector3(-0.29f, 2.55f, 12.83f);
    Vector3 User_Info_Pos = new Vector3(-0.5f, 0.5f, 0);
    Vector3 Hide = new Vector3(-0.2f, -1.45f, -0.162f);

    #endregion

    #region State machine

    // UPDATESTATE override ---------------------

    // overriding virtual UpdateState from stimuli
    public override void UpdateState ()
	{

		//QualitySettings.shadows = ShadowQuality.Disable;
		// wipe out any outstanding invokes (threads) from the previous state
		CancelInvoke ();

#if (DEBUG)
        Debug.Log ("Updated state detected.. new state = " + currentState);
#endif

        switch ( currentState )
		{
			case State.TutorialIntroduction: // ------------
				StartTutorialIntroduction ();
				break;

			case State.TrialStart: // ------------
				StartTrial ();
				break;

			case State.Study: // ------------
				StartStudy ();
				break;

			case State.Recall: // ------------
//			Invoke("StartRecall",0.2f);
				StartRecall ();
				break;

			case State.TrialEnd: // ------------
				TrialEnd ();
				break;
		}

		// now that the state has been intialized, we can update previousState
		previousState = currentState;
	}

    // --------------------------------------

    #endregion

    #region Game State Functions
    


	void StartTutorialIntroduction ()
	{
# if (DEBUG)
        Debug.Log ("Start tutorial..");
# endif

        // init tutorial intro things here!!
        _UIManager.ShowUI ("Tutorial");

		// if we are running a tutorial, schedule its end. Otherwise, skip straight through..
		if ( tutorialTime > 0 )
		{
			Invoke ("EndTutorialIntroduction", tutorialTime);
		}
		else
		{
			EndTutorialIntroduction ();
		}
	}

    // Iterates through the different depths showing the user which one is which, with text telling them
    IEnumerator CycleThroughDepths()
    {
        User_Info.transform.localPosition = Hide;
        yield return new WaitForSeconds(1);
        User_Info_Text.text = "Position One";
        User_Info.gameObject.transform.localPosition = User_Info_Pos;
        Target_One.gameObject.transform.localPosition = Target_One_Pos;
        yield return new WaitForSeconds(5);
        Target_One.gameObject.transform.localPosition = Hide;
        yield return new WaitForSeconds(1);
        User_Info_Text.text = "Position Two";
        Target_Two.gameObject.transform.localPosition = Target_Two_Pos;
        yield return new WaitForSeconds(5);
        Target_Two.gameObject.transform.localPosition = Hide;
        yield return new WaitForSeconds(1);
        User_Info_Text.text = "Position Three";
        Target_Three.gameObject.transform.localPosition = Target_Three_Pos;
        yield return new WaitForSeconds(5);
        Target_Three.gameObject.transform.localPosition = Hide;
        yield return new WaitForSeconds(1);
        User_Info_Text.text = "Position Four";
        Target_Four.gameObject.transform.localPosition = Target_Four_Pos;
        yield return new WaitForSeconds(5);
        Target_Four.gameObject.transform.localPosition = Hide;
        yield return new WaitForSeconds(1);
        User_Info_Text.text = "Position Five";
        Target_Five.gameObject.transform.localPosition = Target_Five_Pos;
        yield return new WaitForSeconds(5);
        Target_Five.gameObject.transform.localPosition = Hide;
        User_Info.gameObject.transform.localPosition = Hide;

        Invoke("EndStartTrial", 1.0f);
    }

	void EndTutorialIntroduction ()
	{
		// avoid duplicate calls (if skipped)
		CancelInvoke ("EndTutorialIntroduction");

		_UIManager.HideUI ("Tutorial");

		// no practice trials in this game
		practiceTrials = false;

		SetState (State.TrialStart);
	}

	void StartTrial ()
	{
 
		Debug.Log ("Start trial " + trialNum);

		// lock skip
		skipEnable = false;


        // Initialize counters and flags
        // reset answer counters
        correctAnswers = 0;
		wrongAnswers = 0;

		// reset reaction timer
		reactionTimer = 0;
		didReact = false;

		// show a little ui cue to say we are in practice mode

		if ( !sessionTimerStarted )
		{
			// sessionTimerStarted bool just makes sure we only start the session timer once
			sessionTimerStarted = true;
		}

		_UIManager.HideUI ("Practice");


		// we leave a little pause before starting up, to allow for 'adjustment'
		if (trialNum < 2) {
			//let's associate each of our orbs with variable names
			//so that we can easily manipulate them.
			//we use 5 different orbs, instead of 1, so that we
			//don't have to manipulate the size of our targets,
			//we only have to manipulate the y position
			Target_One = GameObject.Find ("Target_One");
			Target_Two = GameObject.Find ("Target_Two");
			Target_Three = GameObject.Find ("Target_Three");
			Target_Four = GameObject.Find ("Target_Four");
			Target_Five = GameObject.Find ("Target_Five");
            Break_Info = GameObject.Find ("Break_Canvas");
            User_Info = GameObject.Find("User_Info");
            User_Info_Text = User_Info.GetComponentsInChildren<Text>()[0];

			//we will also associate our fixation cross and
			//fixation canvas so we can manipulate them
			Fixation = GameObject.Find ("focuscross");
			Fixation_Cube = GameObject.Find ("Cube");

			//Now let's turn off shadows for our objects
			Target_One.transform.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			Target_Two.transform.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			Target_Three.transform.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			Target_Four.transform.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			Target_Five.transform.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			Fixation_Cube.transform.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			//now we will loop throug the number of distances (5)
			//and the number of trials per distance (50) we will use.
			//we will store our distance and target/standard info
			//in a variable that we can refer to later
			for (var i_dist = 0; i_dist < 5; i_dist++) {
				for (var i_col = 0; i_col < 50; i_col++) {
                    //if (i_col >= 40) {
                    // Set standards to be i_dist = 0
                    if (i_dist > 0) { 
						trial_dist [0, (i_col + (50 * i_dist))] = 2;
						trial_dist [1, (i_col + (50 * i_dist))] = i_dist + 1;
					} else {
						trial_dist [0, (i_col + (50 * i_dist))] = 1;
						trial_dist [1, (i_col + (50 * i_dist))] = i_dist + 1;
					}
				}
			}
			//this loop will go through each of our 250 trials and
			//shuffle the order, basically replacing the current depth/target info
			//of the current trial with a later trial
			for (var i_dist = 0; i_dist < 5; i_dist++) {
				for (var i_col = 0; i_col < 50; i_col++) {
					int temp1 = trial_dist [0, (i_col + (50 * i_dist))];
					int temp2 = trial_dist [1, (i_col + (50 * i_dist))];

					int randomIndex = Random.Range ((i_col + (50 * i_dist)), 250);
					trial_dist [0, (i_col + (50 * i_dist))] = trial_dist [0, randomIndex];
					trial_dist [1, (i_col + (50 * i_dist))] = trial_dist [1, randomIndex];

					trial_dist [0, randomIndex] = temp1;
					trial_dist [1, randomIndex] = temp2;
				}
			}
			//Target_One.gameObject.transform.localPosition = new Vector3 (-0.20f, 0f, 1.7f);
			trigger = 0;
            //Invoke ("EndStartTrial", 10.0f);
            StartCoroutine(CycleThroughDepths());
        } else if (trialNum == 50 || trialNum == 100 || trialNum == 150 || trialNum == 200) {
			trigger = 255;
			Break_Info.gameObject.transform.localPosition = new Vector3 (-0.5f,1.2f,0f);
			Invoke ("EndStartTrial", 30.0f);
		}else if (trialNum > 1) {
			//trigger = 0;
			Invoke ("EndStartTrial", 0.05f);
		}
	}



	void EndStartTrial ()
	{
		if (trialNum == 50 || trialNum == 100 || trialNum == 150 || trialNum == 200) {
			Break_Info.gameObject.transform.localPosition = new Vector3 (-0.5f,-1.2f,0f);
		}
		trigger = 0;
#if (DEBUG)
        Debug.Log ("EndStartTrial() called.");
#endif

        SetState (State.Study);
	}

	private bool soundHasPlayed;

	void StartStudy ()
	{
#if (DEBUG)
        Debug.Log ("Start Study phase!");
#endif

        didReact = false;
		soundHasPlayed = false;
		// play sound
		CancelInvoke ("PlayRandomSound");
		Invoke ("PlayRandomSound", Random.Range (0.95f, 1.45f));

		//_UIManager.ShowUI("Study");
	}

    // No longer in use
	void Change_Colour()
	{
		//let's check if our trial is a standard or target
		//standard
		if (trial_dist [0, trialNum] == 1) {
			//first distance
			if (trial_dist [1, trialNum] == 1) {
				Target_One.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 1, 0, 1);
				trigger = 80;
			} 
			//second distance
			else if (trial_dist [1, trialNum] == 2) {
				Target_Two.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 1, 0, 1);
				trigger = 85;
			} 
			//third distance
			else if (trial_dist [1, trialNum] == 3) {
				Target_Three.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 1, 0, 1);
				trigger = 87;
			} 
			//fourth distance
			else if (trial_dist [1, trialNum] == 4) {
				Target_Four.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 1, 0, 1);
				trigger = 88;
			} 
			//fifth distance
			else if (trial_dist [1, trialNum] == 5) {
				Target_Five.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 1, 0, 1);
				trigger = 89;
			}
		} 
		//target
		else if(trial_dist [0, trialNum] == 2){
			//first distance
			if (trial_dist [1, trialNum] == 1) {
				Target_One.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 1, 1);
				trigger = 90;
			} 
			//second distance
			else if (trial_dist [1, trialNum] == 2) {
				Target_Two.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 1, 1);
				trigger = 91;
			} 
			//third distance
			else if (trial_dist [1, trialNum] == 3) {
				Target_Three.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 1, 1);
				trigger = 92;
			} 
			//fourth distance
			else if (trial_dist [1, trialNum] == 4) {
				Target_Four.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 1, 1);
				trigger = 93;
			} 
			//fifth distance
			else if (trial_dist [1, trialNum] == 5) {
				Target_Five.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 1, 1);
				trigger = 94;
			}
		}

		EndStudy ();
	}

	void EndStudy ()
	{
		//trigger = 0;
		// hide UI
		_UIManager.HideUI ("Study");

		// do anything we need to do when Study mode ends..
		SetState (State.Recall);

		// NOTE that delay mode does not exist in this game!
	}

	void StartRecall ()
	{
		//trigger = 0;
		// we only allow mouse manipulation in the recall state
		EnableInput ();

		// show the UI
		//_UIManager.ShowUI("Recall");

//		Invoke ("EndRecall", lengthOfRecallModeSeconds);
		Invoke ("EndRecall", 1.5f);
	}

	void EndRecall ()
	{
		//let's check if our trial is a standard or target
		//standard
		if (trial_dist [0, trialNum] == 1) {
			//first distance
			if (trial_dist [1, trialNum] == 1) {
				Target_One.gameObject.transform.localPosition = new Vector3 (-0.2f,-1.45f,-0.162f);
				Target_One.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 126;
			} 
			//second distance
			else if (trial_dist [1, trialNum] == 2) {
				Target_Two.gameObject.transform.localPosition = new Vector3 (-0.23f,-1.73f,3.086f);
				Target_Two.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 127; 
			} 
			//third distance
			else if (trial_dist [1, trialNum] == 3) {
				Target_Three.gameObject.transform.localPosition = new Vector3 (-0.243f,-2.0f,6.334f);
				Target_Three.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 128;
			} 
			//fourth distance
			else if (trial_dist [1, trialNum] == 4) {
				Target_Four.gameObject.transform.localPosition = new Vector3 (-0.263f,-2.277f,9.582f);
				Target_Four.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 133;
			} 
			//fifth distance
			else if (trial_dist [1, trialNum] == 5) {
				Target_Five.gameObject.transform.localPosition = new Vector3 (-0.29f,-2.55f,12.83f);
				Target_Five.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 135;
			}
		} 
		//target
		else if(trial_dist [0, trialNum] == 2){
			//first distance
			if (trial_dist [1, trialNum] == 1) {
				Target_One.gameObject.transform.localPosition = new Vector3 (-0.2f,-1.45f,-0.162f);
				Target_One.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 136;
			} 
			//second distance
			else if (trial_dist [1, trialNum] == 2) {
				Target_Two.gameObject.transform.localPosition = new Vector3 (-0.23f,-1.73f,3.086f);
				Target_Two.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 137;
			} 
			//third distance
			else if (trial_dist [1, trialNum] == 3) {
				Target_Three.gameObject.transform.localPosition = new Vector3 (-0.243f,-2.0f,6.334f);
				Target_Three.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 138;
			} 
			//fourth distance
			else if (trial_dist [1, trialNum] == 4) {
				Target_Four.gameObject.transform.localPosition = new Vector3 (-0.263f,-2.277f,9.582f);
				Target_Four.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 139;
			} 
			//fifth distance
			else if (trial_dist [1, trialNum] == 5) {
				Target_Five.gameObject.transform.localPosition = new Vector3 (-0.29f,-2.55f,12.83f);
				Target_Five.gameObject.GetComponent<Renderer> ().material.color = new Color (0, 0, 0, 1);
				trigger = 140;
			}
		}

		//trigger = 0;

		Fixation.gameObject.transform.localPosition = new Vector3 (0.0f, 0.0f, 0.0f);

		// to avoid problems with multiple calls (space bar skip etc.) let's check the current state before doing anything else
		if ( currentState != State.Recall )
			return;

		// hide and stop the fancy clock graphic
		_UIManager.HideUI ("TimeRunningOut");

		// we only allow mouse manipulation in the recall state, so we disable input now
		DisableInput ();

		// clear out the invokes to avoid any duplicate calls if we hit space to skip
		CancelInvoke ("EndRecall");
		CancelInvoke ("ShowTimeRunningOut");

		// do anything we need to do when Recall state ends..
		_UIManager.HideUI ("Recall");

		SetState (State.TrialEnd);
	}


    void TrialEnd ()
	{
        //trigger = 0;
#if (DEBUG)
        Debug.Log ("Trial " + trialNum + " ended.");
#endif
        // write out to the log
        LogTrialData ();

		resetStimNum = false;

		// increase the trial counter
		trialNum++;

		// if we are in practice mode, keep tabs on how many practice trials we've done..
		if ( practiceTrials )
		{
			practiceTrialCounter++;

			// check score to see if we're done with practice mode (trialScore is how many stims we got close to the startpos)
			if ( trialScore >= 1 )
			{
				// cancel out practice mode to move on to the real thing next trial
				practiceTrials = false;

				resetStimNum = true;

				// hide the practice mode indicator
				_UIManager.HideUI ("Practice");

				// show a quick message
				_UIManager.ShowUI ("PracticeCongrats");
			}
		}

		// show a quick message
		_UIManager.HideUI ("PracticeCongrats");
		//trigger = 0;

		// end if we're hitting trial number 250
		if ( trialNum > totalTrials )
		{
			// show a quick message
			_UIManager.ShowUI ("PracticeCongrats");

			// shut down the log
			EndLogSession ();

			// shut down the session
			EndSession ();
		}
		else
		{
			// and restart the state machine to go around again!
			SetState (State.TrialStart);
		}
	}

	public override void EndSession ()
	{
		SetState (State.SessionEnd);

		_UIManager.HideAllUI ();

		// we only allow mouse manipulation in the recall state, so we disable input now
		DisableInput ();

		// shut down the log, if it needs to be shut
		EndLogSession ();

		// clear out ALL the invokes 
		CancelInvoke ();

		// show a message to say that the session is done
		_UIManager.ShowUI ("SessionComplete");

		Invoke ("LoadMenu", 10f);
	}

#endregion

#region Feedback State

	void SetUpFeedback ()
	{

	}

#endregion

#region Main Loop

	void Update ()
	{
		// first, we kee p an eye out for state changes
		if ( previousState != currentState )
			UpdateState ();

		// Space or mouse hit after tone has played..
		if ( !didReact && ( currentState == State.Recall || ( currentState == State.Study && soundHasPlayed ) ) )
		{
			if ( Input.GetKey (KeyCode.Space) || Input.GetMouseButtonUp (0) || triggerHit )
			{
				triggerHit = false;
				didReact = true;

				//let's check if our trial is a standard or target
				//standard
				if (trial_dist [0, trialNum] == 1) {
					//first distance
					if (trial_dist [1, trialNum] == 1) {
						trigger = 95;
					} 
					//second distance
					else if (trial_dist [1, trialNum] == 2) {
						trigger = 112;
					} 
					//third distance
					else if (trial_dist [1, trialNum] == 3) {
						trigger = 117;
					} 
					//fourth distance
					else if (trial_dist [1, trialNum] == 4) {
						trigger = 119;
					} 
					//fifth distance
					else if (trial_dist [1, trialNum] == 5) {
						trigger = 120;
					}
				} 
				//target
				else if(trial_dist [0, trialNum] == 2){
					//first distance
					if (trial_dist [1, trialNum] == 1) {
						trigger = 121;
					} 
					//second distance
					else if (trial_dist [1, trialNum] == 2) {
						trigger = 122;
					} 
					//third distance
					else if (trial_dist [1, trialNum] == 3) {
						trigger = 123;
					} 
					//fourth distance
					else if (trial_dist [1, trialNum] == 4) {
						trigger = 124;
					} 
					//fifth distance
					else if (trial_dist [1, trialNum] == 5) {
						trigger = 125;
					}
				}
#if (DEBUG)
                Debug.Log ("Input reaction at " + reactionTimer);
#endif
                //EndRecall ();
            }
		}

		// I'm allowed SPACE bar skipping during the tutorial introduction..
		if ( currentState == State.TutorialIntroduction )
		{
			if ( Input.GetKey (KeyCode.Space) || Input.GetMouseButtonUp (1) || triggerHit )
			{
				EndTutorialIntroduction ();
				triggerHit = false;
			}
		}

		// update the session time counter and end the session at the right time
		//if (sessionTimerStarted && currentState != State.SessionEnd)
		//{
		//    sessionCounter += Time.deltaTime;
		//if (sessionCounter >= sessionLengthSecs)
		//    EndSession();
		//}

		// update the reaction timer if we are in the right recall state
		if ( !didReact && ( currentState == State.Recall || ( currentState == State.Study && soundHasPlayed ) ) )
		{
			// add on delta time
			reactionTimer += Time.deltaTime;
		}
	}

#endregion

#region General Game Functions

	public override void GrabStarted ( Transform activeStim )
	{
		// when the user tries to grab a stim, we treat that as being picked
		GotStimPick (activeStim);
	}

	public override void TriggerPress ( Transform activeStim )
	{
		// override this to add your game-specific functionality
		triggerHit = true;

		// when the user uses the trigger, we treat that as being picked as well as the grip (just for ease of use)
		GotStimPick (activeStim);
	}

	void GotStimPick ( Transform activeStim )
	{
		// make sure we are in the right state for answer checking before processing the pick
		if ( currentState != State.Recall )
			return;

		// --------------------------------------------
		// RECORD THE RESULT
		// --------------------------------------------

		EndRecall ();
	}

	void PlayRandomSound ()
	{
		Fixation.gameObject.transform.localPosition = new Vector3 (0.0f, -150.0f, 0.0f);
		// play a tone sound (1 in 5 chance of 1500hz tone)
		if ( _audioManager != null )
		{
			//let's check if our trial is a standard or target
			//standard
			if (trial_dist [0, trialNum] == 1) {
				//first distance
				if (trial_dist [1, trialNum] == 1) {
					Target_One.gameObject.transform.localPosition = new Vector3 (-0.2f,1.45f,-0.162f);
					trigger = 69;
				} 
				//second distance
				else if (trial_dist [1, trialNum] == 2) {
					Target_Two.gameObject.transform.localPosition = new Vector3 (-0.23f,1.73f,3.086f);
					trigger = 71;
				} 
				//third distance
				else if (trial_dist [1, trialNum] == 3) {
					Target_Three.gameObject.transform.localPosition = new Vector3 (-0.243f,2.0f,6.334f);
					trigger = 72;
				} 
				//fourth distance
				else if (trial_dist [1, trialNum] == 4) {
					Target_Four.gameObject.transform.localPosition = new Vector3 (-0.263f,2.277f,9.582f);
					trigger = 73;
				} 
				//fifth distance
				else if (trial_dist [1, trialNum] == 5) {
					Target_Five.gameObject.transform.localPosition = new Vector3 (-0.29f,2.55f,12.83f);
					trigger = 74;
				}
			} 
			//target
			else if(trial_dist [0, trialNum] == 2){
				//first distance
				if (trial_dist [1, trialNum] == 1) {
					Target_One.gameObject.transform.localPosition = new Vector3 (-0.2f,1.45f,-0.162f);
					trigger = 75;
				} 
				//second distance
				else if (trial_dist [1, trialNum] == 2) {
					Target_Two.gameObject.transform.localPosition = new Vector3 (-0.23f,1.73f,3.086f);
					trigger = 76;
				} 
				//third distance
				else if (trial_dist [1, trialNum] == 3) {
					Target_Three.gameObject.transform.localPosition = new Vector3 (-0.243f,2.0f,6.334f);
					trigger = 77;
				} 
				//fourth distance
				else if (trial_dist [1, trialNum] == 4) {
					Target_Four.gameObject.transform.localPosition = new Vector3 (-0.263f,2.277f,9.582f);
					trigger = 78;
				} 
				//fifth distance
				else if (trial_dist [1, trialNum] == 5) {
					Target_Five.gameObject.transform.localPosition = new Vector3 (-0.29f,2.55f,12.83f);
					trigger = 79;
				}
			}
		}

		soundHasPlayed = true;
        //Invoke("Change_Colour",Random.Range (1.0f, 1.2f));
        Invoke("EndStudy", Random.Range(0.0f, 0.2f));
	}

	void OnGUI(){
		draw_trig.show_trig (trigger);
	}

#endregion

#region Logging

	public override void StartLogSession ()
	{
		// check data manager to see if logging is enabled
		if ( ( bool )DataManager.instance.GetData ("LogMode") == false )
			return;

		// we will open up the file at the start of the session and write to it at the end of each trial
		// the log file will be closed on exit

		// start up a data file to log with
		_dataWriter.StartDataFile ("ToneTrials");

		// write a header line
		_dataWriter.WriteItem ("Trial\tReaction Time\tToneFreq");
	}

	protected void LogTrialData ()
	{
		// check data manager to see if logging is enabled
		if ( ( bool )DataManager.instance.GetData ("LogMode") == false )
			return;

		string tab = "\t";

		// get the line started
		string writeString = trialNum + tab;

		// add milliseconds reaction time
		float reactionMillis = ( reactionTimer * 1000 );
		string coordNumberFormat = "{0:0.0}";

		if ( didReact )
		{
			writeString = writeString + string.Format (coordNumberFormat, reactionMillis) + tab;
		}
		else
		{
			writeString = writeString + string.Format (coordNumberFormat, 0) + tab;
		}

		writeString = writeString + toneType.ToString ();

		// OK we're all ready to write this line!
		_dataWriter.WriteItem (writeString);

	}

#endregion
}
