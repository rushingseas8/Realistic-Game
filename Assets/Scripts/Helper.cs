using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper {
	public static float[] interpolate(float x1, float x2, float x3, float y1, float y2, float y3) {
		float w3 = ((y3 - y1) - ((x3 - x1) * (y2 - y1) / (x2 - x1))) / (((x3 * x3) - (x1 * x1)) - ((x3 - x1) * (((x2 * x2) - (x1 * x1)) / (x2 - x1))));
		float w2 = ((y2 - y1) / (x2 - x1)) - ((((x2 * x2) - (x1 * x1)) / (x2 - x1)) * w3);
		float w1 = y1 - (x1 * w2) - ((x1 * x1) * w3);
		return new float[] { w1, w2, w3 };
	}
}
