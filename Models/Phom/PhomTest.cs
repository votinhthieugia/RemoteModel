using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhomTest : MonoBehaviour {

	void Start () {
//		string st = "";
//		for (int i = 0; i < 52; i++) {
//			PhomCard pCard = new PhomCard (i);
//			st += " " + pCard.ToString ();
//		}
//		Debug.Log (st);
//
//		PhomCard pc = new PhomCard ("TC");
//		Debug.Log (pc.ToIndex ());

//		PhomCombination comb = new PhomCombination ("AS AH AD 2D 3D 7H 9D TD JD QD KS KH KC");
//		Debug.Log (comb.GetTypeName ());
		PhomCard c = new PhomCard("TC");
		PhomCard c2 = new PhomCard ("4C");
		Debug.Log (c.GetRank () + " " + c2.GetRank ());

		PhomHand hand = new PhomHand ();
		hand.AddCards ("AS AH 2C 2D 3C KS QH KH KC KD");
//		hand.FindCombinations ();
//		hand.Arrange ();
		Debug.Log("Best:" + hand.Best ());
	}
}