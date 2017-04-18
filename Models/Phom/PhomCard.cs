using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// AS = 0   AC = 13    	AD = 26    	AH = 39
// 2S =     2C =     	2D =     	2H = 
// 3S =     3C =     	3D =     	3H = 
// 4S =     4C =     	4D =     	4H = 
// 5S =     5C =     	5D =     	5H = 
// 6S =     6C =     	6D =     	6H = 
// 7S =     7C =     	7D =     	7H = 
// 8S =     8C =     	8D =     	8H = 
// 9S =     9C =     	9D =     	9H = 
// TS =     TC =     	TD =     	TH = 
// JS =     JC =     	JD =     	JH = 
// QS =     QC =     	QD =     	QH = 
// KS =     KC =     	KD =     	KH = 
//

public class PhomCard : Card {
	public enum Rank {
		ACE,
		TWO,
		THREE,
		FOUR,
		FIVE,
		SIX,
		SEVEN,
		EIGHT,
		NINE,
		TEN,
		JACK,
		QUEEN,
		KING
	};

	public bool IsSingle { get; set; }

	public PhomCard(int index) : base(index) {
	}

	public PhomCard(string cardsString) : base(cardsString) {
	}

	public override string GetRankChar(int rank, int offset) {
//		Debug.Log (rank + " " + offset);
		string rankChar = "0";

		switch (rank) {
		case 12: rankChar = "K"; break;
		case 11: rankChar = "Q"; break;
		case 10: rankChar = "J"; break;
		case 9: rankChar = "T"; break;
		case 0: rankChar = "A"; break;
		default: rankChar = (rank + GetRankOffset()).ToString(); break;
		}

		return rankChar;
	}

	public override int GetRankOffset() {
		return 1;
	}

	public override int CharsToIndex(char rank, char suit) {
//		Debug.Log ("CharsToIndex:" + rank + " " + suit);
		int r = INVALID;

		switch (rank) {
		case 'T': r = (int)Rank.TEN; break;
		case 'J': r = (int)Rank.JACK; break;
		case 'Q': r = (int)Rank.QUEEN; break;
		case 'K': r = (int)Rank.KING; break;
		case 'A': r = (int)Rank.ACE; break;
		default: r = (int)rank - 49; break;
		}


		switch (suit) {
		case 'S': r = ToIndex(r, (int)Suit.SPADE); break;
		case 'C': r = ToIndex(r, (int)Suit.CLUB); break;
		case 'D': r = ToIndex(r, (int)Suit.DIAMOND); break;
		case 'H': r = ToIndex(r, (int)Suit.HEART); break;
		}

		return r;
	}


}