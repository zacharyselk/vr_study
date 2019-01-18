using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class draw_trig  {
	private static Texture2D trigger;
	private static GUIStyle trig_style;

    //	private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
    //	private static readonly GUIStyle textureStyle = new GUIStyle{normal = new GUIStyleState{background = backgroundTexture}};
    //	public static byte sum_red;
    //	public static byte sum_green;

    public static void show_trig(int current_trigger) {

        // Old Method
        /*int x = current_trigger;

        string s = System.Convert.ToString(x, 2); char[] bits = s.PadLeft(8, '0').ToCharArray();
        System.Array.Reverse(bits);

        int[] Aint = System.Array.ConvertAll(bits, c => (int)char.GetNumericValue(c));

        int[] pow = new int[] { 16, 32, 64, 128 };

        int[] red_val = new int[] { Aint[0] * pow[0], Aint[1] * pow[1], Aint[2] * pow[2], Aint[3] * pow[3] };
        int[] green_val = new int[] { Aint[4] * pow[0], Aint[5] * pow[1], Aint[6] * pow[2], Aint[7] * pow[3] };

        byte sum_red = System.Convert.ToByte(red_val.Sum());
        byte sum_green = System.Convert.ToByte(green_val.Sum());   
        */

        int redMask = 15;    // 00001111 in binary
        int greenMask = 240; // 11110000 in binary
        int red, green;      // The values of red and green

        red = (current_trigger & redMask) << 4; // Take the first nibble of current_trigger * 16
        green = (current_trigger & greenMask);  // Take the last nibble of current_trigger

        byte sum_red = (byte)red;
        byte sum_green = (byte)green;

	if (trigger == null){
		trigger = new Texture2D (1, 1);
	}
	if (trig_style == null) {
		trig_style = new GUIStyle ();
	}

	//trigger.SetPixel (0, 0, new Color32 ((byte)current_trigger, (byte)current_trigger, (byte)current_trigger, 255));
	trigger.SetPixel (0, 0, new Color32 (sum_red, sum_green, 0, 255));
	trigger.Apply ();
	trig_style.normal.background = trigger;
	GUI.Box (new Rect(0,0,1,1), GUIContent.none,trig_style);
	}

//	public static void DrawRect(GUIContent content = null)
//	{
//		var backgroundColor = GUI.backgroundColor;
//		GUI.backgroundColor = new Color32 (sum_red, sum_green, 0, 255);
//		GUI.Box (new Rect (0, 0, 100, 100), content ?? GUIContent.none, textureStyle);
//		GUI.backgroundColor = backgroundColor;
//	}
//
//	public static void LayoutBox(GUIContent content = null)
//	{
//		var backgroundColor = GUI.backgroundColor;
//		GUI.backgroundColor = new Color32 (sum_red, sum_green, 0, 255);
//		GUILayout.Box (content ?? GUIContent.none, textureStyle);
//		GUI.backgroundColor = backgroundColor;
//	}

}
